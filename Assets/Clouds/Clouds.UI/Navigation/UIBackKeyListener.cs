using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Clouds.UI
{
    /// <summary>
    /// Nút Back Android / phím Esc: đóng popup trên cùng, không còn popup thì pop màn hình. Gắn một bản
    /// duy nhất trong scene (thường trên Canvas gốc).
    /// </summary>
    public class UIBackKeyListener : MonoBehaviour
    {
        private void Update()
        {
            if (!BackPressed()) return;
            if (!PopupService.HandleBack()) ScreenService.HandleBack();
        }

        // Input System map nút Back của Android vào phím Escape, nên một nhánh là đủ cho cả hai.
        private static bool BackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
