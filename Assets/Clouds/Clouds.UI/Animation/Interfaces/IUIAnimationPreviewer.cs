namespace Clouds.UI
{
    public interface IUIAnimationPreviewer
    {
        void Start();
        void Stop();
        void Prepare(IUIAnimation animation);
    }
}
