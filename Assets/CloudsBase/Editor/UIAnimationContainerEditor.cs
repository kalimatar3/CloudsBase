#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Clouds.UI.Animation;
using Clouds.UI.Editor;

/// <summary>
/// Editor cho UIAnimationContainer.
/// Play Mode: Play/Stop từng key trực tiếp trên component.
/// Edit Mode: Preview bằng DOTweenEditorPreview — nhớ bấm Stop sau khi xem xong.
/// </summary>
[CustomEditor(typeof(UIAnimationContainer))]
public class UIAnimationContainerEditor : OdinEditor
{
    private DOTweenPreviewer _previewer;
    private string _previewingKey;

    private void OnDisable()
    {
        StopPreview();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var container = (UIAnimationContainer)target;
        var entries   = container.Entries;
        if (entries == null || entries.Count == 0) return;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("── Animation Preview ──", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            DrawPlayModePreview(container, entries);
        }
        else
        {
            DrawEditModePreview(container, entries);
        }
    }

    private void DrawPlayModePreview(
        UIAnimationContainer container,
        IReadOnlyList<UIAnimationContainer.AnimationEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Key) || entry.Data == null) continue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(entry.Key, GUILayout.Width(120));
            if (GUILayout.Button("Play", GUILayout.Width(60))) container.Play(entry.Key);
            if (GUILayout.Button("Stop", GUILayout.Width(60))) container.Stop(entry.Key);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Stop All")) container.StopAll();
        if (GUILayout.Button("Rebuild"))  container.Rebuild();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEditModePreview(
        UIAnimationContainer container,
        IReadOnlyList<UIAnimationContainer.AnimationEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Key) || entry.Data == null) continue;

            bool isPreviewing = _previewingKey == entry.Key;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(entry.Key, GUILayout.Width(120));

            GUI.color = isPreviewing ? Color.yellow : Color.white;
            if (GUILayout.Button(isPreviewing ? "▶ Playing" : "Preview", GUILayout.Width(90)))
            {
                if (!isPreviewing)
                    StartPreview(container, entry.Key);
            }
            GUI.color = Color.white;

            if (GUILayout.Button("Stop", GUILayout.Width(60)))
            {
                if (isPreviewing) StopPreview();
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Stop All Previews")) StopPreview();

        EditorGUILayout.HelpBox(
            "Edit mode preview: bấm Stop sau khi xem để reset DOTween state.",
            MessageType.Info);
    }

    private void StartPreview(UIAnimationContainer container, string key)
    {
        StopPreview();

        container.Rebuild();
        var anims = container.GetAnimations(key);
        if (anims.Count == 0) return;

        _previewer     = new DOTweenPreviewer();
        _previewingKey = key;

        _previewer.Start();
        foreach (var anim in anims)
        {
            _previewer.Prepare(anim);
            anim.Restart();
        }

        Repaint();
    }

    private void StopPreview()
    {
        if (_previewer == null) return;
        _previewer.Stop();
        _previewer     = null;
        _previewingKey = null;
        Repaint();
    }
}
#endif
