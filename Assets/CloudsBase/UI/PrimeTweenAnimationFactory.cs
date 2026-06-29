using System;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using Clouds.UI.Animation;
using Clouds.Ultilities;

using PTEase      = PrimeTween.Ease;
using PTCycleMode = PrimeTween.CycleMode;

namespace Clouds.UI.Animation
{
    public class PrimeTweenUIAnimation : IUIAnimation
    {
        private readonly Func<Sequence> _build;
        private readonly float _duration;
        private Sequence _seq;

        public event Action OnComplete;
        public event Action OnStart;

        public bool IsPlaying    => _seq.isAlive;
        public float Duration    => _duration;
        public object NativeAnimation => _seq;

        public PrimeTweenUIAnimation(Func<Sequence> build, float totalDuration)
        {
            _build    = build;
            _duration = totalDuration;
        }

        public void Play()
        {
            if (_seq.isAlive) _seq.Stop();
            OnStart?.Invoke();
            _seq = _build();
            _seq.OnComplete(() => OnComplete?.Invoke());
        }

        public void Stop()
        {
            if (_seq.isAlive) _seq.Stop();
        }

        public void Restart() => Play();
    }

    public class PrimeTweenAnimationFactory : IUIAnimationFactory
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static PTEase MapEase(UIEffectData effect)
        {
            if (effect.EaseType == Ease.Custom) return PTEase.Custom;
            return Enum.TryParse<PTEase>(effect.EaseType.ToString(), out var e) ? e : PTEase.Default;
        }

        private static PTCycleMode MapCycleMode(LoopType t)
            => t == LoopType.Yoyo ? PTCycleMode.Yoyo : PTCycleMode.Restart;

        private static int Cycles(UIEffectData effect)
            => effect.Loop ? (effect.LoopCount <= 0 ? -1 : effect.LoopCount) : 1;

        // TweenSettings bundles duration + ease/curve for one tween
        private static TweenSettings TS(UIEffectData effect)
            => effect.EaseType == Ease.Custom
                ? new TweenSettings(effect.Duration, effect.Curve)
                : new TweenSettings(effect.Duration, MapEase(effect));

        // ── Factory methods ───────────────────────────────────────────────────

        public IUIAnimation CreateMove(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            // Capture positions at setup time so they stay correct across replays
            Vector2 restPos   = rect.anchoredPosition;
            Vector2 startPos  = restPos;
            Vector2 targetPos = restPos;

            switch (effect.MoveType)
            {
                case MOVEEFFECT.Custom:    startPos  = restPos + (Vector2)effect.Offset; break;
                case MOVEEFFECT.FromAbove: startPos  = new Vector2(restPos.x,  ( Screen.height / 2f) + (rect.rect.height / 2f) + 100f); break;
                case MOVEEFFECT.FromBelow: startPos  = new Vector2(restPos.x, -((Screen.height / 2f) + (rect.rect.height / 2f) + 100f)); break;
                case MOVEEFFECT.FromLeft:  startPos  = new Vector2(-((Screen.width  / 2f) + (rect.rect.width  / 2f) + 100f), restPos.y); break;
                case MOVEEFFECT.FromRight: startPos  = new Vector2(  (Screen.width  / 2f) + (rect.rect.width  / 2f) + 100f,  restPos.y); break;
                case MOVEEFFECT.ToAbove:   targetPos = new Vector2(restPos.x,  ( Screen.height / 2f) + (rect.rect.height / 2f) + 100f); break;
                case MOVEEFFECT.ToBelow:   targetPos = new Vector2(restPos.x, -((Screen.height / 2f) + (rect.rect.height / 2f) + 100f)); break;
                case MOVEEFFECT.ToLeft:    targetPos = new Vector2(-((Screen.width  / 2f) + (rect.rect.width  / 2f) + 100f), restPos.y); break;
                case MOVEEFFECT.ToRight:   targetPos = new Vector2(  (Screen.width  / 2f) + (rect.rect.width  / 2f) + 100f,  restPos.y); break;
            }

            Vector2 s = startPos, t = targetPos;
            TweenSettings ts  = TS(effect);
            float delay       = effect.Delay;
            int   cycles      = Cycles(effect);
            PTCycleMode cm    = MapCycleMode(effect.LoopType);

            return new PrimeTweenUIAnimation(() =>
            {
                rect.anchoredPosition = s;
                Sequence seq = Sequence.Create(cycles: cycles, cycleMode: cm);
                if (delay > 0f) seq.ChainDelay(delay);
                seq.Chain(Tween.UIAnchoredPosition(rect, s, t, ts));
                return seq;
            }, delay + effect.Duration);
        }

        public IUIAnimation CreateRotate(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            Quaternion startRot = rect.localRotation;
            Quaternion endRot   = Quaternion.Euler(effect.RotateTo);
            TweenSettings ts    = TS(effect);
            float delay         = effect.Delay;
            int   cycles        = Cycles(effect);
            PTCycleMode cm      = MapCycleMode(effect.LoopType);

            return new PrimeTweenUIAnimation(() =>
            {
                rect.localRotation = startRot;
                Sequence seq = Sequence.Create(cycles: cycles, cycleMode: cm);
                if (delay > 0f) seq.ChainDelay(delay);
                seq.Chain(Tween.LocalRotation(rect, startRot, endRot, ts));
                return seq;
            }, delay + effect.Duration);
        }

        public IUIAnimation CreateScale(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (rect == null && data != null) rect = data.UIObj.GetComponent<RectTransform>();
            Vector3 scaleFrom = effect.ScaleFrom;
            Vector3 scaleTo   = effect.ScaleTo;
            TweenSettings ts  = TS(effect);
            float delay       = effect.Delay;
            int   cycles      = Cycles(effect);
            PTCycleMode cm    = MapCycleMode(effect.LoopType);

            return new PrimeTweenUIAnimation(() =>
            {
                rect.localScale = scaleFrom;
                Sequence seq = Sequence.Create(cycles: cycles, cycleMode: cm);
                if (delay > 0f) seq.ChainDelay(delay);
                seq.Chain(Tween.Scale(rect, scaleFrom, scaleTo, ts));
                return seq;
            }, delay + effect.Duration);
        }

        public IUIAnimation CreateShake(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            float strength    = effect.ShakeStrength;
            int   vibrato     = effect.ShakeVibrato;
            float duration    = effect.Duration;
            float delay       = effect.Delay;
            int   cycles      = Cycles(effect);
            PTCycleMode cm    = MapCycleMode(effect.LoopType);
            bool shakePos     = effect.ShakePosition;
            bool shakeRot     = effect.ShakeRotation;
            bool shakeScale   = effect.ShakeScale;

            return new PrimeTweenUIAnimation(() =>
            {
                Vector3 strVec = new Vector3(strength, strength, 0f);
                Sequence seq = Sequence.Create(cycles: cycles, cycleMode: cm);
                if (delay > 0f) seq.ChainDelay(delay);

                if      (shakePos)   seq.Chain(Tween.ShakeLocalPosition(rect, strVec, duration, vibrato));
                else if (shakeRot)   seq.Chain(Tween.ShakeLocalRotation(rect, strVec, duration, vibrato));
                else if (shakeScale) seq.Chain(Tween.ShakeScale(rect, strVec, duration, vibrato));

                return seq;
            }, delay + duration);
        }

        public IUIAnimation CreatePunch(RectTransform rect, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (rect == null && data != null) rect = data.UIObj.GetComponent<RectTransform>();
            Vector3 direction = new Vector3(effect.PunchDirection.x, effect.PunchDirection.y, 0f);
            int   vibrato     = effect.PunchVibrato;
            float duration    = effect.Duration;
            float delay       = effect.Delay;
            int   cycles      = Cycles(effect);
            PTCycleMode cm    = MapCycleMode(effect.LoopType);
            bool punchPos     = effect.PunchPosition;
            bool punchRot     = effect.PunchRotation;
            bool punchScale   = effect.PunchScale;

            return new PrimeTweenUIAnimation(() =>
            {
                Sequence seq = Sequence.Create(cycles: cycles, cycleMode: cm);
                if (delay > 0f) seq.ChainDelay(delay);

                if      (punchPos)   seq.Chain(Tween.PunchLocalPosition(rect, direction, duration, vibrato));
                else if (punchRot)   seq.Chain(Tween.PunchLocalRotation(rect, direction, duration, vibrato));
                else if (punchScale) seq.Chain(Tween.PunchScale(rect, direction, duration, vibrato));

                return seq;
            }, delay + duration);
        }

        public IUIAnimation CreateFade(CanvasGroup canvas, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (canvas == null && data != null) canvas = data.UIObj.GetComponent<CanvasGroup>();
            if (canvas == null && data != null) canvas = data.UIObj.AddComponent<CanvasGroup>();
            float from      = effect.FadeFrom;
            float to        = effect.FadeTo;
            TweenSettings ts = TS(effect);
            float delay     = effect.Delay;
            int   cycles    = Cycles(effect);
            PTCycleMode cm  = MapCycleMode(effect.LoopType);

            return new PrimeTweenUIAnimation(() =>
            {
                canvas.alpha = from;
                Sequence seq = Sequence.Create(cycles: cycles, cycleMode: cm);
                if (delay > 0f) seq.ChainDelay(delay);
                seq.Chain(Tween.Alpha(canvas, from, to, ts));
                return seq;
            }, delay + effect.Duration);
        }

        public IUIAnimation CreateColor(Graphic graphic, UIEffectData effect, IUISetData data = null, bool ignoreTimeScale = false)
        {
            if (graphic == null && data != null) graphic = data.UIObj.GetComponent<Graphic>();
            if (graphic == null)
                return new PrimeTweenUIAnimation(() => Sequence.Create(), 0f);

            Color from      = effect.ColorFrom;
            Color to        = effect.ColorTo;
            TweenSettings ts = TS(effect);
            float delay     = effect.Delay;
            int   cycles    = Cycles(effect);
            PTCycleMode cm  = MapCycleMode(effect.LoopType);
            Graphic g       = graphic;

            return new PrimeTweenUIAnimation(() =>
            {
                g.color = from;
                Sequence seq = Sequence.Create(cycles: cycles, cycleMode: cm);
                if (delay > 0f) seq.ChainDelay(delay);
                seq.Chain(Tween.Color(g, from, to, ts));
                return seq;
            }, delay + effect.Duration);
        }
    }
}
