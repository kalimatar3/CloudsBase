using System;
using System.Threading;
using Clouds.UI;
using Cysharp.Threading.Tasks;

namespace Clouds.Animation
{
    /// <summary>
    /// Cửa vào duy nhất để phát animation, cả sync lẫn async.
    ///
    /// Play() của TweenAnimationBase và UIAnimationContainer đều chỉ là forwarder gọi vào đây, nên gọi
    /// đường nào cũng đi qua service — kể cả nút Odin trong Inspector hay editor preview. Nhờ vậy có
    /// đúng MỘT chỗ chụp snapshot và quyết định có dựng lại tween hay không.
    ///
    /// Snapshot được chụp ở mỗi lần phát xuôi (Play), KHÔNG chụp ở PlayReverse — chiều ngược là đường
    /// về, nó phải quay lại đúng mốc mà lần phát xuôi vừa ghi.
    /// </summary>
    public static class AnimationService
    {
        // ── TweenAnimationBase ───────────────────────────────────────────────────

        public static void Play(TweenAnimationBase animation)
        {
            if (animation == null) return;
            Prepare(animation);
            animation.PlayInternal();
        }

        public static void PlayReverse(TweenAnimationBase animation)
        {
            if (animation == null) return;
            animation.PlayReverseInternal();
        }

        public static UniTask PlayAsync(TweenAnimationBase animation, CancellationToken cancellationToken = default)
            => Await(animation, () => Play(animation), cancellationToken);

        public static UniTask PlayReverseAsync(TweenAnimationBase animation, CancellationToken cancellationToken = default)
            => Await(animation, () => PlayReverse(animation), cancellationToken);

        /// <summary>
        /// Dừng animation và đặt target về đúng trạng thái trước lần phát gần nhất. Dùng thay cho việc
        /// tự dựng một preset chạy ngược: nó tức thời và không bị nháy hình như PlayReverse() (vốn nhảy
        /// tới điểm kết thúc trước rồi mới chạy lùi).
        /// </summary>
        public static void Restore(TweenAnimationBase animation)
        {
            if (animation == null) return;
            animation.Stop();
            animation.Snapshot.Restore(animation.gameObject);
            animation.Snapshot = default;
        }

        // ── UIAnimationContainer ─────────────────────────────────────────────────

        public static void Play(UIAnimationContainer container, string key,
                                Action onStart = null, Action onComplete = null)
        {
            if (container == null) return;

            UIAnimationData data = container.DataFor(key);
            container.Snapshot = AnimationSnapshot.Capture(container.gameObject, AnimationSnapshot.ChannelsOf(data));
            if (data != null && data.HasFromCurrent) container.RebuildKey(key);

            container.PlayInternal(key, onStart, onComplete);
        }

        public static UniTask PlayAsync(UIAnimationContainer container, string key,
                                        CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled(cancellationToken);

            var source = new UniTaskCompletionSource();
            CancellationTokenRegistration registration = default;

            if (cancellationToken.CanBeCanceled)
                registration = cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));

            Play(container, key, onComplete: () =>
            {
                registration.Dispose();
                source.TrySetResult();
            });

            return source.Task;
        }

        public static void Restore(UIAnimationContainer container)
        {
            if (container == null) return;
            container.StopAll();
            container.Snapshot.Restore(container.gameObject);
            container.Snapshot = default;
        }

        // ── IUIAnimation trần ────────────────────────────────────────────────────
        // Dùng khi chỉ cầm được interface (ví dụ InterfaceReference<IUIAnimation> trong Inspector).
        // Nếu đằng sau là một TweenAnimationBase thì Play() của nó vẫn forward về service nên snapshot
        // vẫn được chụp; nếu là backend khác thì đơn giản là không có snapshot.

        public static void Play(IUIAnimation animation) => animation?.Play();

        public static void PlayReverse(IUIAnimation animation) => animation?.PlayReverse();

        public static UniTask PlayAsync(IUIAnimation animation, CancellationToken cancellationToken = default)
            => Await(animation, () => animation.Play(), cancellationToken);

        public static UniTask PlayReverseAsync(IUIAnimation animation, CancellationToken cancellationToken = default)
            => Await(animation, () => animation.PlayReverse(), cancellationToken);

        public static void Restore(IUIAnimation animation)
        {
            if (animation is TweenAnimationBase component) Restore(component);
        }

        // ── Chờ animation xong ───────────────────────────────────────────────────

        /// <summary>
        /// Chạy start() rồi đợi OnComplete. Dựng trên event có sẵn nên chạy được với mọi backend.
        ///
        /// LƯU Ý: animation loop vô hạn (Loop = true, LoopCount &lt;= 0) không bao giờ bắn OnComplete
        /// nên await nó là treo vĩnh viễn. Stop() giữa chừng cũng không bắn OnComplete, và object bị
        /// Destroy thì continuation sẽ chạy trên GameObject đã chết — CancellationToken là lối thoát
        /// cho cả hai, truyền this.GetCancellationTokenOnDestroy() là xong vế thứ hai.
        /// </summary>
        public static UniTask Await(IUIAnimation animation, Action start, CancellationToken cancellationToken = default)
        {
            if (animation == null) return UniTask.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled(cancellationToken);

            var source = new UniTaskCompletionSource();
            CancellationTokenRegistration registration = default;

            void OnFinished()
            {
                animation.OnComplete -= OnFinished;
                registration.Dispose();
                source.TrySetResult();
            }

            // Đăng ký trước khi start(): animation rỗng hoặc duration 0 bắn OnComplete ngay trong
            // start(), đăng ký sau là bỏ lỡ tín hiệu và await treo luôn.
            animation.OnComplete += OnFinished;

            if (cancellationToken.CanBeCanceled)
                registration = cancellationToken.Register(() =>
                {
                    animation.OnComplete -= OnFinished;
                    source.TrySetCanceled(cancellationToken);
                });

            start();
            return source.Task;
        }

        // ── Internal ─────────────────────────────────────────────────────────────

        private static void Prepare(TweenAnimationBase animation)
        {
            UIAnimationData data = animation.UIAnimationData;
            animation.Snapshot = AnimationSnapshot.Capture(animation.gameObject, AnimationSnapshot.ChannelsOf(data));

            // Tween chụp giá trị From ngay lúc dựng, nên "From = giá trị hiện tại" chỉ đúng nếu dựng
            // lại sát lúc phát. Chỉ trả cái giá đó khi preset thật sự cần.
            if (data != null && data.HasFromCurrent) animation.Rebuild();
        }
    }
}
