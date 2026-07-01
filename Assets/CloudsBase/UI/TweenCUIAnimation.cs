using System;
using System.Collections.Generic;
using Clouds.UI.Animation;
using Clouds.UI.Settings;
using Clouds.Ultilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(RectTransform))]
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

    private readonly List<IUIAnimation> _animations = new();

    public bool IsPlaying
    {
        get { foreach (var a in _animations) if (a.IsPlaying) return true; return false; }
    }

    public object NativeAnimation => _animations.Count > 0 ? _animations[0].NativeAnimation : null;

    public float Duration => UIAnimationData != null ? UIAnimationData.GetTotalDuration() : 0f;

    public event Action OnComplete;
    public event Action OnStart;

    private int  _completedLoops;
    private bool _isReverseCycle;

    protected override void Awake()
    {
        base.Awake();
        Build();
    }

    [Button(ButtonSizes.Large)]
    public void Play()
    {
        if (_animations.Count == 0) return;
        _completedLoops  = 0;
        _isReverseCycle  = false;
        HookOneShot(_animations[0], isStart: true, () => OnStart?.Invoke());
        PlayCycle();
    }

    private void PlayCycle()
    {
        HookOneShot(_animations[^1], isStart: false, OnCycleComplete);
        if (_isReverseCycle)
            foreach (var anim in _animations) anim.PlayReverse();
        else
            foreach (var anim in _animations) anim.Restart();
    }

    private void OnCycleComplete()
    {
        _completedLoops++;
        bool shouldLoop = UIAnimationData != null && UIAnimationData.Loop &&
                          (UIAnimationData.LoopCount <= 0 || _completedLoops < UIAnimationData.LoopCount);

        if (shouldLoop)
        {
            if (UIAnimationData.LoopMode == LoopType.Yoyo) _isReverseCycle = !_isReverseCycle;
            PlayCycle();
        }
        else
            OnComplete?.Invoke();
    }

    public void PlayReverse()
    {
        if (_animations.Count == 0) return;
        _completedLoops = 0;
        _isReverseCycle = true;
        HookOneShot(_animations[0],  isStart: true,  () => OnStart?.Invoke());
        HookOneShot(_animations[^1], isStart: false, () => OnComplete?.Invoke());
        foreach (var anim in _animations) anim.PlayReverse();
    }

    public void Stop()
    {
        foreach (var anim in _animations) anim.Stop();
    }

    public void Restart() => Play();

    private void Build()
    {
        _animations.Clear();
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

    private static void HookOneShot(IUIAnimation target, bool isStart, Action callback)
    {
        if (isStart)
        {
            void Wrapper() { callback(); target.OnStart -= Wrapper; }
            target.OnStart += Wrapper;
        }
        else
        {
            void Wrapper() { callback(); target.OnComplete -= Wrapper; }
            target.OnComplete += Wrapper;
        }
    }
}
