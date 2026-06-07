using System;
using Clouds.UI.Animation;
using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(SkeletonGraphic))]
public class SpineFurnitureAnimation : MyBehaviour, IUIAnimation
{
    [SerializeField] protected SkeletonGraphic spineanimation;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        spineanimation = GetComponent<SkeletonGraphic>();
        UpdateAnimationSpeed();
    }
    public bool IsPlaying => spineanimation.AnimationState.GetCurrent(0) != null;
    public object NativeAnimation => spineanimation;
    public float Duration => spineanimation.Skeleton.Data.FindAnimation(AnimanName)?.Duration ?? 0f;
    public event Action OnComplete;
    public event Action OnStart;
    public string AnimanName = "ani1";
    public bool IsLoop = false;
    [SerializeField] public float animationSpeed = 1f;

    protected virtual void OnValidate()
    {
        UpdateAnimationSpeed();
    }

    private void UpdateAnimationSpeed()
    {
        if (spineanimation == null) return;
        spineanimation.AnimationState.TimeScale = Mathf.Max(0f, animationSpeed);
    }

    public void Play()
    {
        UpdateAnimationSpeed();
        spineanimation.AnimationState.SetAnimation(0, AnimanName, IsLoop);
    }

    public void Restart()
    {
        UpdateAnimationSpeed();
        spineanimation.AnimationState.SetAnimation(0, AnimanName, IsLoop);
    }
    public void Stop()
    {
        spineanimation.AnimationState.ClearTrack(0);
    }
}
