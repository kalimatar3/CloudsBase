using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using Clouds.UI.Animation;
using Clouds.Ultilities;

namespace Clouds.Ultilities
{
    [System.Serializable]
    public struct UIEffectData
    {
        public TRIGGEREFFECT type;        
        public float Delay;
        public float Duration
        {
            get
            {
                switch (type)
                {
                    case TRIGGEREFFECT.Move:
                        return timeMove;
                    case TRIGGEREFFECT.Rotate:
                        return timeRotate;
                    case TRIGGEREFFECT.Scale:
                        return timeScale;
                    case TRIGGEREFFECT.Shake:
                        return timeShake;
                    case TRIGGEREFFECT.Punch:
                        return timePunch;
                    case TRIGGEREFFECT.Fade:
                        return timeFade;
                    case TRIGGEREFFECT.Color:
                        return timeColor;
                    default:
                        return 0f;
                }
            }
        }
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public MOVEEFFECT moveType;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] [EnableIf("@this.type == TRIGGEREFFECT.Move")] public Vector3 Offset;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public float timeMove;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public Ease easeMove;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public bool loopMove;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] [ShowIf("loopMove")] public LoopType MoveLoopType;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] [ShowIf("loopMove")] public int loopCircleMove;

        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public Vector3 rotateTo;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public float timeRotate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public Ease easeRotate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public bool loopRotate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] [ShowIf("loopRotate")] public LoopType RotateLoopType;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] [ShowIf("loopRotate")] public int loopCircleRotate;


        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public Vector3 scalefrom;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public Vector3 scaleTo;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public float timeScale;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public ACTIVATETYPE scaleActivate;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public Ease easeScale;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public bool loopScale;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public LoopType ScaleLoopType;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] [ShowIf("loopScale")] public int loopCircleScale;

        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public bool shakePosition, shakeRotation, shakeScale;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public float shakeStrength, shakeRandomness, timeShake;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public int shakeVibrate;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public Ease easeShake;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public bool loopShake;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] [ShowIf("loopShake")] public LoopType ShakeLoopType;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] [ShowIf("loopShake")] public int loopCircleShake;

        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public bool punchPostion, punchRotation, punchScale;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public Vector2 punchTo;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public int punchVibrate;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public float punchElasticity, timePunch;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public Ease easePunch;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public bool loopPunch;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] [ShowIf("loopPunch")] public LoopType PunchLoopType;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] [ShowIf("loopPunch")] public int loopCirclePunch;


        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] [Range(0, 1)] public float fadefrom;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] [Range(0, 1)] public float fadeTo;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public float timeFade;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public Ease easeFade;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public bool loopFade ;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] [ShowIf("loopFade")] public LoopType FadeLooptype;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] [ShowIf("loopFade")] public int loopCircleFade;

        [BoxGroup("Color Settings")] [ShowIf("type", TRIGGEREFFECT.Color)] public Color colorFrom;
        [BoxGroup("Color Settings")] [ShowIf("type", TRIGGEREFFECT.Color)] public Color colorTo;
        [BoxGroup("Color Settings")] [ShowIf("type", TRIGGEREFFECT.Color)] public float timeColor;
        [BoxGroup("Color Settings")] [ShowIf("type", TRIGGEREFFECT.Color)] public Ease easeColor;
        [BoxGroup("Color Settings")] [ShowIf("type", TRIGGEREFFECT.Color)] public bool loopColor;
        [BoxGroup("Color Settings")] [ShowIf("type", TRIGGEREFFECT.Color)] [ShowIf("loopColor")] public LoopType ColorLoopType;
        [BoxGroup("Color Settings")] [ShowIf("type", TRIGGEREFFECT.Color)] [ShowIf("loopColor")] public int loopCircleColor;
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