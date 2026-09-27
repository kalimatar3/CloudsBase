using Cysharp.Threading.Tasks;

namespace Clouds.UI
{
    /// <summary>
    /// Mở/đóng popup. Key mặc định = tên class, nên prefab đặt tên trùng class trong
    /// Assets/Game.Addressable/ là mở được không cần khai báo gì thêm (xem AddressableFolderSync).
    /// layer = null → PopupLayer đầu tiên trong scene.
    ///
    ///   await PopupService.ShowAsync&lt;ShopPopup&gt;();
    ///   var confirm = await PopupService.ShowAsync&lt;ConfirmPopup, ConfirmArgs&gt;(args);
    ///   await confirm.WaitHiddenAsync();
    ///   if (confirm.Accepted) ...
    /// </summary>
    public static class PopupService
    {
        public static UniTask<T> ShowAsync<T>(string key = null, string layer = null) where T : PopupView
        {
            PopupLayer target = UILayer.Find<PopupLayer>(layer);
            return target != null ? target.ShowAsync<T>(key ?? typeof(T).Name) : UniTask.FromResult<T>(null);
        }

        public static UniTask<T> ShowAsync<T, TArgs>(TArgs args, string key = null, string layer = null)
            where T : PopupView, IViewArgs<TArgs>
        {
            PopupLayer target = UILayer.Find<PopupLayer>(layer);
            return target != null
                ? target.ShowAsync<T>(key ?? typeof(T).Name, popup => popup.SetArgs(args))
                : UniTask.FromResult<T>(null);
        }

        /// <summary>Mở theo key cho popup không có class riêng.</summary>
        public static UniTask<PopupView> ShowAsync(string key, string layer = null) => ShowAsync<PopupView>(key, layer);

        public static UniTask CloseAsync(PopupView popup) => popup != null ? popup.CloseAsync() : UniTask.CompletedTask;

        public static UniTask CloseTopAsync(string layer = null)
        {
            PopupLayer target = UILayer.Find<PopupLayer>(layer);
            return target != null ? target.CloseTopAsync() : UniTask.CompletedTask;
        }

        public static UniTask CloseAllAsync(string layer = null)
        {
            PopupLayer target = UILayer.Find<PopupLayer>(layer);
            return target != null ? target.CloseAllAsync() : UniTask.CompletedTask;
        }

        public static PopupView GetTop(string layer = null)
        {
            PopupLayer target = UILayer.Find<PopupLayer>(layer);
            return target != null ? target.Top : null;
        }

        /// <summary>Back/Esc: đóng popup trên cùng của layer vẽ trên cùng. true nếu có popup nhận phím.</summary>
        public static bool HandleBack()
        {
            foreach (PopupLayer layer in UILayer.AllTopFirst<PopupLayer>())
                if (layer.HandleBack()) return true;
            return false;
        }
    }
}
