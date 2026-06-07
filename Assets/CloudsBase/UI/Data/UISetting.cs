using UnityEngine;
using Clouds.UI.Animation;
using Clouds.UI.Editor;

namespace Clouds.UI.Settings
{
    public enum UIAnimationProvider { DOTween, LitMotion }

    [CreateAssetMenu(fileName = "UISetting", menuName = "Clouds/UI/UISetting")]
    public class UISetting : ScriptableObject
    {
        public UIAnimationProvider provider;

        // Singleton-like access for runtime
        private static UISetting _instance;
        public static UISetting Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<UISetting>("UISetting");
                return _instance;
            }
        }

        public IUIAnimationFactory GetFactory()
        {
            switch (provider)
            {
                case UIAnimationProvider.DOTween:
                    return new DOTweenAnimationFactory();
                case UIAnimationProvider.LitMotion:
                    // return new LitMotionAnimationFactory(); // Se thuc thi sau
                    return new DOTweenAnimationFactory();
                default:
                    return new DOTweenAnimationFactory();
            }
        }
    }
}