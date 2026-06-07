#if UNITY_EDITOR
using DG.DOTweenEditor;
using DG.Tweening;
using Clouds.UI.Animation;

namespace Clouds.UI.Editor
{
    public class DOTweenPreviewer : IUIAnimationPreviewer
    {
        public void Start() => DOTweenEditorPreview.Start();
        public void Stop() => DOTweenEditorPreview.Stop();

        public void Prepare(IUIAnimation animation)
        {
            if (animation.NativeAnimation is Tween tween)
            {
                DOTweenEditorPreview.PrepareTweenForPreview(tween);
            }
        }
    }
}
#endif