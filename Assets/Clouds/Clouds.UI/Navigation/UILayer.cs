using System;
using System.Collections.Generic;
using Clouds.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Clouds.UI
{
    /// <summary>
    /// Lớp gốc của ScreenLayer, PopupLayer, ActivityLayer — một vùng trên Canvas chứa view của đúng một
    /// loại. Thứ tự vẽ giữa các layer là thứ tự trong hierarchy (hoặc Canvas override sorting nếu gắn).
    ///
    /// Phần dùng chung:
    /// - Tự đăng ký theo tên để service tìm được (UILayer.Find).
    /// - Lấy view: đặt sẵn trong scene (con trực tiếp của layer) → pool → nạp prefab qua AssetService.
    /// - Xếp hàng: mọi lệnh Show/Hide/Push/Pop của một layer chạy tuần tự, lệnh gửi tới lúc đang có
    ///   transition sẽ đợi tới lượt chứ không bị bỏ.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class UILayer : MyBehaviour
    {
        [Tooltip("Tên để gọi layer từ service khi có nhiều layer cùng loại. Để trống = tên GameObject.")]
        [SerializeField] private string _layerName;

        private static readonly List<UILayer> _layers = new();

        private readonly Dictionary<string, UIView> _sceneViews = new();
        private readonly Dictionary<string, Stack<UIView>> _pool = new();
        private GameObject _inputBlocker;
        private UniTask _pending = UniTask.CompletedTask;
        private int _runningCount;

        public string LayerName => string.IsNullOrEmpty(_layerName) ? gameObject.name : _layerName;

        /// <summary>Đang có lệnh điều hướng chạy hoặc đang xếp hàng.</summary>
        public bool IsBusy => _runningCount > 0;

        protected static UINavigatorConfig Config => UINavigatorConfig.Current;

        protected override void Awake()
        {
            base.Awake();
            CollectSceneViews();
            _layers.Add(this);
        }

        protected virtual void OnDestroy() => _layers.Remove(this);

        /// <summary>Đóng một view thuộc layer này — mỗi loại layer hiểu "đóng" theo cách riêng.</summary>
        public abstract UniTask CloseAsync(UIView view);

        // ── Tìm layer ────────────────────────────────────────────────────────────

        /// <summary>layerName = null → layer đầu tiên thuộc loại T.</summary>
        public static T Find<T>(string layerName = null) where T : UILayer
        {
            foreach (var layer in _layers)
                if (layer is T typed && (layerName == null || layer.LayerName == layerName)) return typed;

            Debug.LogError(layerName == null
                ? $"[UILayer] Không có {typeof(T).Name} nào trong scene."
                : $"[UILayer] Không có {typeof(T).Name} tên '{layerName}'.");
            return null;
        }

        /// <summary>Mọi layer loại T, layer vẽ trên cùng đứng đầu.</summary>
        public static List<T> AllTopFirst<T>() where T : UILayer
        {
            var result = new List<T>();
            foreach (var layer in _layers)
                if (layer is T typed) result.Add(typed);
            result.Sort((a, b) => CompareDrawOrder(b, a));
            return result;
        }

        // Canvas sortingOrder trước, sau đó tới vị trí trong hierarchy (vẽ sau = nằm trên).
        private static int CompareDrawOrder(UILayer a, UILayer b)
        {
            int bySorting = SortingOrderOf(a).CompareTo(SortingOrderOf(b));
            if (bySorting != 0) return bySorting;

            List<int> pathA = SiblingPath(a.transform), pathB = SiblingPath(b.transform);
            for (int i = 0; i < Mathf.Min(pathA.Count, pathB.Count); i++)
                if (pathA[i] != pathB[i]) return pathA[i].CompareTo(pathB[i]);
            return pathA.Count.CompareTo(pathB.Count);
        }

        private static int SortingOrderOf(UILayer layer)
        {
            Canvas canvas = layer.GetComponentInParent<Canvas>();
            return canvas != null ? canvas.sortingOrder : 0;
        }

        private static List<int> SiblingPath(Transform t)
        {
            var path = new List<int>();
            for (; t != null; t = t.parent) path.Insert(0, t.GetSiblingIndex());
            return path;
        }

        // ── Lấy / trả view ───────────────────────────────────────────────────────

        private void CollectSceneViews()
        {
            foreach (Transform child in transform)
            {
                if (!child.TryGetComponent(out UIView view)) continue;
                view.Key             = view.SceneKey;
                view.Layer           = this;
                view.IsSceneResident = true;
                child.gameObject.SetActive(false);
                _sceneViews[view.Key] = view;
            }
        }

        /// <summary>
        /// View đặt sẵn trong scene nếu có key đó (kể cả khi nó đang hiện — lớp con tự quyết xử lý ra
        /// sao), không thì lấy từ pool, không nữa thì nạp prefab theo key qua AssetService.
        /// </summary>
        protected async UniTask<T> AcquireAsync<T>(string key) where T : UIView
        {
            UIView view;
            if (_sceneViews.TryGetValue(key, out UIView resident))
                view = resident;
            else if (_pool.TryGetValue(key, out Stack<UIView> pooled) && pooled.Count > 0)
                view = pooled.Pop();
            else
                view = await InstantiateAsync(key);

            if (view == null) return null;
            if (view is T typed) return typed;

            Debug.LogError($"[{GetType().Name}] View '{key}' là {view.GetType().Name}, không phải {typeof(T).Name}.");
            Release(view);
            return null;
        }

        private async UniTask<UIView> InstantiateAsync(string key)
        {
            GameObject prefab = await AssetService.LoadAsync<GameObject>(key);
            if (prefab == null) return null;   // AssetService đã log lý do

            GameObject instance = Instantiate(prefab, transform, false);
            if (!instance.TryGetComponent(out UIView view))
            {
                Debug.LogError($"[{GetType().Name}] Prefab '{key}' không có component UIView trên root.");
                Destroy(instance);
                return null;
            }

            instance.SetActive(false);
            view.Key   = key;
            view.Layer = this;
            return view;
        }

        /// <summary>Tắt view rồi cất vào pool, hoặc huỷ nếu pooling tắt. View đặt sẵn trong scene chỉ bị tắt.</summary>
        protected void Release(UIView view)
        {
            if (view == null) return;
            view.gameObject.SetActive(false);
            if (view.IsSceneResident) return;

            if (!Config.EnablePooling)
            {
                Destroy(view.gameObject);
                return;
            }

            if (!_pool.TryGetValue(view.Key, out Stack<UIView> stack)) _pool[view.Key] = stack = new Stack<UIView>();
            stack.Push(view);
        }

        // ── Xếp hàng ─────────────────────────────────────────────────────────────

        protected async UniTask<T> Enqueue<T>(Func<UniTask<T>> operation)
        {
            UniTask previous = _pending;
            var done = new UniTaskCompletionSource();
            _pending = done.Task;
            BeginOperation();

            try
            {
                await previous;
                return await operation();
            }
            finally
            {
                EndOperation();
                done.TrySetResult();
            }
        }

        protected UniTask Enqueue(Func<UniTask> operation)
            => Enqueue(async () => { await operation(); return true; });

        private void BeginOperation()
        {
            _runningCount++;
            if (!Config.InteractableDuringTransition) SetInputBlocked(true);
        }

        private void EndOperation()
        {
            _runningCount--;
            if (_runningCount == 0) SetInputBlocked(false);
        }

        // Chặn bấm bằng một tấm trong suốt phủ trên cùng, KHÔNG dùng CanvasGroup.interactable: tắt
        // interactable làm mọi Button chuyển sang màu disabled, nên mỗi lần popup mở/đóng các nút nháy
        // xám. Tấm chắn có Canvas riêng với sorting cao nhất để luôn nằm trên view vừa SetAsLastSibling,
        // và chặn cả các layer khác — bấm xuyên xuống màn dưới lúc popup đang bay vào cũng là lỗi.
        private void SetInputBlocked(bool blocked)
        {
            if (_inputBlocker == null)
            {
                if (!blocked) return;
                _inputBlocker = new GameObject("[InputBlocker]", typeof(RectTransform), typeof(Canvas),
                                               typeof(GraphicRaycaster), typeof(Image));
                _inputBlocker.transform.SetParent(transform, false);
                UIBackdrop.Stretch((RectTransform)_inputBlocker.transform);
                var canvas = _inputBlocker.GetComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder    = short.MaxValue;
                _inputBlocker.GetComponent<Image>().color = Color.clear;
            }
            _inputBlocker.SetActive(blocked);
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _layers.Clear();
#endif
    }
}
