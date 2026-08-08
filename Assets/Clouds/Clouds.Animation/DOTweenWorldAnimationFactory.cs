using System;
using Clouds.UI;
using DG.Tweening;
using UnityEngine;

namespace Clouds.Animation
{
    /// <summary>
    /// Backend DOTween cho world-space. Dùng lại wrapper DOTweenUIAnimation (nó chỉ bọc Tween, không
    /// dính gì tới UI). Phần map Ease/Loop bị lặp với DOTweenAnimationFactory — sẽ gộp khi core
    /// animation được dời hẳn xuống Clouds.Animation.
    /// </summary>
    public class DOTweenWorldAnimationFactory : IWorldAnimationFactory
    {
        // URP đặt tên property màu là _BaseColor; shader built-in và Sprites/Default dùng _Color.
        private static readonly int BaseColorId   = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        private static DG.Tweening.Ease MapEase(Clouds.UI.Ease ease)
            => (DG.Tweening.Ease)Enum.Parse(typeof(DG.Tweening.Ease), ease.ToString());

        private static DG.Tweening.LoopType MapLoop(Clouds.UI.LoopType loopType)
            => (DG.Tweening.LoopType)Enum.Parse(typeof(DG.Tweening.LoopType), loopType.ToString());

        private static void ApplyEase(Tween tween, UIEffectData effect)
        {
            if (effect.EaseType == Clouds.UI.Ease.Custom) tween.SetEase(effect.Curve);
            else tween.SetEase(MapEase(effect.EaseType));
        }

        // Không SetId(1) như bản UI: id đó dùng chung thì một DOTween.Kill(1) bên UI sẽ giết luôn mọi
        // tween world đang chạy.
        private static Sequence BaseSequence(bool ignoreTimeScale, GameObject link) =>
            DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetAutoKill(false)
                .Pause()
                .SetLink(link, LinkBehaviour.KillOnDestroy);

        private static void Finish(Sequence seq, Tween tween, UIEffectData effect)
        {
            if (tween != null)
            {
                ApplyEase(tween, effect);
                seq.Append(tween);
            }
            if (effect.Loop) seq.SetLoops(effect.LoopCount, MapLoop(effect.LoopType));
        }

        private static IUIAnimation Empty(bool ignoreTimeScale) =>
            new DOTweenUIAnimation(DOTween.Sequence().SetUpdate(ignoreTimeScale).SetAutoKill(false).Pause());

        public IUIAnimation CreateMove(Transform target, UIEffectData effect, bool ignoreTimeScale = false)
        {
            // MOVEEFFECT.From*/To* tính mốc bằng Screen.width/height nên chỉ có nghĩa trong canvas
            // space. Ngoài world chỉ Custom (Offset, đơn vị mét) là dùng được.
            if (effect.MoveType != MOVEEFFECT.Custom)
                Debug.LogWarning(
                    $"[DOTweenWorldAnimationFactory] {target.name}: MoveType '{effect.MoveType}' là hiệu ứng " +
                    "screen-space, world chỉ hỗ trợ Custom — dùng Offset thay thế.", target);

            Sequence seq = BaseSequence(ignoreTimeScale, target.gameObject);

            Vector3 targetPos = target.localPosition;
            Vector3 startPos  = targetPos + effect.Offset;

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            Finish(seq, target.DOLocalMove(targetPos, effect.Duration).From(startPos, false, false), effect);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateRotate(Transform target, UIEffectData effect, bool ignoreTimeScale = false)
        {
            Vector3 startRot = target.localEulerAngles;
            Sequence seq = BaseSequence(ignoreTimeScale, target.gameObject);

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            Finish(seq, target.DOLocalRotate(effect.RotateTo, effect.Duration, RotateMode.FastBeyond360)
                              .From(startRot, false, false), effect);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateScale(Transform target, UIEffectData effect, bool ignoreTimeScale = false)
        {
            Sequence seq = BaseSequence(ignoreTimeScale, target.gameObject);

            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);
            Finish(seq, target.DOScale(effect.ScaleTo, effect.Duration).From(effect.ScaleFrom, false, false), effect);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreateShake(Transform target, UIEffectData effect, bool ignoreTimeScale = false)
        {
            Sequence seq = BaseSequence(ignoreTimeScale, target.gameObject);
            seq.AppendInterval(effect.Delay);

            Tween shakeTween = null;
            if      (effect.ShakePosition) shakeTween = target.DOShakePosition(effect.Duration, effect.ShakeStrength, effect.ShakeVibrato, effect.ShakeRandomness);
            else if (effect.ShakeRotation) shakeTween = target.DOShakeRotation(effect.Duration, effect.ShakeStrength, effect.ShakeVibrato, effect.ShakeRandomness);
            else if (effect.ShakeScale)    shakeTween = target.DOShakeScale(effect.Duration,    effect.ShakeStrength, effect.ShakeVibrato, effect.ShakeRandomness);

            Finish(seq, shakeTween, effect);
            return new DOTweenUIAnimation(seq);
        }

        public IUIAnimation CreatePunch(Transform target, UIEffectData effect, bool ignoreTimeScale = false)
        {
            Sequence seq = BaseSequence(ignoreTimeScale, target.gameObject);
            seq.AppendInterval(effect.Delay);

            Tween punchTween = null;
            if      (effect.PunchPosition) punchTween = target.DOPunchPosition(effect.PunchDirection, effect.Duration, effect.PunchVibrato, effect.PunchElasticity);
            else if (effect.PunchRotation) punchTween = target.DOPunchRotation(effect.PunchDirection, effect.Duration, effect.PunchVibrato, effect.PunchElasticity);
            else if (effect.PunchScale)    punchTween = target.DOPunchScale(effect.PunchDirection,    effect.Duration, effect.PunchVibrato, effect.PunchElasticity);

            Finish(seq, punchTween, effect);
            return new DOTweenUIAnimation(seq);
        }

        // World không có CanvasGroup nên fade phải đi qua alpha của màu vertex/material. Ghi bằng
        // MaterialPropertyBlock chứ không phải renderer.material: material là asset dùng chung, đụng
        // thẳng vào nó là nhuộm mọi object đang dùng chung material đó, còn renderer.material thì tạo
        // một bản sao riêng cho từng instance (phá batching, và bản sao đó rò rỉ khi object bị destroy).
        // Lưu ý: alpha chỉ nhìn thấy nếu material đang ở surface type Transparent.
        public IUIAnimation CreateFade(Renderer renderer, UIEffectData effect, bool ignoreTimeScale = false)
        {
            if (renderer == null) return Empty(ignoreTimeScale);

            var block      = new MaterialPropertyBlock();
            int propertyId = ResolveColorProperty(renderer);
            float alpha    = effect.FadeFrom;

            Sequence seq = BaseSequence(ignoreTimeScale, renderer.gameObject);
            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);

            Tween tween = DOTween.To(() => alpha, value =>
            {
                alpha = value;
                Color color = ReadColor(renderer, block, propertyId);
                color.a = value;
                WriteColor(renderer, block, propertyId, color);
            }, effect.FadeTo, effect.Duration).From(effect.FadeFrom, false);

            Finish(seq, tween, effect);
            return new DOTweenUIAnimation(seq);
        }

        // MaterialPropertyBlock không chạm được tới LineRenderer.startColor/endColor (đó là vertex
        // color của LineRenderer, không phải property của material) — LineRenderer cần tween riêng.
        public IUIAnimation CreateColor(Renderer renderer, UIEffectData effect, bool ignoreTimeScale = false)
        {
            if (renderer == null) return Empty(ignoreTimeScale);

            var block      = new MaterialPropertyBlock();
            int propertyId = ResolveColorProperty(renderer);
            Color current  = effect.ColorFrom;

            Sequence seq = BaseSequence(ignoreTimeScale, renderer.gameObject);
            if (effect.Delay > 0) seq.AppendInterval(effect.Delay);

            Tween tween = DOTween.To(() => current, value =>
            {
                current = value;
                WriteColor(renderer, block, propertyId, value);
            }, effect.ColorTo, effect.Duration).From(effect.ColorFrom, false);

            Finish(seq, tween, effect);
            return new DOTweenUIAnimation(seq);
        }

        private static int ResolveColorProperty(Renderer renderer)
        {
            Material mat = renderer.sharedMaterial;
            return mat != null && !mat.HasProperty(BaseColorId) ? LegacyColorId : BaseColorId;
        }

        private static Color ReadColor(Renderer renderer, MaterialPropertyBlock block, int propertyId)
        {
            renderer.GetPropertyBlock(block);
            if (block.HasColor(propertyId)) return block.GetColor(propertyId);

            Material mat = renderer.sharedMaterial;
            return mat != null && mat.HasProperty(propertyId) ? mat.GetColor(propertyId) : Color.white;
        }

        private static void WriteColor(Renderer renderer, MaterialPropertyBlock block, int propertyId, Color color)
        {
            renderer.GetPropertyBlock(block);
            block.SetColor(propertyId, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
