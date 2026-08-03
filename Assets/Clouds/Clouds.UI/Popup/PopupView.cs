using UnityEngine;

namespace Clouds.UI
{
    // Self-registering bridge between a scene popup GameObject and the static PopupService.
    public class PopupView : MonoBehaviour
    {
        [SerializeField] private string key;

        private string Key => string.IsNullOrEmpty(key) ? name : key;

        // Registered in Awake (not OnEnable) so Hide()'s SetActive(false) doesn't unregister the popup.
        private void Awake() => PopupService.Register(Key, gameObject);
        private void OnDestroy() => PopupService.Unregister(Key);
    }
}
