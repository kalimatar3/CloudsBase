using Clouds.Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Clouds.UI
{
    /// <summary>
    /// Cấu hình chung của hệ điều hướng UI (Screen/Popup/Activity). Nạp qua ConfigService như mọi
    /// config khác: đặt asset vào group Addressables "Game.Config".
    ///
    /// Các UIAnimationData ở đây là transition MẶC ĐỊNH, phát trên root của view. View nào có
    /// UIAnimationSequencer với key "Show"/"Hide" thì dùng key đó thay cho mặc định.
    /// </summary>
    [CreateAssetMenu(fileName = "UINavigatorConfig", menuName = "Clouds/UI/UI Navigator Config")]
    public class UINavigatorConfig : ScriptableObject
    {
        [Title("Popup")]
        public UIAnimationData PopupShow;
        public UIAnimationData PopupHide;

        [Title("Backdrop")]
        [Tooltip("Key Addressables của prefab backdrop. Để trống = backdrop màu đơn tự dựng.")]
        public string BackdropKey;
        [Tooltip("Màu backdrop tự dựng (bỏ qua khi dùng prefab).")]
        public Color BackdropColor = new(0f, 0f, 0f, 0.75f);
        public bool CloseOnBackdropClick = true;
        public UIAnimationData BackdropShow;
        public UIAnimationData BackdropHide;

        [Title("Screen")]
        public UIAnimationData ScreenPushEnter;
        public UIAnimationData ScreenPushExit;
        public UIAnimationData ScreenPopEnter;
        public UIAnimationData ScreenPopExit;

        [Title("Activity")]
        public UIAnimationData ActivityShow;
        public UIAnimationData ActivityHide;

        [Title("Chung")]
        [Tooltip("Giữ lại view đã đóng để lần mở sau khỏi Instantiate lại.")]
        public bool EnablePooling = true;
        [Tooltip("Cho phép bấm nút trong layer khi transition đang chạy.")]
        public bool InteractableDuringTransition;
        [Tooltip("Transition mặc định chạy theo thời gian thực — vẫn mở/đóng được khi game pause.")]
        public bool IgnoreTimeScale = true;

        private static UINavigatorConfig _fallback;

        /// <summary>
        /// Config đang dùng. Chưa nạp (bấm Play thẳng từ scene không qua Bootstrap, hoặc quên đưa asset
        /// vào group Game.Config) thì trả về một bản rỗng — UI vẫn chạy, chỉ là không có transition.
        /// </summary>
        public static UINavigatorConfig Current
        {
            get
            {
                if (ConfigService.IsLoaded<UINavigatorConfig>()) return ConfigService.GetConfig<UINavigatorConfig>();
                if (_fallback != null) return _fallback;

                Debug.LogWarning("[UINavigatorConfig] Chưa nạp config — Popup/Screen/Activity chạy không có transition mặc định. " +
                                 "Đưa asset UINavigatorConfig vào group Addressables \"Game.Config\" và khởi động qua Bootstrap.");
                _fallback = CreateInstance<UINavigatorConfig>();
                return _fallback;
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _fallback = null;
#endif
    }
}
