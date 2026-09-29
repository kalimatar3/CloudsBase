using Clouds.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
    /// <summary>Màn chơi test: chỉ có thanh trên cùng với nút mở SettingPopup.</summary>
    public class IngameScreen : ScreenView
    {
        [SerializeField] private Button _settingsButton;

        private void OnEnable()  => _settingsButton.onClick.AddListener(OpenSettings);
        private void OnDisable() => _settingsButton.onClick.RemoveListener(OpenSettings);

        private void OpenSettings() => PopupService.ShowAsync<SettingPopup>().Forget();
    }
}
