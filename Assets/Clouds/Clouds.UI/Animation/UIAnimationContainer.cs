using System;
using System.Collections.Generic;
using Clouds.Animation;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Clouds.UI
{
    /// <summary>
    /// Gắn component này lên bất kỳ UI GameObject nào để quản lý và phát animation.
    /// Mỗi animation được đặt tên (key). Gọi Play("Show"), Play("Hide"), Play("Click")...
    ///
    /// Ví dụ từ code:
    ///   _animContainer.Play("Show");
    ///   _animContainer.Play("Hide", onComplete: () => gameObject.SetActive(false));
    ///   _animContainer.Play("Click", OnClickStarted, OnClickCompleted);
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIAnimationContainer : MonoBehaviour
    {
        [Serializable]
        public struct AnimationEntry
        {
            [HorizontalGroup(Width = 100), HideLabel]
            public string Key;

            [HorizontalGroup, HideLabel]
            public UIAnimationData Data;
        }

        private static IUIAnimationFactory _factory;
        public static IUIAnimationFactory AnimationFactory
        {
            get
            {
                if (_factory == null) _factory = UISetting.Instance.GetFactory();
                return _factory;
            }
        }

        [ListDrawerSettings(ShowIndexLabels = false, DraggableItems = true)]
        [SerializeField] private List<AnimationEntry> _entries = new();

        private readonly Dictionary<string, List<IUIAnimation>> _runtime = new();

        private RectTransform _rect;
        private CanvasGroup   _canvasGroup;
        private Graphic       _graphic;

        /// <summary>Trạng thái target trước lần phát gần nhất, do AnimationService ghi. Xem Restore().</summary>
        [NonSerialized] public AnimationSnapshot Snapshot;

        public IReadOnlyList<AnimationEntry> Entries => _entries;

        private void Awake() => Build();

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Phát animation theo key.
        /// onStart: gọi khi animation đầu tiên bắt đầu.
        /// onComplete: gọi khi animation cuối cùng kết thúc.
        /// Nếu key không tồn tại, cả hai callback đều gọi ngay.
        /// </summary>
        public void Play(string key, Action onStart = null, Action onComplete = null)
            => AnimationService.Play(this, key, onStart, onComplete);

        internal void PlayInternal(string key, Action onStart, Action onComplete)
        {
            if (!_runtime.TryGetValue(key, out var anims) || anims.Count == 0)
            {
                onStart?.Invoke();
                onComplete?.Invoke();
                return;
            }

            if (onStart    != null) HookOneShot(anims[0],    isStart: true,  callback: onStart);
            if (onComplete != null) HookOneShot(anims[^1], isStart: false, callback: onComplete);

            foreach (var anim in anims) anim.Restart();
        }

        public void Stop(string key)
        {
            if (_runtime.TryGetValue(key, out var anims))
                foreach (var a in anims) a.Stop();
        }

        public void StopAll()
        {
            foreach (var kvp in _runtime)
                foreach (var a in kvp.Value) a.Stop();
        }

        public bool HasKey(string key) => _runtime.ContainsKey(key);

        /// <summary>Lấy danh sách IUIAnimation đã build cho key (dùng bởi Editor preview).</summary>
        public IReadOnlyList<IUIAnimation> GetAnimations(string key)
        {
            if (_runtime.TryGetValue(key, out var list)) return list;
            return Array.Empty<IUIAnimation>();
        }

        /// <summary>Build lại toàn bộ runtime animations. Gọi trong Awake và khi data thay đổi.</summary>
        public void Rebuild() => Build();

        // ── Internal ──────────────────────────────────────────────────────────────

        private void Build()
        {
            _runtime.Clear();
            _rect        = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _graphic     = GetComponent<Graphic>();

            foreach (var entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Key) || entry.Data == null) continue;
                _runtime[entry.Key] = BuildAnimations(entry.Data, _rect, _canvasGroup, _graphic);
            }
        }

        internal UIAnimationData DataFor(string key)
        {
            foreach (var entry in _entries)
                if (entry.Key == key) return entry.Data;
            return null;
        }

        /// <summary>
        /// Dựng lại tween của đúng một key, các key khác giữ nguyên. AnimationService gọi trước khi
        /// phát preset dùng "From = giá trị hiện tại".
        /// </summary>
        internal void RebuildKey(string key)
        {
            UIAnimationData data = DataFor(key);
            if (data == null) return;

            if (_runtime.TryGetValue(key, out var previous))
                foreach (var a in previous) a.Stop();

            _runtime[key] = BuildAnimations(data, _rect, _canvasGroup, _graphic);
        }

        private static List<IUIAnimation> BuildAnimations(
            UIAnimationData data, RectTransform rt, CanvasGroup cg, Graphic graphic)
            => UIAnimationBuilder.Build(AnimationFactory, data, rt, cg, graphic);

        private static void HookOneShot(IUIAnimation target, bool isStart, Action callback)
        {
            if (isStart)
            {
                Action wrapper = null;
                wrapper = () => { callback(); target.OnStart -= wrapper; };
                target.OnStart += wrapper;
            }
            else
            {
                Action wrapper = null;
                wrapper = () => { callback(); target.OnComplete -= wrapper; };
                target.OnComplete += wrapper;
            }
        }
    }
}
