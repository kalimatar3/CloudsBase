using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Clouds.UI
{
    /// <summary>
    /// Lớp phủ không xếp chồng: mỗi key bật/tắt độc lập, hiện tối đa một bản. Gọi qua ActivityService.
    /// </summary>
    public class ActivityLayer : UILayer
    {
        private readonly Dictionary<string, ActivityView> _active = new();

        public IReadOnlyCollection<ActivityView> Activities => _active.Values;

        public bool IsShowing(string key) => _active.ContainsKey(key);

        /// <summary>Key đang hiện thì trả về bản đang hiện, không chạy lại transition.</summary>
        public UniTask<T> ShowAsync<T>(string key, Action<T> beforeShow = null) where T : ActivityView
            => Enqueue(async () =>
            {
                if (_active.TryGetValue(key, out ActivityView existing)) return existing as T;

                T activity = await AcquireAsync<T>(key);
                if (activity == null) return null;

                beforeShow?.Invoke(activity);
                activity.transform.SetAsLastSibling();
                _active[key] = activity;

                activity.PrepareShow();
                await activity.WillShowAsync();
                await activity.PlayTransitionAsync(true, Config.ActivityShow, Config.IgnoreTimeScale);
                activity.DidShow();
                return activity;
            });

        public UniTask HideAsync(string key)
            => Enqueue(async () =>
            {
                if (_active.TryGetValue(key, out ActivityView activity)) await HideInternalAsync(activity);
            });

        public UniTask HideAllAsync()
            => Enqueue(async () =>
            {
                var tasks = new List<UniTask>();
                foreach (ActivityView activity in new List<ActivityView>(_active.Values)) tasks.Add(HideInternalAsync(activity));
                await UniTask.WhenAll(tasks);
            });

        public override UniTask CloseAsync(UIView view)
            => view != null ? HideAsync(view.Key) : UniTask.CompletedTask;

        private async UniTask HideInternalAsync(ActivityView activity)
        {
            await activity.WillHideAsync();
            await activity.PlayTransitionAsync(false, Config.ActivityHide, Config.IgnoreTimeScale);
            _active.Remove(activity.Key);
            activity.DidHide();
            Release(activity);
        }
    }
}
