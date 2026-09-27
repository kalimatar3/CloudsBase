using Clouds.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Clouds.UI
{
    /// <summary>Nút đóng view chứa nó (Popup đóng, Screen pop, Activity ẩn). Không cần viết code.</summary>
    [RequireComponent(typeof(Button))]
    public class UICloseButton : MyBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private UIView _view;

        protected override void LoadComponents()
        {
            base.LoadComponents();
            if (_button == null) _button = GetComponent<Button>();
            if (_view == null) _view = GetComponentInParent<UIView>(true);
        }

        private void OnEnable()  => _button.onClick.AddListener(Close);
        private void OnDisable() => _button.onClick.RemoveListener(Close);

        private void Close()
        {
            if (_view != null) _view.CloseAsync().Forget();
        }
    }
}
