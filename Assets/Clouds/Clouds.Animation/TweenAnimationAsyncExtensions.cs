using System;
using System.Threading;
using Clouds.UI;
using Cysharp.Threading.Tasks;

namespace Clouds.Animation
{
    /// <summary>
    /// Lớp await cho animation, dựng trên event OnComplete có sẵn nên chạy được với mọi backend
    /// (DOTween, PrimeTween, LitMotion) mà không phải sửa IUIAnimation.
    ///
    /// Không đổi thẳng Play()/PlayReverse()/Restart() thành UniTask vì UIAnimationContainerEditor gọi
    /// Restart() ở edit mode để preview, mà PlayerLoop của UniTask chỉ chạy trong Play mode — task tạo
    /// ra ở đó sẽ không bao giờ hoàn thành.
    ///
    /// LƯU Ý: animation loop vô hạn (UIAnimationData.Loop = true và LoopCount &lt;= 0) không bao giờ bắn
    /// OnComplete, nên await nó là treo vĩnh viễn. Luôn truyền CancellationToken cho những trường hợp
    /// này, hoặc đừng await chúng.
    /// </summary>
    public static class TweenAnimationAsyncExtensions
    {
        /// <summary>Phát animation rồi đợi tới khi chạy xong (đã tính cả loop hữu hạn).</summary>
        public static UniTask PlayAsync(this IUIAnimation animation, CancellationToken cancellationToken = default)
            => Await(animation, animation.Play, cancellationToken);

        public static UniTask PlayReverseAsync(this IUIAnimation animation, CancellationToken cancellationToken = default)
            => Await(animation, animation.PlayReverse, cancellationToken);

        public static UniTask RestartAsync(this IUIAnimation animation, CancellationToken cancellationToken = default)
            => Await(animation, animation.Restart, cancellationToken);

        /// <summary>
        /// Bản async của UIAnimationContainer.Play(key). Key không tồn tại thì hoàn thành ngay, giống
        /// hệt đường callback đồng bộ.
        /// </summary>
        public static UniTask PlayAsync(this UIAnimationContainer container, string key,
                                        CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled(cancellationToken);

            var source = new UniTaskCompletionSource();
            CancellationTokenRegistration registration = default;

            if (cancellationToken.CanBeCanceled)
                registration = cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));

            container.Play(key, onComplete: () =>
            {
                registration.Dispose();
                source.TrySetResult();
            });

            return source.Task;
        }

        private static UniTask Await(IUIAnimation animation, Action start, CancellationToken cancellationToken)
        {
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

            // Stop() giữa chừng KHÔNG bắn OnComplete (DOTween Kill lặng lẽ), và object bị Destroy thì
            // continuation sẽ chạy trên GameObject đã chết. Token là lối thoát cho cả hai — gọi kèm
            // this.GetCancellationTokenOnDestroy() là xử lý xong vế thứ hai.
            if (cancellationToken.CanBeCanceled)
                registration = cancellationToken.Register(() =>
                {
                    animation.OnComplete -= OnFinished;
                    source.TrySetCanceled(cancellationToken);
                });

            start();
            return source.Task;
        }
    }
}
