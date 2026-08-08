using Clouds.UI;
using UnityEngine;

namespace Clouds.Animation
{
    /// <summary>
    /// Bản world-space của IUIAnimationFactory. Tách riêng chứ không dùng chung vì target khác kiểu:
    /// Transform không có DOAnchorPos, và world không có CanvasGroup/Graphic để fade hay đổi màu.
    /// </summary>
    public interface IWorldAnimationFactory
    {
        IUIAnimation CreateMove(Transform target, UIEffectData effect, bool ignoreTimeScale = false);
        IUIAnimation CreateRotate(Transform target, UIEffectData effect, bool ignoreTimeScale = false);
        IUIAnimation CreateScale(Transform target, UIEffectData effect, bool ignoreTimeScale = false);
        IUIAnimation CreateShake(Transform target, UIEffectData effect, bool ignoreTimeScale = false);
        IUIAnimation CreatePunch(Transform target, UIEffectData effect, bool ignoreTimeScale = false);
        IUIAnimation CreateFade(Renderer renderer, UIEffectData effect, bool ignoreTimeScale = false);
        IUIAnimation CreateColor(Renderer renderer, UIEffectData effect, bool ignoreTimeScale = false);
    }
}
