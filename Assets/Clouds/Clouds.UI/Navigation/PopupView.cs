using Sirenix.OdinInspector;
using UnityEngine;

namespace Clouds.UI
{
    /// <summary>
    /// Popup (modal) — nằm trong PopupLayer, xếp chồng, mỗi popup có backdrop riêng. Dùng thẳng cho popup
    /// không có logic riêng (chỉ có UICloseButton), hoặc kế thừa để thêm logic:
    ///
    ///   public class ConfirmPopup : PopupView, IViewArgs&lt;ConfirmArgs&gt; { ... }
    /// </summary>
    public class PopupView : UIView
    {
        [Title("Popup")]
        [Tooltip("Nút Back/Esc đóng popup này. Tắt cho popup bắt buộc (force update…) — Back vẫn bị popup chặn lại.")]
        [SerializeField] private bool _closeOnBack = true;

        [SerializeField] private bool _useBackdrop = true;

        [Tooltip("Bật để dùng màu và hành vi bấm backdrop riêng thay cho UINavigatorConfig.")]
        [SerializeField, ShowIf(nameof(_useBackdrop))] private bool _overrideBackdrop;

        [SerializeField, ShowIf("@_useBackdrop && _overrideBackdrop")]
        private Color _backdropColor = new(0f, 0f, 0f, 0.75f);

        [SerializeField, ShowIf("@_useBackdrop && _overrideBackdrop")]
        private bool _closeOnBackdropClick = true;

        public bool CloseOnBack => _closeOnBack;
        public bool UseBackdrop => _useBackdrop;

        internal Color BackdropColor(UINavigatorConfig config)
            => _overrideBackdrop ? _backdropColor : config.BackdropColor;

        internal bool CloseOnBackdropClick(UINavigatorConfig config)
            => _overrideBackdrop ? _closeOnBackdropClick : config.CloseOnBackdropClick;
    }
}
