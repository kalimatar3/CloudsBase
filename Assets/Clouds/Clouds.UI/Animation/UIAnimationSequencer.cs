using System;
using System.Collections.Generic;
using System.Threading;
using Clouds.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Clouds.UI
{
    /// <summary>
    /// Dàn dựng animation cho NHIỀU object. TweenCUIAnimation mô tả một object chuyển động thế nào;
    /// component này mô tả cả nhóm object chuyển động theo thứ tự nào.
    ///
    /// Mỗi key là một track gồm các bước chạy NỐI TIẾP; trong một bước, mọi animation chạy SONG SONG
    /// và bước kết thúc khi animation dài nhất xong. Nối tiếp thuần = mỗi bước một animation, song song
    /// thuần = một bước nhiều animation.
    ///
    /// Đặt trên root của một UIView (Popup/Screen/Activity), key "Show"/"Hide" sẽ thay cho transition
    /// mặc định lấy từ UINavigatorConfig.
    ///
    ///   await _sequencer.PlayAsync("Reward");
    ///   _sequencer.Play("Idle");
    /// </summary>
    [DisallowMultipleComponent]
    public class UIAnimationSequencer : MonoBehaviour
    {
        public const string SHOW = "Show";
        public const string HIDE = "Hide";

        [Serializable]
        public class Step
        {
            [Min(0f)] public float Delay;
            public List<TweenAnimationBase> Animations = new();

            /// <summary>Thời lượng của bước, không tính Delay. Vô hạn nếu có animation loop mãi.</summary>
            public float Duration
            {
                get
                {
                    float max = 0f;
                    foreach (var anim in Animations)
                        if (anim != null) max = Mathf.Max(max, LengthOf(anim));
                    return max;
                }
            }
        }

        [Serializable]
        public class Track
        {
            public string Key;
            public List<Step> Steps = new();

            public float Duration
            {
                get
                {
                    float total = 0f;
                    foreach (var step in Steps) total += step.Delay + step.Duration;
                    return total;
                }
            }
        }

        [SerializeField] private List<Track> _tracks = new();

        [Tooltip("Delay giữa các bước chạy theo thời gian thực, không bị Time.timeScale ảnh hưởng.")]
        [SerializeField] private bool _ignoreTimeScale = true;

        private readonly Dictionary<string, CancellationTokenSource> _running = new();

        public IReadOnlyList<Track> Tracks => _tracks;

        public bool HasKey(string key) => Find(key) != null;

        public Track Find(string key)
        {
            foreach (var track in _tracks)
                if (track.Key == key) return track;
            return null;
        }

        public float GetDuration(string key) => Find(key)?.Duration ?? 0f;

        // ── Phát ─────────────────────────────────────────────────────────────────

        /// <summary>Bản fire-and-forget. onComplete không được gọi nếu track bị Stop giữa chừng.</summary>
        public void Play(string key, Action onComplete = null) => PlayAndForget(key, onComplete).Forget();

        private async UniTaskVoid PlayAndForget(string key, Action onComplete)
        {
            try { await PlayAsync(key); }
            catch (OperationCanceledException) { return; }
            onComplete?.Invoke();
        }

        /// <summary>
        /// Phát track theo key rồi đợi tới bước cuối xong. Key không tồn tại thì hoàn thành ngay.
        /// Phát lại một key đang chạy thì lần trước bị huỷ. Track chứa animation loop vô hạn thì không
        /// bao giờ xong — đừng await nó.
        /// </summary>
        public async UniTask PlayAsync(string key, CancellationToken cancellationToken = default)
        {
            Track track = Find(key);
            if (track == null) return;

            Stop(key);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, destroyCancellationToken);
            _running[key] = cts;

            try
            {
                foreach (var step in track.Steps)
                {
                    if (step.Delay > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(step.Delay), _ignoreTimeScale,
                                            cancellationToken: cts.Token);
                    await PlayStepAsync(step, cts.Token);
                }
            }
            finally
            {
                if (_running.TryGetValue(key, out var current) && current == cts) _running.Remove(key);
                cts.Dispose();
            }
        }

        private static UniTask PlayStepAsync(Step step, CancellationToken token)
        {
            var tasks = new List<UniTask>(step.Animations.Count);
            foreach (var anim in step.Animations)
            {
                if (anim == null) continue;
                // Dựng lại trước mỗi lần phát: backend DOTween Kill tween khi Stop(), phát lại tween đã
                // chết thì không có gì chạy và await treo mãi. Dựng mới cũng giúp preset "From = giá trị
                // hiện tại" lấy đúng giá trị lúc bước này bắt đầu chứ không phải lúc Awake.
                anim.Rebuild();
                tasks.Add(AnimationService.PlayAsync(anim, token));
            }
            return UniTask.WhenAll(tasks);
        }

        // ── Dừng / khôi phục ─────────────────────────────────────────────────────

        /// <summary>Dừng track, object giữ nguyên trạng thái đang dở.</summary>
        public void Stop(string key)
        {
            if (_running.Remove(key, out var cts)) cts.Cancel();

            Track track = Find(key);
            if (track == null) return;
            foreach (var anim in DistinctAnimations(track)) anim.Rebuild();   // Rebuild = Stop + dựng lại tween sẵn sàng phát
        }

        public void StopAll()
        {
            foreach (var track in _tracks) Stop(track.Key);
        }

        /// <summary>
        /// Dừng track và đưa mọi object về trạng thái ngay trước lần phát gần nhất. UIView gọi sau khi
        /// Hide xong để lần mở kế tiếp (popup lấy từ pool) không bắt đầu từ trạng thái đã ẩn.
        /// </summary>
        public void Restore(string key)
        {
            if (_running.Remove(key, out var cts)) cts.Cancel();

            Track track = Find(key);
            if (track == null) return;

            // Đi ngược: hai animation cùng chạm một object thì snapshot của cái phát trước mới là
            // trạng thái gốc, phải được ghi sau cùng.
            List<TweenAnimationBase> anims = DistinctAnimations(track);
            for (int i = anims.Count - 1; i >= 0; i--)
            {
                AnimationService.Restore(anims[i]);
                anims[i].Rebuild();
            }
        }

        // ── Tiện ích ─────────────────────────────────────────────────────────────

        /// <summary>Thời lượng thật của một animation, tính cả loop hữu hạn.</summary>
        public static float LengthOf(TweenAnimationBase anim)
        {
            UIAnimationData data = anim.UIAnimationData;
            if (data == null) return 0f;
            float cycle = data.GetTotalDuration();
            if (!data.Loop) return cycle;
            return data.LoopCount <= 0 ? float.PositiveInfinity : cycle * data.LoopCount;
        }

        public static List<TweenAnimationBase> DistinctAnimations(Track track)
        {
            var result = new List<TweenAnimationBase>();
            foreach (var step in track.Steps)
                foreach (var anim in step.Animations)
                    if (anim != null && !result.Contains(anim)) result.Add(anim);
            return result;
        }
    }
}
