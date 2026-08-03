using System.Collections.Generic;
using UnityEngine;

namespace Clouds.UI
{
    public static class PopupService
    {
        private static readonly Dictionary<string, GameObject> _popups = new();

        public static void Register(string key, GameObject popup) => _popups[key] = popup;
        public static void Unregister(string key) => _popups.Remove(key);

        public static void Show(string key)
        {
            if (_popups.TryGetValue(key, out var popup)) popup.SetActive(true);
            else Debug.LogWarning($"[PopupService] No popup registered for key '{key}'");
        }

        public static void Hide(string key)
        {
            if (_popups.TryGetValue(key, out var popup)) popup.SetActive(false);
            else Debug.LogWarning($"[PopupService] No popup registered for key '{key}'");
        }

        public static void HideAll()
        {
            foreach (var popup in _popups.Values) popup.SetActive(false);
        }

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _popups.Clear();
#endif
    }
}
