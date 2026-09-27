using System;
using Clouds.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Clouds.Animation
{
    [Flags]
    public enum AnimationChannel
    {
        None     = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale    = 1 << 2,
        Alpha    = 1 << 3,
        Color    = 1 << 4
    }

    /// <summary>
    /// Trạng thái của target ngay trước khi một animation chạy — chính là giá trị From mà "FromCurrent"
    /// dùng, và cũng là mốc để Restore() đưa object về sau khi animation xong.
    ///
    /// Chỉ chụp đúng những kênh animation thật sự đụng tới. Chụp thừa rồi Restore là ghi đè cả giá trị
    /// do gameplay đặt — ví dụ màu node do luật chơi set chứ không phải do tween.
    /// </summary>
    public struct AnimationSnapshot
    {
        public AnimationChannel Channels;

        public Vector3    LocalPosition;
        public Vector2    AnchoredPosition;
        public Quaternion LocalRotation;
        public Vector3    LocalScale;
        public float      Alpha;
        public Color      Color;

        public bool HasValue => Channels != AnimationChannel.None;

        /// <summary>Những kênh mà preset này sẽ ghi vào target.</summary>
        public static AnimationChannel ChannelsOf(UIAnimationData data)
        {
            if (data == null || data.Effects == null) return AnimationChannel.None;

            AnimationChannel channels = AnimationChannel.None;
            foreach (var effect in data.Effects)
            {
                switch (effect.type)
                {
                    case TRIGGEREFFECT.Move:   channels |= AnimationChannel.Position; break;
                    case TRIGGEREFFECT.Rotate: channels |= AnimationChannel.Rotation; break;
                    case TRIGGEREFFECT.Scale:  channels |= AnimationChannel.Scale;    break;
                    case TRIGGEREFFECT.Fade:   channels |= AnimationChannel.Alpha;    break;
                    case TRIGGEREFFECT.Color:  channels |= AnimationChannel.Color;    break;

                    // Shake/Punch tự trả về gốc khi chạy hết, nhưng Stop() giữa chừng thì bỏ object lại
                    // ở chỗ lệch — vẫn phải chụp để Restore() còn kéo về được.
                    case TRIGGEREFFECT.Shake:
                    case TRIGGEREFFECT.Punch:
                        if (effect.ShakePosition || effect.PunchPosition) channels |= AnimationChannel.Position;
                        if (effect.ShakeRotation || effect.PunchRotation) channels |= AnimationChannel.Rotation;
                        if (effect.ShakeScale    || effect.PunchScale)    channels |= AnimationChannel.Scale;
                        break;
                }
            }
            return channels;
        }

        public static AnimationSnapshot Capture(GameObject go, AnimationChannel channels)
        {
            var snapshot = new AnimationSnapshot { Channels = channels };
            if (channels == AnimationChannel.None || go == null) return snapshot;

            Transform t = go.transform;
            if ((channels & AnimationChannel.Position) != 0)
            {
                snapshot.LocalPosition = t.localPosition;
                if (t is RectTransform rect) snapshot.AnchoredPosition = rect.anchoredPosition;
            }
            if ((channels & AnimationChannel.Rotation) != 0) snapshot.LocalRotation = t.localRotation;
            if ((channels & AnimationChannel.Scale)    != 0) snapshot.LocalScale    = t.localScale;

            bool needsAlpha = (channels & AnimationChannel.Alpha) != 0;
            bool needsColor = (channels & AnimationChannel.Color) != 0;
            if (!needsAlpha && !needsColor) return snapshot;

            // UI ghi alpha qua CanvasGroup và màu qua Graphic; world không có cả hai nên rơi xuống
            // Renderer. Cùng thứ tự ưu tiên với TweenCUIAnimation/TweenWorldAnimation lúc dựng tween.
            var canvasGroup = go.GetComponent<CanvasGroup>();
            var graphic     = go.GetComponent<Graphic>();

            if (needsAlpha && canvasGroup != null) snapshot.Alpha = canvasGroup.alpha;
            if (needsColor && graphic     != null) snapshot.Color = graphic.color;

            bool alphaPending = needsAlpha && canvasGroup == null;
            bool colorPending = needsColor && graphic     == null;
            if (!alphaPending && !colorPending) return snapshot;

            var renderer = go.GetComponentInChildren<Renderer>();
            if (renderer == null) return snapshot;

            Color current = RendererColor.Read(renderer, new MaterialPropertyBlock(), RendererColor.ResolveProperty(renderer));
            if (alphaPending) snapshot.Alpha = current.a;
            if (colorPending) snapshot.Color = current;
            return snapshot;
        }

        public void Restore(GameObject go)
        {
            if (Channels == AnimationChannel.None || go == null) return;

            Transform t = go.transform;
            if ((Channels & AnimationChannel.Position) != 0)
            {
                if (t is RectTransform rect) rect.anchoredPosition = AnchoredPosition;
                else                         t.localPosition       = LocalPosition;
            }
            if ((Channels & AnimationChannel.Rotation) != 0) t.localRotation = LocalRotation;
            if ((Channels & AnimationChannel.Scale)    != 0) t.localScale    = LocalScale;

            bool hasAlpha = (Channels & AnimationChannel.Alpha) != 0;
            bool hasColor = (Channels & AnimationChannel.Color) != 0;
            if (!hasAlpha && !hasColor) return;

            var canvasGroup = go.GetComponent<CanvasGroup>();
            var graphic     = go.GetComponent<Graphic>();

            if (hasAlpha && canvasGroup != null) canvasGroup.alpha = Alpha;
            if (hasColor && graphic     != null) graphic.color     = Color;

            bool alphaPending = hasAlpha && canvasGroup == null;
            bool colorPending = hasColor && graphic     == null;
            if (!alphaPending && !colorPending) return;

            var renderer = go.GetComponentInChildren<Renderer>();
            if (renderer == null) return;

            var block      = new MaterialPropertyBlock();
            int propertyId = RendererColor.ResolveProperty(renderer);

            // Chỉ chụp alpha thì phải giữ nguyên RGB đang có, không kéo cả màu về theo.
            Color target = colorPending ? Color : RendererColor.Read(renderer, block, propertyId);
            if (alphaPending) target.a = Alpha;
            RendererColor.Write(renderer, block, propertyId, target);
        }
    }
}
