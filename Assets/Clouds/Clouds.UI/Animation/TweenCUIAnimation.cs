using Clouds.Animation;
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
            _animations.AddRange(UIAnimationBuilder.Build(AnimationFactory, UIAnimationData, gameObject, IgnoreTimeScale));
        }
    }
}
