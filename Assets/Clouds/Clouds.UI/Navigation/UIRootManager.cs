using System;
using System.Collections.Generic;
using Clouds.Common;
using Clouds.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Clouds.UI
{
    /// <summary>
    /// Gốc UI của cả game — tương đương UIController (UnityScreenNavigatorLauncher) bên CartSort. Đặt
    /// trong scene Bootstrap: dựng các layer theo danh sách trong Inspector, mỗi layer một Canvas với
    /// sorting order riêng, rồi DontDestroyOnLoad để ScreenService/PopupService/ActivityService dùng được
    /// ở mọi scene load sau đó.
    ///
    /// Scene nào khác cũng đặt một bản (để bấm Play thẳng từ scene đó khi test) thì bản đó tự huỷ nếu
    /// root từ Bootstrap đã sống sẵn — luôn chỉ có một bộ layer.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    [RequireComponent(typeof(UIBackKeyListener))]
    public class UIRootManager : MyBehaviour
    {
        public enum LayerType { Screen, Popup, Activity }

        [Serializable]
        public class LayerConfig
        {
            [Tooltip("Tên GameObject của layer — cũng là tên truyền vào tham số layer của service.")]
            public string Name;
            public LayerType Type;
            [Tooltip("Sorting order của Canvas riêng của layer. Lớn hơn = nằm trên.")]
            public int SortingOrder;

            public LayerConfig(string name, LayerType type, int sortingOrder)
            {
                Name = name;
                Type = type;
                SortingOrder = sortingOrder;
            }
        }

        [Tooltip("Sống qua các lần load scene. Tắt khi mỗi scene tự có UI root riêng.")]
        [SerializeField] private bool _persistAcrossScenes = true;

        [Tooltip("Tạo EventSystem (con của root, sống cùng root) nếu game chưa có cái nào. " +
                 "Bật thì các scene load sau Bootstrap không cần EventSystem riêng.")]
        [SerializeField] private bool _createEventSystem = true;

        [Tooltip("Tạo theo thứ tự này. Layer con có sẵn cùng tên thì giữ nguyên, không tạo lại.")]
        [SerializeField] private List<LayerConfig> _layers = new()
        {
            new LayerConfig("Screens",    LayerType.Screen,   100),
            new LayerConfig("Popups",     LayerType.Popup,    200),
            new LayerConfig("Activities", LayerType.Activity, 300),
        };

        private static UIRootManager _persistent;

        protected override void Awake()
        {
            if (_persistent != null)
            {
                // Tắt trước khi huỷ: Destroy chỉ có hiệu lực cuối frame, trong lúc đó layer con (nếu đặt
                // tay) vẫn kịp Awake và đăng ký, service có thể tìm nhầm vào bộ layer sắp chết.
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            base.Awake();
            SetupCanvas();
            if (_createEventSystem) EnsureEventSystem();
            BuildLayers();

            if (_persistAcrossScenes)
            {
                _persistent = this;
                if (transform.parent != null) transform.SetParent(null, false);   // DontDestroyOnLoad chỉ nhận object gốc
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_persistent == this) _persistent = null;
        }

        // Gắn component trong Editor: RequireComponent thêm CanvasScaler ở chế độ Constant Pixel Size
        // mặc định — UI dựng cho 1080×1920 sẽ to/nhỏ theo độ phân giải thật. Đặt sẵn cấu hình ở đây.
        protected override void Reset()
        {
            base.Reset();
            if (TryGetComponent(out CanvasScaler scaler)) ConfigureScaler(scaler);
        }

        // RequireComponent chỉ tự thêm khi gắn component trong Editor; root chép YAML vào scene mà thiếu
        // component thì phải tự bổ sung ở đây.
        private void SetupCanvas()
        {
            if (!TryGetComponent(out Canvas canvas)) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (!TryGetComponent(out CanvasScaler _)) ConfigureScaler(gameObject.AddComponent<CanvasScaler>());

            gameObject.GetOrAddComponent<GraphicRaycaster>();
            gameObject.GetOrAddComponent<UIBackKeyListener>();
        }

        private static void ConfigureScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 0.5f;
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            go.transform.SetParent(transform, false);
        }

        private void BuildLayers()
        {
            foreach (LayerConfig config in _layers)
            {
                if (string.IsNullOrEmpty(config.Name)) continue;

                Transform existing = transform.Find(config.Name);
                if (existing != null && existing.GetComponent<UILayer>() != null) continue;

                var go = new GameObject(config.Name, typeof(RectTransform));
                go.transform.SetParent(transform, false);
                UIBackdrop.Stretch((RectTransform)go.transform);

                // Canvas riêng cho từng layer: thứ tự vẽ theo sorting order chứ không phụ thuộc thứ tự
                // con, và HUD của scene gameplay (Canvas sorting 0) luôn nằm dưới UI điều hướng.
                var canvas = go.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder    = config.SortingOrder;
                go.AddComponent<GraphicRaycaster>();

                switch (config.Type)
                {
                    case LayerType.Screen:   go.AddComponent<ScreenLayer>();   break;
                    case LayerType.Popup:    go.AddComponent<PopupLayer>();    break;
                    case LayerType.Activity: go.AddComponent<ActivityLayer>(); break;
                }
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _persistent = null;
#endif
    }
}
