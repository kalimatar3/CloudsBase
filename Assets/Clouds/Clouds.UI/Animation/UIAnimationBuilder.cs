using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Clouds.UI
{
    /// <summary>
    /// Dựng danh sách tween từ một UIAnimationData cho một target UI. Chỗ duy nhất map
    /// TRIGGEREFFECT → factory, dùng chung cho TweenCUIAnimation, UIAnimationContainer và transition
    /// mặc định của hệ điều hướng (Popup/Screen/Activity).
    /// </summary>
    public static class UIAnimationBuilder
    {
        public static List<IUIAnimation> Build(IUIAnimationFactory factory, UIAnimationData data,
                                               RectTransform rect, CanvasGroup canvasGroup, Graphic graphic,
                                               bool ignoreTimeScale = false)
        {
            var list = new List<IUIAnimation>();
            if (data == null || data.Effects == null) return list;

            foreach (var effect in data.Effects)
            {
                IUIAnimation anim = effect.type switch
                {
                    TRIGGEREFFECT.Move   => factory.CreateMove(rect, effect, ignoreTimeScale: ignoreTimeScale),
                    TRIGGEREFFECT.Rotate => factory.CreateRotate(rect, effect, ignoreTimeScale: ignoreTimeScale),
                    TRIGGEREFFECT.Scale  => factory.CreateScale(rect, effect, ignoreTimeScale: ignoreTimeScale),
                    TRIGGEREFFECT.Shake  => factory.CreateShake(rect, effect, ignoreTimeScale: ignoreTimeScale),
                    TRIGGEREFFECT.Punch  => factory.CreatePunch(rect, effect, ignoreTimeScale: ignoreTimeScale),
                    TRIGGEREFFECT.Fade   => canvasGroup != null ? factory.CreateFade(canvasGroup, effect, ignoreTimeScale: ignoreTimeScale) : null,
                    TRIGGEREFFECT.Color  => graphic     != null ? factory.CreateColor(graphic, effect, ignoreTimeScale: ignoreTimeScale)   : null,
                    _                    => null
                };
                if (anim != null) list.Add(anim);
            }
            return list;
        }

        public static List<IUIAnimation> Build(IUIAnimationFactory factory, UIAnimationData data, GameObject target,
                                               bool ignoreTimeScale = false)
            => Build(factory, data, target.GetComponent<RectTransform>(), target.GetComponent<CanvasGroup>(),
                     target.GetComponent<Graphic>(), ignoreTimeScale);
    }
}
