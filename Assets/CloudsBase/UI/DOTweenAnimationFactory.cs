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
        public void PlayReverse()
        {
            _tween?.Goto(_tween.Duration());
            _tween?.PlayBackwards();
        }
        public void Stop() => _tween?.Kill();
        public void Restart() => _tween?.Restart();
        public bool IsPlaying => _tween != null && _tween.IsPlaying();
        public object NativeAnimation => _tween;
        public float Duration => _tween.Duration();
    }

    public class DOTweenAnimationFactory : IUIAnimationFactory
    {
        private DG.Tweening.Ease MapEase(Ease ease)
            => (DG.Tweening.Ease)Enum.Parse(typeof(DG.Tweening.Ease), ease.ToString());

        private DG.Tweening.LoopType MapLoop(LoopType loopType)
            => (DG.Tweening.LoopType)Enum.Parse(typeof(DG.Tweening.LoopType), loopType.ToString());

        private void ApplyEase(Tween tween, UIEffectData effect)
        {
            if (effect.EaseType == Ease.Custom) tween.SetEase(effect.Curve);
            else tween.SetEase(MapEase(effect.EaseType));
        }

        private static Sequence BaseSequence(bool ignoreTimeScale, GameObject link) =>
            DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetId(1)
                .SetAutoKill(false)
                .Pause()
                .SetLink(link, LinkBehaviour.KillOnDestroy);

        public IUIAnimation CreateMove(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            Sequence seq = BaseSequence(ignoreTimeScale, rect.gameObject);

            Vector2 startPos  = rect.anchoredPosition;
            Vector2 targetPos = rect.anchoredPosition;
            switch (effect.MoveType)
            {
                case MOVEEFFECT.Custom:    startPos  = targetPos + (Vector2)effect.Offset; break;
                case MOVEEFFECT.FromAbove: startPos  = new Vector2(targetPos.x,  ( Screen.height / 2f) + (rect.rect.height / 2f) + 100f); break;
                case MOVEEFFECT.FromBelow: startPos  = new Vector2(targetPos.x, -((Screen.height / 2f) + (rect.rect.height / 2f) + 100f)); break;
                case MOVEEFFECT.FromLeft:  startPos  = new Vector2(-((Screen.width / 2f) + (rect.rect.width  / 2f) + 100f), targetPos.y); break;
                case MOVEEFFECT.FromRight: startPos  = new Vector2( (Screen.width / 2f) + (rect.rect.width  / 2f) + 100f,  targetPos.y); break;
                case MOVEEFFECT.ToAbove:   targetPos = new Vector2(startPos.x,  ( Screen.height / 2f) + (rect.rect.height / 2f) + 100f); break;
                case MOVEEFFECT.ToBelow:   targetPos = new Vector2(startPos.x, -((Screen.height / 2f) + (rect.rect.height / 2f) + 100f)); break;
                case MOVEEFFECT.ToLeft:    targetPos = new Vector2(-((Screen.width / 2f) + (rect.rect.width  / 2f) + 100f), startPos.y); break;
                case MOVEEFFECT.ToRight:   targetPos = new Vector2( (Screen.width / 2f) + (rect.rect.width  / 2f) + 100f,  startPos.y); break;
            }

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            var tween = rect.DOAnchorPos(targetPos, effect.Duration).From(startPos, false, false);
            ApplyEase(tween, effect);
            seq.Append(tween);
            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateRotate(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            Vector3 startRot = rect.localEulerAngles;
            Sequence seq = BaseSequence(ignoreTimeScale, rect.gameObject);

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            var tween = rect.DOLocalRotate(effect.RotateTo, effect.Duration, RotateMode.FastBeyond360).From(startRot, false, false);
            ApplyEase(tween, effect);
            seq.Append(tween);
            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateScale(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (rect == null && data != null) rect = data.UIObj.GetComponent<RectTransform>();
            Sequence seq = BaseSequence(ignoreTimeScale, rect.gameObject);

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            var tween = rect.DOScale(effect.ScaleTo, effect.Duration).From(effect.ScaleFrom, false, false);
            ApplyEase(tween, effect);
            seq.Append(tween);
            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateShake(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            Sequence seq = BaseSequence(ignoreTimeScale, rect.gameObject);
            seq.AppendInterval(effect.Delay);

            Tween shakeTween = null;
            if      (effect.ShakePosition) shakeTween = rect.DOShakePosition(effect.Duration, effect.ShakeStrength, effect.ShakeVibrato, effect.ShakeRandomness);
            else if (effect.ShakeRotation) shakeTween = rect.DOShakeRotation(effect.Duration, effect.ShakeStrength, effect.ShakeVibrato, effect.ShakeRandomness);
            else if (effect.ShakeScale)    shakeTween = rect.DOShakeScale(effect.Duration,    effect.ShakeStrength, effect.ShakeVibrato, effect.ShakeRandomness);

            if (shakeTween != null)
            {
                ApplyEase(shakeTween, effect);
                seq.Append(shakeTween);
            }

            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreatePunch(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (rect == null && data != null) rect = data.UIObj.GetComponent<RectTransform>();
            Sequence seq = BaseSequence(ignoreTimeScale, rect.gameObject);
            seq.AppendInterval(effect.Delay);

            Tween punchTween = null;
            if      (effect.PunchPosition) punchTween = rect.DOPunchPosition(effect.PunchDirection, effect.Duration, effect.PunchVibrato, effect.PunchElasticity);
            else if (effect.PunchRotation) punchTween = rect.DOPunchRotation(effect.PunchDirection, effect.Duration, effect.PunchVibrato, effect.PunchElasticity);
            else if (effect.PunchScale)    punchTween = rect.DOPunchScale(effect.PunchDirection,    effect.Duration, effect.PunchVibrato, effect.PunchElasticity);

            if (punchTween != null)
            {
                ApplyEase(punchTween, effect);
                seq.Append(punchTween);
            }

            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateFade(CanvasGroup canvas, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (canvas == null && data != null) canvas = data.UIObj.GetComponent<CanvasGroup>();
            if (canvas == null && data != null) canvas = data.UIObj.AddComponent<CanvasGroup>();

            Sequence seq = BaseSequence(ignoreTimeScale, canvas.gameObject);
            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            var tween = canvas.DOFade(effect.FadeTo, effect.Duration).From(effect.FadeFrom, false, false);
            ApplyEase(tween, effect);
            seq.Append(tween);
            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateColor(Graphic graphic, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (graphic == null && data != null) graphic = data.UIObj.GetComponent<Graphic>();
            if (graphic == null)
                return new DOTweenUIAnimation(DOTween.Sequence().SetUpdate(ignoreTimeScale).SetAutoKill(false).Pause());

            Sequence seq = BaseSequence(ignoreTimeScale, graphic.gameObject);
            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            var tween = graphic.DOColor(effect.ColorTo, effect.Duration).From(effect.ColorFrom, false);
            ApplyEase(tween, effect);
            seq.Append(tween);
            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
            return new DOTweenUIAnimation(seq);
        }
    }
}
