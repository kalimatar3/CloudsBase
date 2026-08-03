using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using Clouds.Common;

namespace Clouds.UI
{
    [System.Serializable]
    public struct UIEffectData
    {
        public TRIGGEREFFECT type;

        // ── Type-specific (hiện ngay sau khi chọn type) ──────────────────────

        [BoxGroup("Move")] [ShowIf("type", TRIGGEREFFECT.Move)]
        public MOVEEFFECT MoveType;
        [BoxGroup("Move")] [ShowIf("type", TRIGGEREFFECT.Move)]
        public Vector3 Offset;

        [BoxGroup("Rotate")] [ShowIf("type", TRIGGEREFFECT.Rotate)]
        public Vector3 RotateTo;

        [BoxGroup("Scale")] [ShowIf("type", TRIGGEREFFECT.Scale)]
        [HorizontalGroup("Scale/Range"), LabelText("From"), LabelWidth(36)] public Vector3 ScaleFrom;
        [BoxGroup("Scale")] [ShowIf("type", TRIGGEREFFECT.Scale)]
        [HorizontalGroup("Scale/Range"), LabelText("To"),   LabelWidth(22)] public Vector3 ScaleTo;


        [BoxGroup("Shake")] [ShowIf("type", TRIGGEREFFECT.Shake)]
        [HorizontalGroup("Shake/Mode"), LabelText("Position"), LabelWidth(58)] public bool ShakePosition;
        [BoxGroup("Shake")] [ShowIf("type", TRIGGEREFFECT.Shake)]
        [HorizontalGroup("Shake/Mode"), LabelText("Rotation"), LabelWidth(58)] public bool ShakeRotation;
        [BoxGroup("Shake")] [ShowIf("type", TRIGGEREFFECT.Shake)]
        [HorizontalGroup("Shake/Mode"), LabelText("Scale"),    LabelWidth(42)] public bool ShakeScale;
        [BoxGroup("Shake")] [ShowIf("type", TRIGGEREFFECT.Shake)]
        [HorizontalGroup("Shake/Params"), LabelText("Strength"),   LabelWidth(60)] public float ShakeStrength;
        [BoxGroup("Shake")] [ShowIf("type", TRIGGEREFFECT.Shake)]
        [HorizontalGroup("Shake/Params"), LabelText("Randomness"), LabelWidth(78)] public float ShakeRandomness;
        [BoxGroup("Shake")] [ShowIf("type", TRIGGEREFFECT.Shake)]
        public int ShakeVibrato;

        [BoxGroup("Punch")] [ShowIf("type", TRIGGEREFFECT.Punch)]
        [HorizontalGroup("Punch/Mode"), LabelText("Position"), LabelWidth(58)] public bool PunchPosition;
        [BoxGroup("Punch")] [ShowIf("type", TRIGGEREFFECT.Punch)]
        [HorizontalGroup("Punch/Mode"), LabelText("Rotation"), LabelWidth(58)] public bool PunchRotation;
        [BoxGroup("Punch")] [ShowIf("type", TRIGGEREFFECT.Punch)]
        [HorizontalGroup("Punch/Mode"), LabelText("Scale"),    LabelWidth(42)] public bool PunchScale;
        [BoxGroup("Punch")] [ShowIf("type", TRIGGEREFFECT.Punch)]
        public Vector2 PunchDirection;
        [BoxGroup("Punch")] [ShowIf("type", TRIGGEREFFECT.Punch)]
        [HorizontalGroup("Punch/Params"), LabelText("Vibrato"),    LabelWidth(54)] public int PunchVibrato;
        [BoxGroup("Punch")] [ShowIf("type", TRIGGEREFFECT.Punch)]
        [HorizontalGroup("Punch/Params"), LabelText("Elasticity"), LabelWidth(70)] public float PunchElasticity;

        [BoxGroup("Fade")] [ShowIf("type", TRIGGEREFFECT.Fade)]
        [HorizontalGroup("Fade/Range"), LabelText("From"), LabelWidth(36), Range(0, 1)] public float FadeFrom;
        [BoxGroup("Fade")] [ShowIf("type", TRIGGEREFFECT.Fade)]
        [HorizontalGroup("Fade/Range"), LabelText("To"),   LabelWidth(22), Range(0, 1)] public float FadeTo;

        [BoxGroup("Color")] [ShowIf("type", TRIGGEREFFECT.Color)]
        [HorizontalGroup("Color/Range"), LabelText("From"), LabelWidth(36)] public Color ColorFrom;
        [BoxGroup("Color")] [ShowIf("type", TRIGGEREFFECT.Color)]
        [HorizontalGroup("Color/Range"), LabelText("To"),   LabelWidth(22)] public Color ColorTo;

        // ── Timing ───────────────────────────────────────────────────────────
        [Title("Timing")]
        [HorizontalGroup("Timing"), LabelWidth(42)] public float Delay;
        [HorizontalGroup("Timing"), LabelWidth(60)] public float Duration;

        // ── Ease ─────────────────────────────────────────────────────────────
        [Title("Ease")]
        public Ease EaseType;
        [ShowIf("@EaseType == Clouds.UI.Ease.Custom")]
        public AnimationCurve Curve;

        // ── Loop ─────────────────────────────────────────────────────────────
        [Title("Loop")]
        [HorizontalGroup("Loop"), LabelWidth(38)] public bool Loop;
        [HorizontalGroup("Loop"), ShowIf("Loop"), LabelText("Mode"), LabelWidth(42)] public LoopType LoopType;
        [HorizontalGroup("Loop"), ShowIf("Loop"), LabelText("Count"), LabelWidth(44)] public int LoopCount;

        // ── List label (dùng bởi UIAnimationData ListDrawerSettings) ─────────
        public readonly string SummaryLabel => type switch
        {
            TRIGGEREFFECT.Move   => $"[Move] {MoveType}  {Duration:0.#}s  {EaseType}",
            TRIGGEREFFECT.Rotate => $"[Rotate] z:{RotateTo.z:0}°  {Duration:0.#}s",
            TRIGGEREFFECT.Scale  => $"[Scale] ({ScaleTo.x:0.#}, {ScaleTo.y:0.#})  {Duration:0.#}s",
            TRIGGEREFFECT.Shake  => $"[Shake] str:{ShakeStrength:0.#}  {Duration:0.#}s",
            TRIGGEREFFECT.Punch  => $"[Punch] {PunchDirection}  {Duration:0.#}s",
            TRIGGEREFFECT.Fade   => $"[Fade] {FadeFrom:0.#}→{FadeTo:0.#}  {Duration:0.#}s",
            TRIGGEREFFECT.Color  => $"[Color] {Duration:0.#}s  {EaseType}",
            _                    => type.ToString()
        };
    }

    [System.Serializable]
    public struct UIContinousEffectdata
    {
        public CONTINUOSEFFECT type;
        [ShowIf("type", CONTINUOSEFFECT.FillText)] public string Text;
        [ShowIf("type", CONTINUOSEFFECT.FillText)] public TextMeshProUGUI TextComponent;
        [ShowIf("type", CONTINUOSEFFECT.ChangeOpacity)] public Image Image;
        [ShowIf("type", CONTINUOSEFFECT.ChangeOpacity)] public float Opacity;
    }

    [System.Serializable]
    public class UIAnimationRawData : IUISetData
    {
        public ANIMATIONSOURCE animationSource = ANIMATIONSOURCE.FromData;
        public float Delay;
        [ReadOnly] public float TimePlay;
        GameObject IUISetData.UIObj => UIObj;
        float IUISetData.Delay => Delay;
        float IUISetData.TimePlay => TimePlay;
        [EnumToggleButtons][ShowIf("animationSource", ANIMATIONSOURCE.FromData)]  public GameObject UIObj;
        [EnumToggleButtons][ShowIf("animationSource", ANIMATIONSOURCE.FromData)] public UIAnimationData UIAnimationData;
        [EnumToggleButtons][ShowIf("animationSource", ANIMATIONSOURCE.Custom)]  public InterfaceReference<IUIAnimation> CustomUIAnimationData;
    }
    [System.Serializable]
    public class UIAnimationRuntimeData
    {
        public float TimePlay;
        public float Delay;
        public List<IUIAnimation> Animation;
    }
    [System.Serializable]
    public class UIAnimationRawDatas
    {
        public List<UIAnimationRawData> Animations;
    }
    [System.Serializable]
    public enum ANIMATIONSOURCE
    {
        FromData,
        Custom,
    }
}
