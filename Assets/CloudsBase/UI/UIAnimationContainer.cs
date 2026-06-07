using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Clouds.UI.Animation;
using Clouds.UI.Settings;
using Clouds.Ultilities;

/// <summary>
/// Gắn component này lên bất kỳ UI GameObject nào để quản lý và phát animation.
/// Mỗi animation được đặt tên (key). Gọi Play("Show"), Play("Hide"), Play("Click")...
///
/// Ví dụ từ code:
///   _animContainer.Play("Show");
///   _animContainer.Play("Hide", onComplete: () => gameObject.SetActive(false));
///   _animContainer.Play("Click", OnClickStarted, OnClickCompleted);
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIAnimationContainer : MonoBehaviour
{
    [Serializable]
    public struct AnimationEntry
    {
        [HorizontalGroup(Width = 100), HideLabel]
        public string Key;

        [HorizontalGroup, HideLabel]
        public UIAnimationData Data;
    }

    private static IUIAnimationFactory _factory;
    public static IUIAnimationFactory AnimationFactory
    {
        get
        {
            if (_factory == null) _factory = UISetting.Instance.GetFactory();
            return _factory;
        }
    }

    [ListDrawerSettings(ShowIndexLabels = false, DraggableItems = true)]
    [SerializeField] private List<AnimationEntry> _entries = new();

    private readonly Dictionary<string, List<IUIAnimation>> _runtime = new();

    public IReadOnlyList<AnimationEntry> Entries => _entries;

    private void Awake() => Build();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Phát animation theo key.
    /// onStart: gọi khi animation đầu tiên bắt đầu.
    /// onComplete: gọi khi animation cuối cùng kết thúc.
    /// Nếu key không tồn tại, cả hai callback đều gọi ngay.
    /// </summary>
    public void Play(string key, Action onStart = null, Action onComplete = null)
    {
        if (!_runtime.TryGetValue(key, out var anims) || anims.Count == 0)
        {
            onStart?.Invoke();
            onComplete?.Invoke();
            return;
        }

        if (onStart    != null) HookOneShot(anims[0],    isStart: true,  callback: onStart);
        if (onComplete != null) HookOneShot(anims[^1], isStart: false, callback: onComplete);

        foreach (var anim in anims) anim.Restart();
    }

    public void Stop(string key)
    {
        if (_runtime.TryGetValue(key, out var anims))
            foreach (var a in anims) a.Stop();
    }

    public void StopAll()
    {
        foreach (var kvp in _runtime)
            foreach (var a in kvp.Value) a.Stop();
    }

    public bool HasKey(string key) => _runtime.ContainsKey(key);

    /// <summary>Lấy danh sách IUIAnimation đã build cho key (dùng bởi Editor preview).</summary>
    public IReadOnlyList<IUIAnimation> GetAnimations(string key)
    {
        if (_runtime.TryGetValue(key, out var list)) return list;
        return Array.Empty<IUIAnimation>();
    }

    /// <summary>Build lại toàn bộ runtime animations. Gọi trong Awake và khi data thay đổi.</summary>
    public void Rebuild() => Build();

    // ── Internal ──────────────────────────────────────────────────────────────

    private void Build()
    {
        _runtime.Clear();
        var rt      = GetComponent<RectTransform>();
        var cg      = GetComponent<CanvasGroup>();
        var graphic = GetComponent<Graphic>();

        foreach (var entry in _entries)
        {
            if (string.IsNullOrEmpty(entry.Key) || entry.Data == null) continue;
            _runtime[entry.Key] = BuildAnimations(entry.Data, rt, cg, graphic);
        }
    }

    private static List<IUIAnimation> BuildAnimations(
        UIAnimationData data, RectTransform rt, CanvasGroup cg, Graphic graphic)
    {
        var list = new List<IUIAnimation>();
        foreach (var effect in data.Effects)
        {
            IUIAnimation anim = effect.type switch
            {
                TRIGGEREFFECT.Move   => AnimationFactory.CreateMove(rt, effect),
                TRIGGEREFFECT.Rotate => AnimationFactory.CreateRotate(rt, effect),
                TRIGGEREFFECT.Scale  => AnimationFactory.CreateScale(rt, effect),
                TRIGGEREFFECT.Shake  => AnimationFactory.CreateShake(rt, effect),
                TRIGGEREFFECT.Punch  => AnimationFactory.CreatePunch(rt, effect),
                TRIGGEREFFECT.Fade   => cg      != null ? AnimationFactory.CreateFade(cg, effect)       : null,
                TRIGGEREFFECT.Color  => graphic != null ? AnimationFactory.CreateColor(graphic, effect) : null,
                _                    => null
            };
            if (anim != null) list.Add(anim);
        }
        return list;
    }

    private static void HookOneShot(IUIAnimation target, bool isStart, Action callback)
    {
        if (isStart)
        {
            Action wrapper = null;
            wrapper = () => { callback(); target.OnStart -= wrapper; };
            target.OnStart += wrapper;
        }
        else
        {
            Action wrapper = null;
            wrapper = () => { callback(); target.OnComplete -= wrapper; };
            target.OnComplete += wrapper;
        }
    }
}
