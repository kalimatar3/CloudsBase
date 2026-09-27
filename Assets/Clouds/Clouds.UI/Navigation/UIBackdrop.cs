using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clouds.UI
{
    /// <summary>
    /// Nền tối nằm ngay dưới một popup: chặn chạm xuống layer bên dưới và báo Clicked khi bị bấm.
    /// Prefab backdrop riêng (UINavigatorConfig.BackdropKey) cần một Graphic có Raycast Target trên root.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class UIBackdrop : MonoBehaviour, IPointerClickHandler
    {
        public event Action Clicked;

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke();

        internal static UIBackdrop CreateDefault(Transform parent, Color color)
        {
            var go = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                                    typeof(CanvasGroup), typeof(UIBackdrop));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<UIBackdrop>();
        }

        internal static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal UniTask PlayAsync(bool show, UIAnimationData data, bool ignoreTimeScale, CancellationToken token)
        {
            if (show) GetComponent<CanvasGroup>().alpha = 1f;
            return UITransition.PlayAsync(data, gameObject, ignoreTimeScale, token);
        }
    }
}
