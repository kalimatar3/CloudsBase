using Clouds.UI.Animation;

namespace Clouds.UI.Editor
{
    public interface IUIAnimationPreviewer
    {
        void Start();
        void Stop();
        void Prepare(IUIAnimation animation);
    }
}