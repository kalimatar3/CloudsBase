using Clouds.UI;
using UnityEngine;

namespace Clouds.Animation
{
    /// <summary>
    /// Bản world-space của TweenCUIAnimation: cùng UIAnimationData, cùng khung loop/callback, chỉ khác
    /// chỗ tween được gắn vào — Transform + Renderer thay cho RectTransform + CanvasGroup/Graphic.
    ///
    /// Preset UIAnimationData viết cho UI KHÔNG dùng lại được ở đây: Offset/ShakeStrength/PunchDirection
    /// của UI tính bằng pixel canvas, còn world tính bằng mét — cùng một con số lệch nhau cả trăm lần.
    /// </summary>
    public class TweenWorldAnimation : TweenAnimationBase
    {
        [SerializeField] private Renderer targetRenderer;

        private static IWorldAnimationFactory _factory;
        public static IWorldAnimationFactory AnimationFactory
        {
            get => _factory ??= new DOTweenWorldAnimationFactory();
            set => _factory = value;
        }

        protected override void LoadComponents()
        {
            base.LoadComponents();
            if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
        }

        protected override void BuildAnimations()
        {
            if (UIAnimationData == null) return;

            foreach (var effect in UIAnimationData.Effects)
            {
                IUIAnimation anim = effect.type switch
                {
                    TRIGGEREFFECT.Move   => AnimationFactory.CreateMove(transform, effect),
                    TRIGGEREFFECT.Rotate => AnimationFactory.CreateRotate(transform, effect),
                    TRIGGEREFFECT.Scale  => AnimationFactory.CreateScale(transform, effect),
                    TRIGGEREFFECT.Shake  => AnimationFactory.CreateShake(transform, effect),
                    TRIGGEREFFECT.Punch  => AnimationFactory.CreatePunch(transform, effect),
                    TRIGGEREFFECT.Fade   => targetRenderer != null ? AnimationFactory.CreateFade(targetRenderer, effect)  : null,
                    TRIGGEREFFECT.Color  => targetRenderer != null ? AnimationFactory.CreateColor(targetRenderer, effect) : null,
                    _                    => null
                };
                if (anim != null) _animations.Add(anim);
            }
        }
    }
}
