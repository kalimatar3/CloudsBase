#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Clouds.Animation;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Clouds.UI
{
    /// <summary>
    /// Inspector của UIAnimationSequencer — vẽ tay thay cho list/dictionary mặc định:
    /// mỗi key một thẻ gập được, trong thẻ là các bước đánh số, mỗi bước là một nhóm animation chạy
    /// song song. Nút ▶ phát thử ngay trong Prefab Mode/Edit Mode (PrimeTween hoặc DOTween), ■ dừng và đưa
    /// object về trạng thái trước khi phát.
    /// </summary>
    [CustomEditor(typeof(UIAnimationSequencer))]
    public class UIAnimationSequencerEditor : OdinEditor
    {
        private SerializedProperty _tracks;
        private SerializedProperty _ignoreTimeScale;
        private readonly SequencerEditPreview _preview = new();

        // Mọi thay đổi cấu trúc (thêm/xoá/đổi chỗ) dồn về cuối lượt vẽ: sửa mảng giữa chừng làm lệch
        // chỉ số của những phần tử vẽ sau nó trong cùng lượt.
        private Action _deferred;

        private UIAnimationSequencer Sequencer => (UIAnimationSequencer)target;

        protected override void OnEnable()
        {
            base.OnEnable();
            _tracks          = serializedObject.FindProperty("_tracks");
            _ignoreTimeScale = serializedObject.FindProperty("_ignoreTimeScale");
            _preview.Changed += Repaint;
        }

        protected override void OnDisable()
        {
            _preview.Changed -= Repaint;
            _preview.Stop();
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _deferred = null;

            EditorGUILayout.PropertyField(_ignoreTimeScale, new GUIContent("Delay theo thời gian thực",
                "Delay giữa các bước không bị Time.timeScale ảnh hưởng."));
            DrawDuplicateKeyWarning();
            EditorGUILayout.Space(4);

            for (int i = 0; i < _tracks.arraySize; i++) DrawTrack(i);

            DrawAddTrackRow();

            _deferred?.Invoke();
            serializedObject.ApplyModifiedProperties();
        }

        // ── Track ────────────────────────────────────────────────────────────────

        private void DrawTrack(int index)
        {
            SerializedProperty track = _tracks.GetArrayElementAtIndex(index);
            SerializedProperty keyProp = track.FindPropertyRelative("Key");
            SerializedProperty steps = track.FindPropertyRelative("Steps");
            string key = keyProp.stringValue;
            UIAnimationSequencer.Track runtimeTrack = index < Sequencer.Tracks.Count ? Sequencer.Tracks[index] : null;

            string foldKey = $"Clouds.UIAnimationSequencer.{target.GetInstanceID()}.{index}";
            bool expanded = SessionState.GetBool(foldKey, true);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            Rect header = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(new Rect(header.x - 2, header.y - 1, header.width + 4, header.height + 2), HeaderTint(key));

            Rect foldRect = GUILayoutUtility.GetRect(14, EditorGUIUtility.singleLineHeight, GUILayout.Width(14));
            bool newExpanded = EditorGUI.Foldout(foldRect, expanded, GUIContent.none, true);
            if (newExpanded != expanded) SessionState.SetBool(foldKey, newExpanded);

            keyProp.stringValue = EditorGUILayout.TextField(key, KeyFieldStyle, GUILayout.Width(110));
            string role = RoleLabel(key);
            if (role != null) GUILayout.Label(role, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            if (runtimeTrack != null)
                GUILayout.Label($"{steps.arraySize} bước · {Format(runtimeTrack.Duration)}", EditorStyles.miniLabel);

            DrawPlayButtons(key);

            if (IconButton("▲", "Đưa lên", index > 0))                        Defer(() => _tracks.MoveArrayElement(index, index - 1));
            if (IconButton("▼", "Đưa xuống", index < _tracks.arraySize - 1)) Defer(() => _tracks.MoveArrayElement(index, index + 1));
            if (IconButton("✕", "Xoá key này", true))                         Defer(() => _tracks.DeleteArrayElementAtIndex(index));
            EditorGUILayout.EndHorizontal();

            if (_preview.IsRunning && _preview.Key == key)
            {
                Rect bar = GUILayoutUtility.GetRect(0, 3, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(bar, new Color(0f, 0f, 0f, 0.2f));
                bar.width *= Mathf.Clamp01(_preview.Progress);
                EditorGUI.DrawRect(bar, new Color(1f, 0.8f, 0.2f, 0.9f));
            }

            if (newExpanded)
            {
                EditorGUILayout.Space(2);
                for (int s = 0; s < steps.arraySize; s++)
                    DrawStep(key, steps, s, runtimeTrack != null && s < runtimeTrack.Steps.Count ? runtimeTrack.Steps[s] : null);

                if (GUILayout.Button("+ Thêm bước", EditorStyles.miniButton)) Defer(() => AddStep(steps));
                if (runtimeTrack != null) DrawTrackWarnings(key, runtimeTrack);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawPlayButtons(string key)
        {
            bool previewing = _preview.IsRunning && _preview.Key == key;
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(key)))
            {
                GUI.color = previewing ? new Color(1f, 0.85f, 0.3f) : Color.white;
                if (GUILayout.Button(new GUIContent("▶", "Phát thử key này"), EditorStyles.miniButtonLeft, GUILayout.Width(26)))
                {
                    if (Application.isPlaying) Sequencer.Play(key);
                    else _preview.Start(Sequencer, key);
                }
                GUI.color = Color.white;

                if (GUILayout.Button(new GUIContent("■", "Dừng và đưa object về trạng thái trước khi phát"),
                                     EditorStyles.miniButtonRight, GUILayout.Width(26)))
                {
                    if (Application.isPlaying) Sequencer.Restore(key);
                    else _preview.Stop();
                }
            }
        }

        private void DrawTrackWarnings(string key, UIAnimationSequencer.Track track)
        {
            bool isTransition = key == UIAnimationSequencer.SHOW || key == UIAnimationSequencer.HIDE;
            List<TweenAnimationBase> anims = UIAnimationSequencer.DistinctAnimations(track);

            if (isTransition && float.IsInfinity(track.Duration))
                EditorGUILayout.HelpBox("Có animation loop vô hạn — transition sẽ không bao giờ xong và view kẹt ở trạng thái đang mở/đóng.",
                                        MessageType.Error);

            var scaled = anims.FindAll(a => !a.IgnoreTimeScale);
            if (isTransition && scaled.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox($"{scaled.Count} animation chạy theo Time.timeScale — mở view lúc game pause thì transition đứng im.",
                                        MessageType.Warning);
                if (GUILayout.Button("Bật\nIgnoreTimeScale", GUILayout.Width(110), GUILayout.ExpandHeight(true)))
                    foreach (var anim in scaled) SetIgnoreTimeScale(anim);
                EditorGUILayout.EndHorizontal();
            }
        }

        // ── Step ─────────────────────────────────────────────────────────────────

        private void DrawStep(string key, SerializedProperty steps, int index, UIAnimationSequencer.Step runtimeStep)
        {
            SerializedProperty step = steps.GetArrayElementAtIndex(index);
            SerializedProperty delay = step.FindPropertyRelative("Delay");
            SerializedProperty animations = step.FindPropertyRelative("Animations");

            Rect box = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_preview.IsRunning && _preview.Key == key && _preview.ActiveStep == index)
                EditorGUI.DrawRect(box, new Color(1f, 0.8f, 0.2f, 0.12f));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Bước {index + 1}", EditorStyles.boldLabel, GUILayout.Width(52));
            GUILayout.Label("Delay", GUILayout.Width(36));
            delay.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(delay.floatValue, GUILayout.Width(44)));
            if (runtimeStep != null)
                GUILayout.Label(animations.arraySize > 1 ? $"song song · {Format(runtimeStep.Duration)}"
                                                         : Format(runtimeStep.Duration), EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (IconButton("▲", "Chạy sớm hơn", index > 0))                   Defer(() => steps.MoveArrayElement(index, index - 1));
            if (IconButton("▼", "Chạy muộn hơn", index < steps.arraySize - 1)) Defer(() => steps.MoveArrayElement(index, index + 1));
            if (IconButton("✕", "Xoá bước", true))                              Defer(() => steps.DeleteArrayElementAtIndex(index));
            EditorGUILayout.EndHorizontal();

            bool isTransition = key == UIAnimationSequencer.SHOW || key == UIAnimationSequencer.HIDE;
            for (int a = 0; a < animations.arraySize; a++) DrawAnimationRow(animations, a, isTransition);

            DrawDropRow(animations);
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationRow(SerializedProperty animations, int index, bool isTransition)
        {
            SerializedProperty element = animations.GetArrayElementAtIndex(index);
            var anim = element.objectReferenceValue as TweenAnimationBase;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(element, GUIContent.none, GUILayout.MinWidth(90));

            string info;
            string tooltip = null;
            if (anim == null) info = "(trống)";
            else if (anim.UIAnimationData == null) info = "chưa gán UIAnimationData";
            else
            {
                float length = UIAnimationSequencer.LengthOf(anim);
                info = $"{anim.UIAnimationData.name} · {Format(length)}";
                if (float.IsInfinity(length)) tooltip = "Loop vô hạn";
                else if (isTransition && !anim.IgnoreTimeScale) { info = "⏸ " + info; tooltip = "Chạy theo Time.timeScale"; }
            }
            GUILayout.Label(new GUIContent(info, tooltip), EditorStyles.miniLabel, GUILayout.MaxWidth(170));

            if (IconButton("✕", "Bỏ khỏi bước", true)) Defer(() => RemoveReference(animations, index));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDropRow(SerializedProperty animations)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);

            Rect zone = GUILayoutUtility.GetRect(new GUIContent("+"), EditorStyles.helpBox,
                                                 GUILayout.Height(20), GUILayout.ExpandWidth(true));
            GUI.Box(zone, "+ Kéo thả object vào đây (chạy song song)", DropZoneStyle);
            HandleDrop(zone, animations);

            var picked = (TweenAnimationBase)EditorGUILayout.ObjectField(null, typeof(TweenAnimationBase), true, GUILayout.Width(110));
            if (picked != null) Defer(() => AddReferences(animations, new List<TweenAnimationBase> { picked }));
            EditorGUILayout.EndHorizontal();
        }

        private void HandleDrop(Rect zone, SerializedProperty animations)
        {
            Event e = Event.current;
            if (!zone.Contains(e.mousePosition)) return;
            if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return;

            List<TweenAnimationBase> found = CollectAnimations(DragAndDrop.objectReferences);
            DragAndDrop.visualMode = found.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

            if (e.type == EventType.DragPerform && found.Count > 0)
            {
                DragAndDrop.AcceptDrag();
                Defer(() => AddReferences(animations, found));
            }
            e.Use();
        }

        // Kéo một GameObject vào là lấy MỌI TweenAnimation trên nó — object vừa fade vừa scale bằng hai
        // component thì cả hai cùng vào bước này.
        private static List<TweenAnimationBase> CollectAnimations(UnityEngine.Object[] objects)
        {
            var result = new List<TweenAnimationBase>();
            foreach (var obj in objects)
            {
                if (obj is TweenAnimationBase anim) { if (!result.Contains(anim)) result.Add(anim); continue; }

                GameObject go = obj as GameObject ?? (obj as Component)?.gameObject;
                if (go == null) continue;
                foreach (var found in go.GetComponents<TweenAnimationBase>())
                    if (!result.Contains(found)) result.Add(found);
            }
            return result;
        }

        // ── Thêm key ─────────────────────────────────────────────────────────────

        private void DrawAddTrackRow()
        {
            EditorGUILayout.BeginHorizontal();
            if (!HasKey(UIAnimationSequencer.SHOW) &&
                GUILayout.Button(new GUIContent("+ Show", "Thay transition mở mặc định của view"), EditorStyles.miniButtonLeft))
                Defer(() => AddTrack(UIAnimationSequencer.SHOW));

            if (!HasKey(UIAnimationSequencer.HIDE) &&
                GUILayout.Button(new GUIContent("+ Hide", "Thay transition đóng mặc định của view"), EditorStyles.miniButtonMid))
                Defer(() => AddTrack(UIAnimationSequencer.HIDE));

            if (GUILayout.Button("+ Key khác", EditorStyles.miniButtonRight))
                Defer(() => AddTrack(UniqueKey("NewKey")));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDuplicateKeyWarning()
        {
            var seen = new HashSet<string>();
            for (int i = 0; i < _tracks.arraySize; i++)
            {
                string key = _tracks.GetArrayElementAtIndex(i).FindPropertyRelative("Key").stringValue;
                if (string.IsNullOrEmpty(key))
                {
                    EditorGUILayout.HelpBox("Có key để trống — không gọi được.", MessageType.Warning);
                    return;
                }
                if (!seen.Add(key))
                {
                    EditorGUILayout.HelpBox($"Key '{key}' bị trùng — chỉ key đầu tiên được dùng.", MessageType.Warning);
                    return;
                }
            }
        }

        // ── Thao tác mảng ────────────────────────────────────────────────────────

        private void Defer(Action action) => _deferred += action;

        private void AddTrack(string key)
        {
            int index = _tracks.arraySize;
            _tracks.InsertArrayElementAtIndex(index);
            SerializedProperty track = _tracks.GetArrayElementAtIndex(index);
            track.FindPropertyRelative("Key").stringValue = key;
            SerializedProperty steps = track.FindPropertyRelative("Steps");
            steps.ClearArray();
            AddStep(steps);
            SessionState.SetBool($"Clouds.UIAnimationSequencer.{target.GetInstanceID()}.{index}", true);
        }

        // InsertArrayElementAtIndex chép nguyên phần tử trước đó — phải dọn sạch để bước mới thật sự rỗng.
        private static void AddStep(SerializedProperty steps)
        {
            int index = steps.arraySize;
            steps.InsertArrayElementAtIndex(index);
            SerializedProperty step = steps.GetArrayElementAtIndex(index);
            step.FindPropertyRelative("Delay").floatValue = 0f;
            step.FindPropertyRelative("Animations").ClearArray();
        }

        private static void AddReferences(SerializedProperty animations, List<TweenAnimationBase> anims)
        {
            foreach (var anim in anims)
            {
                bool exists = false;
                for (int i = 0; i < animations.arraySize; i++)
                    if (animations.GetArrayElementAtIndex(i).objectReferenceValue == anim) { exists = true; break; }
                if (exists) continue;

                int index = animations.arraySize;
                animations.InsertArrayElementAtIndex(index);
                animations.GetArrayElementAtIndex(index).objectReferenceValue = anim;
            }
        }

        // Xoá phần tử tham chiếu còn giá trị thì có bản Unity chỉ đặt nó về null; gán null trước để một
        // lần xoá là mất hẳn trên mọi phiên bản.
        private static void RemoveReference(SerializedProperty animations, int index)
        {
            animations.GetArrayElementAtIndex(index).objectReferenceValue = null;
            animations.DeleteArrayElementAtIndex(index);
        }

        private static void SetIgnoreTimeScale(TweenAnimationBase anim)
        {
            var so = new SerializedObject(anim);
            so.FindProperty(nameof(TweenAnimationBase.IgnoreTimeScale)).boolValue = true;
            so.ApplyModifiedProperties();
        }

        private bool HasKey(string key)
        {
            for (int i = 0; i < _tracks.arraySize; i++)
                if (_tracks.GetArrayElementAtIndex(i).FindPropertyRelative("Key").stringValue == key) return true;
            return false;
        }

        private string UniqueKey(string baseKey)
        {
            string key = baseKey;
            for (int n = 1; HasKey(key); n++) key = baseKey + n;
            return key;
        }

        // ── Hiển thị ─────────────────────────────────────────────────────────────

        private static string Format(float seconds) => float.IsInfinity(seconds) ? "∞" : $"{seconds:0.##}s";

        private static string RoleLabel(string key) => key switch
        {
            UIAnimationSequencer.SHOW => "thay transition mở",
            UIAnimationSequencer.HIDE => "thay transition đóng",
            _                         => null
        };

        private static Color HeaderTint(string key) => key switch
        {
            UIAnimationSequencer.SHOW => new Color(0.3f, 0.8f, 0.4f, 0.18f),
            UIAnimationSequencer.HIDE => new Color(0.9f, 0.4f, 0.3f, 0.18f),
            _                         => new Color(0.5f, 0.6f, 0.9f, 0.12f)
        };

        private static bool IconButton(string icon, string tooltip, bool enabled)
        {
            using (new EditorGUI.DisabledScope(!enabled))
                return GUILayout.Button(new GUIContent(icon, tooltip), EditorStyles.miniButton, GUILayout.Width(22));
        }

        private static GUIStyle _keyFieldStyle;
        private static GUIStyle KeyFieldStyle => _keyFieldStyle ??= new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold };

        private static GUIStyle _dropZoneStyle;
        private static GUIStyle DropZoneStyle => _dropZoneStyle ??= new GUIStyle(EditorStyles.helpBox)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic
        };
    }

    /// <summary>
    /// Phát thử một track ngoài Play Mode. UniTask không chạy khi chưa Play nên không dùng được
    /// UIAnimationSequencer.PlayAsync: ở đây lịch được tính trước từ Delay/Duration của từng bước và
    /// EditorApplication.update bấm giờ để khởi động từng animation đúng lúc. Loop không được phát lại
    /// (lịch chỉ khởi động mỗi animation một lần), mỗi animation chạy đúng một chu kỳ.
    /// </summary>
    internal class SequencerEditPreview
    {
        private const float HOLD_AFTER_END = 0.6f;   // giữ khung cuối một chút cho kịp nhìn rồi mới khôi phục

        private readonly List<(float at, TweenAnimationBase anim)> _schedule = new();
        private readonly List<(float start, float end)> _stepWindows = new();
        private readonly List<(GameObject go, AnimationSnapshot snapshot)> _snapshots = new();
        private DOTweenPreviewer _previewer;
        private double _startTime;
        private float _total;
        private int _next;

        public event Action Changed;
        public bool IsRunning { get; private set; }
        public string Key { get; private set; }
        public float Elapsed => (float)(EditorApplication.timeSinceStartup - _startTime);
        public float Progress => _total > 0f ? Elapsed / _total : 1f;

        public int ActiveStep
        {
            get
            {
                float t = Elapsed;
                for (int i = _stepWindows.Count - 1; i >= 0; i--)
                    if (t >= _stepWindows[i].start) return i;
                return -1;
            }
        }

        public void Start(UIAnimationSequencer sequencer, string key)
        {
            Stop();
            UIAnimationSequencer.Track track = sequencer.Find(key);
            if (track == null) return;

            float t = 0f;
            foreach (var step in track.Steps)
            {
                t += step.Delay;
                float stepLength = 0f;
                foreach (var anim in step.Animations)
                {
                    if (anim == null) continue;
                    _schedule.Add((t, anim));
                    stepLength = Mathf.Max(stepLength, anim.Duration);   // một chu kỳ — xem ghi chú lớp
                }
                _stepWindows.Add((t, t + stepLength));
                t += stepLength;
            }
            _total = t;

            foreach (var anim in UIAnimationSequencer.DistinctAnimations(track))
                _snapshots.Add((anim.gameObject, AnimationSnapshot.Capture(anim.gameObject, AnimationSnapshot.ChannelsOf(anim.UIAnimationData))));

            Key        = key;
            IsRunning  = true;
            _startTime = EditorApplication.timeSinceStartup;

            // PrimeTween tự chạy tween trong Edit Mode; DOTween thì phải đăng ký từng tween vào
            // DOTweenEditorPreview mới được cập nhật.
            _previewer = TweenCUIAnimation.AnimationFactory is DOTweenAnimationFactory ? new DOTweenPreviewer() : null;
            _previewer?.Start();
            EditorApplication.update += Tick;
            Tick();
        }

        public void Stop()
        {
            if (IsRunning)
            {
                EditorApplication.update -= Tick;
                foreach (var (_, anim) in _schedule) if (anim != null) anim.Stop();
                _previewer?.Stop();

                // Ngược thứ tự chụp: hai animation cùng chạm một object thì bản chụp đầu mới là gốc.
                for (int i = _snapshots.Count - 1; i >= 0; i--)
                    _snapshots[i].snapshot.Restore(_snapshots[i].go);

                SceneView.RepaintAll();
            }

            _previewer = null;
            _schedule.Clear();
            _stepWindows.Clear();
            _snapshots.Clear();
            _next     = 0;
            Key       = null;
            IsRunning = false;
            Changed?.Invoke();
        }

        private void Tick()
        {
            float elapsed = Elapsed;
            while (_next < _schedule.Count && _schedule[_next].at <= elapsed)
            {
                TweenAnimationBase anim = _schedule[_next].anim;
                _next++;
                if (anim == null) continue;

                anim.Rebuild();
                foreach (IUIAnimation tween in anim.Animations)
                {
                    _previewer?.Prepare(tween);
                    tween.Restart();
                }
            }

            if (elapsed > _total + HOLD_AFTER_END)
            {
                Stop();
                return;
            }
            Changed?.Invoke();
        }
    }
}
#endif
