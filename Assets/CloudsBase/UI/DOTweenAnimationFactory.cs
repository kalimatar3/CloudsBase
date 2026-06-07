using UnityEngine;
using Clouds.UI.Animation;
using Clouds.Ultilities;
using DG.Tweening;
using System;
using UnityEngine.UI;

namespace Clouds.UI.Animation
{
    public class DOTweenUIAnimation : IUIAnimation
    {
        private Tween _tween;
        public event Action OnComplete;
        public event Action OnStart;
        public DOTweenUIAnimation(Tween tween)
        {
            _tween = tween;
            if (_tween != null) 
            {
                _tween.OnComplete(() => OnComplete?.Invoke());
                _tween.OnStart(() => OnStart?.Invoke());
            }
        }

        public void Play() => _tween?.Restart();
        public void Stop() => _tween?.Kill();
        public void Restart() => _tween?.Restart();
        public bool IsPlaying => _tween != null && _tween.IsPlaying();
        public object NativeAnimation => _tween; // Returns the actual Tween for the Editor
        public float Duration => _tween.Duration();
    }

    public class DOTweenAnimationFactory : IUIAnimationFactory
    {
        // Helper function to map Ease
        private DG.Tweening.Ease MapEase(Ease ease)
        {
            return (DG.Tweening.Ease)Enum.Parse(typeof(DG.Tweening.Ease), ease.ToString());
        }
        // Helper function to map LoopType
        private DG.Tweening.LoopType MapLoop(LoopType loopType) {
            return (DG.Tweening.LoopType)Enum.Parse(typeof(DG.Tweening.LoopType),loopType.ToString());
        }
        public IUIAnimation CreateMove(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            DG.Tweening.Ease dotweenEase = MapEase(effect.easeMove);
            Sequence seq = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetId(1)
                .SetAutoKill(false)
                .Pause()
                .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy);

            Vector2 startPos = rect.anchoredPosition;
            Vector2 targetPos = rect.anchoredPosition;
            switch (effect.moveType)
            {
                case MOVEEFFECT.Custom:
                    startPos = targetPos + (Vector2)effect.Offset;
                    break;
                case MOVEEFFECT.FromAbove:
                    startPos = new Vector2(targetPos.x, (Screen.height / 2f) + (rect.rect.height / 2f) + 100f);
                    break;
                case MOVEEFFECT.FromBelow:
                    startPos = new Vector2(targetPos.x, -((Screen.height / 2f) + (rect.rect.height / 2f) + 100f));
                    break;
                case MOVEEFFECT.FromLeft:
                    startPos = new Vector2(-((Screen.width / 2f) + (rect.rect.width / 2f) + 100f), targetPos.y);
                    break;
                case MOVEEFFECT.FromRight:
                    startPos = new Vector2((Screen.width / 2f) + (rect.rect.width / 2f) + 100f, targetPos.y);
                    break;
                case MOVEEFFECT.ToAbove:
                    targetPos = new Vector2(startPos.x, (Screen.height / 2f) + (rect.rect.height / 2f) + 100f);
                    break;
                case MOVEEFFECT.ToBelow:
                    targetPos = new Vector2(startPos.x, -((Screen.height / 2f) + (rect.rect.height / 2f) + 100f));
                    break;
                case MOVEEFFECT.ToLeft:
                    targetPos = new Vector2(-((Screen.width / 2f) + (rect.rect.width / 2f) + 100f), startPos.y);
                    break;
                case MOVEEFFECT.ToRight:
                    targetPos = new Vector2((Screen.width / 2f) + (rect.rect.width / 2f) + 100f, startPos.y);
                    break;
            }

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            seq.Append(rect.DOAnchorPos(targetPos, effect.timeMove).From(startPos, false, false).SetEase(dotweenEase));
            if (effect.loopMove) seq.SetLoops(effect.loopCircleMove);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateRotate(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            Vector3 startRot = rect.localEulerAngles;
            Sequence seq = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetId(1)
                .SetAutoKill(false)
                .Pause()
                .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy);

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            seq.Append(rect.DOLocalRotate(effect.rotateTo, effect.timeRotate, RotateMode.FastBeyond360).From(startRot, false, false).SetEase(MapEase(effect.easeRotate)));
            if (effect.loopRotate) seq.SetLoops(effect.loopCircleRotate);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateScale(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (rect == null && data != null) rect = data.UIObj.GetComponent<RectTransform>();
            Sequence seq = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetId(1)
                .SetAutoKill(false)
                .Pause()
                .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy);

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            seq.Append(rect.DOScale(effect.scaleTo, effect.timeScale).From(effect.scalefrom, false, false).SetEase(MapEase(effect.easeScale)));
            seq.SetLoops(effect.loopCircleScale, MapLoop(effect.ScaleLoopType));

            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateShake(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            Sequence seq = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetId(1)
                .SetAutoKill(false)
                .Pause()
                .SetLink(rect.gameObject,LinkBehaviour.KillOnDestroy);

            seq.AppendInterval(effect.Delay);

            Tween shakeTween = null;
            if (effect.shakePosition) shakeTween = rect.DOShakePosition(effect.timeShake, effect.shakeStrength, effect.shakeVibrate, effect.shakeRandomness);
            else if (effect.shakeRotation) shakeTween = rect.DOShakeRotation(effect.timeShake, effect.shakeStrength, effect.shakeVibrate, effect.shakeRandomness);
            else if (effect.shakeScale) shakeTween = rect.DOShakeScale(effect.timeShake, effect.shakeStrength, effect.shakeVibrate, effect.shakeRandomness);

            if (shakeTween != null)
            {
                shakeTween.SetEase(MapEase(effect.easeShake));
                seq.Append(shakeTween);
            }

            if (effect.loopShake) seq.SetLoops(effect.loopCircleShake);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreatePunch(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (rect == null && data != null) rect = data.UIObj.GetComponent<RectTransform>();
            Sequence seq = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetId(1)
                .SetAutoKill(false)
                .Pause()
                .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy);

            seq.AppendInterval(effect.Delay);

            Tween punchTween = null;
            if (effect.punchPostion) punchTween = rect.DOPunchPosition(effect.punchTo, effect.timePunch, effect.punchVibrate, effect.punchElasticity);
            else if (effect.punchRotation) punchTween = rect.DOPunchRotation(effect.punchTo, effect.timePunch, effect.punchVibrate, effect.punchElasticity);
            else if (effect.punchScale) punchTween = rect.DOPunchScale(effect.punchTo, effect.timePunch, effect.punchVibrate, effect.punchElasticity);

            if (punchTween != null)
            {
                punchTween.SetEase(MapEase(effect.easePunch));
                seq.Append(punchTween);
            }

            if (effect.loopPunch) seq.SetLoops(effect.loopCirclePunch);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateFade(CanvasGroup canvas, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (canvas == null && data != null) canvas = data.UIObj.GetComponent<CanvasGroup>();
            if (canvas == null && data != null) canvas = data.UIObj.AddComponent<CanvasGroup>();

            Sequence sequence = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetAutoKill(false)
                .Pause()
                .SetLink(canvas.gameObject, LinkBehaviour.KillOnDestroy);
                if (effect.Delay > 0) sequence.AppendInterval(effect.Delay);
                sequence.Append(canvas.DOFade(effect.fadeTo, effect.timeFade)
                .From(effect.fadefrom, false, false)
                .SetEase(MapEase(effect.easeFade)));
            if (effect.loopFade) sequence.SetLoops(effect.loopCircleFade);
            return new DOTweenUIAnimation(sequence);
        }

        public IUIAnimation CreateColor(Graphic graphic, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (graphic == null && data != null)
                graphic = data.UIObj.GetComponent<Graphic>();

            if (graphic == null)
                return new DOTweenUIAnimation(DOTween.Sequence().SetUpdate(ignoreTimeScale).SetAutoKill(false).Pause());

            Sequence sequence = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetAutoKill(false)
                .Pause()
                .SetLink(graphic.gameObject, LinkBehaviour.KillOnDestroy);

            if (effect.Delay > 0)
                sequence.AppendInterval(effect.Delay);

            sequence.Append(graphic.DOColor(effect.colorTo, effect.timeColor)
                .From(effect.colorFrom, false)
                .SetEase(MapEase(effect.easeColor)));

            if (effect.loopColor)
                sequence.SetLoops(effect.loopCircleColor, MapLoop(effect.ColorLoopType));

            return new DOTweenUIAnimation(sequence);
        }
    }
}