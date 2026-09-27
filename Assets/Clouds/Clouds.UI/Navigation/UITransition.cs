using System.Collections.Generic;
using System.Threading;
using Clouds.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Clouds.UI
{
    /// <summary>Phát một UIAnimationData trên target mà không cần component nào gắn sẵn.</summary>
    internal static class UITransition
    {
        public static async UniTask PlayAsync(UIAnimationData data, GameObject target, bool ignoreTimeScale,
                                              CancellationToken cancellationToken)
        {
            if (data == null || target == null) return;

            // Dựng mới mỗi lần: transition chạy thưa, và dựng sát lúc phát thì preset "From = giá trị
            // hiện tại" lấy đúng trạng thái lúc này.
            List<IUIAnimation> anims = UIAnimationBuilder.Build(TweenCUIAnimation.AnimationFactory, data, target, ignoreTimeScale);
            if (anims.Count == 0) return;

            try
            {
                var tasks = new UniTask[anims.Count];
                for (int i = 0; i < anims.Count; i++)
                {
                    IUIAnimation anim = anims[i];
                    tasks[i] = AnimationService.Await(anim, anim.Restart, cancellationToken);
                }
                await UniTask.WhenAll(tasks);
            }
            finally
            {
                // Tween dựng với AutoKill(false): không Kill thì nó sống tới khi target bị huỷ.
                foreach (var anim in anims) anim.Stop();
            }
        }
    }
}
