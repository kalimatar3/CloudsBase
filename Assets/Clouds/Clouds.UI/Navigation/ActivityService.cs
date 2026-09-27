using Cysharp.Threading.Tasks;

namespace Clouds.UI
{
    /// <summary>
    /// Bật/tắt lớp phủ (toast, tutorial, loading, chặn chạm). Key mặc định = tên class; layer = null →
    /// ActivityLayer đầu tiên trong scene.
    ///
    ///   await ActivityService.ShowAsync&lt;LoadingActivity&gt;();
    ///   await ActivityService.HideAsync&lt;LoadingActivity&gt;();
    /// </summary>
    public static class ActivityService
    {
        public static UniTask<T> ShowAsync<T>(string key = null, string layer = null) where T : ActivityView
        {
            ActivityLayer target = UILayer.Find<ActivityLayer>(layer);
            return target != null ? target.ShowAsync<T>(key ?? typeof(T).Name) : UniTask.FromResult<T>(null);
        }

        public static UniTask<T> ShowAsync<T, TArgs>(TArgs args, string key = null, string layer = null)
            where T : ActivityView, IViewArgs<TArgs>
        {
            ActivityLayer target = UILayer.Find<ActivityLayer>(layer);
            return target != null
                ? target.ShowAsync<T>(key ?? typeof(T).Name, activity => activity.SetArgs(args))
                : UniTask.FromResult<T>(null);
        }

        public static UniTask<ActivityView> ShowAsync(string key, string layer = null) => ShowAsync<ActivityView>(key, layer);

        public static UniTask HideAsync<T>(string layer = null) where T : ActivityView => HideAsync(typeof(T).Name, layer);

        public static UniTask HideAsync(string key, string layer = null)
        {
            ActivityLayer target = UILayer.Find<ActivityLayer>(layer);
            return target != null ? target.HideAsync(key) : UniTask.CompletedTask;
        }

        public static UniTask HideAllAsync(string layer = null)
        {
            ActivityLayer target = UILayer.Find<ActivityLayer>(layer);
            return target != null ? target.HideAllAsync() : UniTask.CompletedTask;
        }

        public static bool IsShowing(string key, string layer = null)
        {
            ActivityLayer target = UILayer.Find<ActivityLayer>(layer);
            return target != null && target.IsShowing(key);
        }
    }
}
