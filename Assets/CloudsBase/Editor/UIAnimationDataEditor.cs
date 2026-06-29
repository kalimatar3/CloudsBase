#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Clouds.Ultilities;
using Clouds.UI.Animation;

[CustomEditor(typeof(UIAnimationData))]
public class UIAnimationDataEditor : OdinEditor
{
    // Color per effect type — index matches TRIGGEREFFECT enum order
    private static readonly Color[] EffectColors =
    {
        new Color(0.35f, 0.75f, 1.00f), // Move   — sky blue
        new Color(0.40f, 1.00f, 0.55f), // Rotate — green
        new Color(1.00f, 0.90f, 0.25f), // Scale  — yellow
        new Color(1.00f, 0.38f, 0.38f), // Shake  — red
        new Color(1.00f, 0.60f, 0.20f), // Punch  — orange
        new Color(0.45f, 0.55f, 1.00f), // Fade   — blue
        new Color(0.90f, 0.40f, 1.00f), // Color  — magenta
        new Color(0.40f, 0.40f, 0.40f), // Nothing — gray
    };

    private const float LabelW  = 58f;
    private const float BarW    = 260f;
    private const float BarH    = 15f;
    private const float InfoW   = 120f;
    private const float RowGap  = 3f;

    private static readonly Color DelayColor    = new Color(0.22f, 0.22f, 0.22f);
    private static readonly Color TrackBg       = new Color(0.15f, 0.15f, 0.15f);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var so = (UIAnimationData)target;
        if (so.Effects == null || so.Effects.Length == 0) return;

        EditorGUILayout.Space(12);
        DrawTimeline(so);
    }

    private void DrawTimeline(UIAnimationData so)
    {
        float total = so.GetTotalDuration();
        if (total <= 0f) return;

        EditorGUILayout.LabelField("Effect Timeline", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        float pixelsPerSecond = BarW / total;

        foreach (var effect in so.Effects)
        {
            DrawEffectRow(effect, pixelsPerSecond);
            GUILayout.Space(RowGap);
        }

        EditorGUILayout.Space(4);
        DrawRuler(total, pixelsPerSecond);
        EditorGUILayout.Space(2);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(LabelW + 4);
            EditorGUILayout.LabelField($"Total  {total:0.00}s", EditorStyles.miniLabel);
        }
    }

    private void DrawEffectRow(UIEffectData effect, float pixelsPerSecond)
    {
        int typeIndex = (int)effect.type;
        Color typeColor = typeIndex < EffectColors.Length ? EffectColors[typeIndex] : Color.gray;

        using (new EditorGUILayout.HorizontalScope())
        {
            // Type label
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                { normal = { textColor = typeColor }, fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField(effect.type.ToString(), labelStyle, GUILayout.Width(LabelW));

            // Bar canvas
            Rect canvas = GUILayoutUtility.GetRect(BarW, BarH, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(canvas, TrackBg);

            // Delay segment
            float delayPx = effect.Delay * pixelsPerSecond;
            if (delayPx > 0f)
                EditorGUI.DrawRect(new Rect(canvas.x, canvas.y, delayPx, BarH), DelayColor);

            // Duration segment
            float durPx = effect.Duration * pixelsPerSecond;
            if (durPx > 0f)
            {
                var barColor = typeColor;
                barColor.a = 0.85f;
                EditorGUI.DrawRect(new Rect(canvas.x + delayPx, canvas.y, durPx, BarH), barColor);
            }

            GUILayout.Space(6);

            // Info label
            string easeLabel = effect.EaseType == Clouds.UI.Animation.Ease.Custom ? "Custom" : effect.EaseType.ToString();
            EditorGUILayout.LabelField(
                $"d {effect.Delay:0.##}s  t {effect.Duration:0.##}s  {easeLabel}",
                EditorStyles.miniLabel, GUILayout.Width(InfoW));
        }
    }

    private void DrawRuler(float total, float pixelsPerSecond)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(LabelW + 4);
            Rect rulerRect = GUILayoutUtility.GetRect(BarW, 12f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(rulerRect, new Color(0.12f, 0.12f, 0.12f));

            // Tick marks every 0.1s (or scaled if timeline is long)
            float tickInterval = total <= 1f ? 0.1f : total <= 3f ? 0.25f : 0.5f;
            var tickStyle = new GUIStyle(EditorStyles.miniLabel)
                { normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }, fontSize = 8 };

            for (float t = 0f; t <= total + 0.001f; t += tickInterval)
            {
                float x = rulerRect.x + t * pixelsPerSecond;
                if (x > rulerRect.xMax) break;
                EditorGUI.DrawRect(new Rect(x, rulerRect.y, 1f, 5f), new Color(0.5f, 0.5f, 0.5f));
                if (t > 0f)
                    GUI.Label(new Rect(x + 2f, rulerRect.y, 30f, 12f), $"{t:0.#}", tickStyle);
            }
        }
    }
}
#endif
