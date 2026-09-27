using Cysharp.Threading.Tasks;

namespace Clouds.UI
{
    /// <summary>
    /// Điều hướng giữa các màn hình (stack có lịch sử). Key mặc định = tên class; layer = null →
    /// ScreenLayer đầu tiên trong scene.
    ///
    ///   await ScreenService.PushAsync&lt;ShopScreen&gt;();
    ///   await ScreenService.PopAsync();
    /// </summary>
    public static class ScreenService
    {
        public static UniTask<T> PushAsync<T>(string key = null, string layer = null) where T : ScreenView
        {
            ScreenLayer target = UILayer.Find<ScreenLayer>(layer);
            return target != null ? target.PushAsync<T>(key ?? typeof(T).Name) : UniTask.FromResult<T>(null);
        }

        public static UniTask<T> PushAsync<T, TArgs>(TArgs args, string key = null, string layer = null)
            where T : ScreenView, IViewArgs<TArgs>
        {
            ScreenLayer target = UILayer.Find<ScreenLayer>(layer);
            return target != null
                ? target.PushAsync<T>(key ?? typeof(T).Name, screen => screen.SetArgs(args))
                : UniTask.FromResult<T>(null);
        }

        public static UniTask<ScreenView> PushAsync(string key, string layer = null) => PushAsync<ScreenView>(key, layer);

        public static UniTask PopAsync(string layer = null)
        {
            ScreenLayer target = UILayer.Find<ScreenLayer>(layer);
            return target != null ? target.PopAsync() : UniTask.CompletedTask;
        }

        public static ScreenView GetCurrent(string layer = null)
        {
            ScreenLayer target = UILayer.Find<ScreenLayer>(layer);
            return target != null ? target.Current : null;
        }

        /// <summary>Back/Esc: pop màn trên cùng của layer vẽ trên cùng còn lịch sử. true nếu đã pop.</summary>
        public static bool HandleBack()
        {
            foreach (ScreenLayer layer in UILayer.AllTopFirst<ScreenLayer>())
                if (layer.HandleBack()) return true;
            return false;
        }
    }
}
