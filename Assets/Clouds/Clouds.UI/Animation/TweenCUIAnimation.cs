using Clouds.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace Clouds.UI
{
    public class TweenCUIAnimation : TweenAnimationBase
    {
        private static IUIAnimationFactory _factory;
        public static IUIAnimationFactory AnimationFactory
        {
            get
            {
                if (_factory == null) _factory = UISetting.Instance.GetFactory();
                return _factory;
            }
        }

        protected override void BuildAnimations()
        {
            if (UIAnimationData == null) return;

            var rt      = GetComponent<RectTransform>();
            var cg      = GetComponent<CanvasGroup>();
            var graphic = GetComponent<Graphic>();

            foreach (var effect in UIAnimationData.Effects)
            {
                IUIAnimation anim = effect.type switch
                {
                    TRIGGEREFFECT.Move   => AnimationFactory.CreateMove(rt, effect),
                    TRIGGEREFFECT.Rotate => AnimationFactory.CreateRotate(rt, effect),
                    TRIGGEREFFECT.Scale  => AnimationFactory.CreateScale(rt, effect),
                    TRIGGEREFFECT.Shake  => AnimationFactory.CreateShake(rt, effect),
                    TRIGGEREFFECT.Punch  => AnimationFactory.CreatePunch(rt, effect),
                    TRIGGEREFFECT.Fade   => cg      != null ? AnimationFactory.CreateFade(cg, effect)       : null,
                    TRIGGEREFFECT.Color  => graphic != null ? AnimationFactory.CreateColor(graphic, effect) : null,
                    _                    => null
                };
                if (anim != null) _animations.Add(anim);
            }
        }
    }
}
