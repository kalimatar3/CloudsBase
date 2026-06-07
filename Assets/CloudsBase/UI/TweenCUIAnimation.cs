using System;
using System.Collections.Generic;
using Clouds.UI.Animation;
using Clouds.UI.Settings;
using Clouds.Ultilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TweenCUIAnimation : MyBehaviour, IUIAnimation
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
    public UIAnimationData UIAnimationData;
    protected List<IUIAnimation> animations = new List<IUIAnimation>();
    public bool IsPlaying => throw new NotImplementedException();

    public object NativeAnimation => UIAnimationData;

    public float Duration => UIAnimationData.GetTotalDuration();

    public event Action OnComplete;
    public event Action OnStart;
    protected override void Awake()
    {
        base.Awake();
        this.CreateUIAnimations();
    }
    [Sirenix.OdinInspector.Button(ButtonSizes.Large)]
    public void Play()
    {
        foreach (var anim in animations)
        {
            anim.Play();
        }
    }

    public void Restart()
    {
    }

    public void Stop()
    {
        
    }
    [Sirenix.OdinInspector.Button(ButtonSizes.Large)]
    public void CreateUIAnimations()
    {
        if (animations != null && animations.Count > 0) return;
        animations = new List<IUIAnimation>();
        RectTransform rt = this.GetComponent<RectTransform>();
        CanvasGroup cg = this.GetComponent<CanvasGroup>();
        foreach (UIEffectData effect in UIAnimationData.Effects)
        {
            IUIAnimation anim = null;
            switch (effect.type)
            {
                case TRIGGEREFFECT.Move:
                    anim = AnimationFactory.CreateMove(rt, effect);
                    break;
                case TRIGGEREFFECT.Rotate:
                    anim = AnimationFactory.CreateRotate(rt, effect);
                    break;
                case TRIGGEREFFECT.Scale:
                    anim = AnimationFactory.CreateScale(rt, effect);
                    break;
                case TRIGGEREFFECT.Shake:
                    anim = AnimationFactory.CreateShake(rt, effect);
                    break;
                case TRIGGEREFFECT.Punch:
                    anim = AnimationFactory.CreatePunch(rt, effect);
                    break;
                case TRIGGEREFFECT.Fade:
                    if (cg != null) anim = AnimationFactory.CreateFade(cg, effect);
                    break;
                case TRIGGEREFFECT.Color:
                    Graphic graphic = this.GetComponent<Graphic>();
                    if (graphic != null) anim = AnimationFactory.CreateColor(graphic, effect);
                    break;
            }
            if (anim != null) animations.Add(anim);
        }
    }
}
