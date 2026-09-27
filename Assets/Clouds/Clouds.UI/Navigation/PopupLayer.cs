using System;
using System.Collections.Generic;
using Clouds.Common;
using Clouds.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Clouds.UI
{
    /// <summary>
    /// Chồng popup. Popup mới nằm trên, popup cũ vẫn hiện bên dưới; mỗi popup có một backdrop riêng nằm
    /// ngay dưới nó. Gọi qua PopupService thay vì cầm layer trực tiếp.
    /// </summary>
    public class PopupLayer : UILayer
    {
        private readonly List<PopupView> _stack = new();
        private readonly Dictionary<PopupView, UIBackdrop> _backdrops = new();

        public IReadOnlyList<PopupView> Popups => _stack;
        public PopupView Top => _stack.Count > 0 ? _stack[^1] : null;

        public UniTask<T> ShowAsync<T>(string key, Action<T> beforeShow = null) where T : PopupView
            => Enqueue(async () =>
            {
                T popup = await AcquireAsync<T>(key);
                if (popup == null) return null;

                // Popup đặt sẵn trong scene chỉ có một bản: đang mở thì trả về chính nó.
                if (_stack.Contains(popup)) return popup;

                beforeShow?.Invoke(popup);
                popup.transform.SetAsLastSibling();
                UIBackdrop backdrop = popup.UseBackdrop ? await CreateBackdropAsync(popup) : null;
                _stack.Add(popup);

                popup.PrepareShow();
                await popup.WillShowAsync();
                await UniTask.WhenAll(
                    popup.PlayTransitionAsync(true, Config.PopupShow, Config.IgnoreTimeScale),
                    backdrop != null ? backdrop.PlayAsync(true, Config.BackdropShow, Config.IgnoreTimeScale, destroyCancellationToken)
                                     : UniTask.CompletedTask);
                popup.DidShow();
                return popup;
            });

        public override UniTask CloseAsync(UIView view)
            => view is PopupView popup ? CloseAsync(popup) : UniTask.CompletedTask;

        public UniTask CloseAsync(PopupView popup)
            => Enqueue(async () =>
            {
                // Đóng hai lần liền (bấm nút đóng + Back) thì lần sau thấy popup đã rời stack và bỏ qua.
                if (popup == null || !_stack.Contains(popup)) return;
                await HideAsync(popup);
            });

        public UniTask CloseTopAsync()
            => Enqueue(async () =>
            {
                if (Top != null) await HideAsync(Top);
            });

        public UniTask CloseAllAsync()
            => Enqueue(async () =>
            {
                var tasks = new List<UniTask>();
                foreach (PopupView popup in _stack.ToArray()) tasks.Add(HideAsync(popup));
                await UniTask.WhenAll(tasks);
            });

        /// <summary>Back/Esc: đóng popup trên cùng. true = layer này đã nhận phím (kể cả khi popup không cho đóng).</summary>
        internal bool HandleBack()
        {
            PopupView top = Top;
            if (top == null) return false;
            if (top.CloseOnBack) CloseAsync(top).Forget();
            return true;
        }

        private async UniTask HideAsync(PopupView popup)
        {
            _backdrops.Remove(popup, out UIBackdrop backdrop);

            await popup.WillHideAsync();
            await UniTask.WhenAll(
                popup.PlayTransitionAsync(false, Config.PopupHide, Config.IgnoreTimeScale),
                backdrop != null ? backdrop.PlayAsync(false, Config.BackdropHide, Config.IgnoreTimeScale, destroyCancellationToken)
                                 : UniTask.CompletedTask);

            _stack.Remove(popup);
            popup.DidHide();
            if (backdrop != null) Destroy(backdrop.gameObject);
            Release(popup);
        }

        private async UniTask<UIBackdrop> CreateBackdropAsync(PopupView popup)
        {
            UINavigatorConfig config = Config;
            UIBackdrop backdrop = null;

            if (!string.IsNullOrEmpty(config.BackdropKey))
            {
                GameObject prefab = await AssetService.LoadAsync<GameObject>(config.BackdropKey);
                if (prefab != null)
                {
                    GameObject instance = Instantiate(prefab, transform, false);
                    backdrop = instance.GetOrAddComponent<UIBackdrop>();
                }
            }

            // Prefab nạp hỏng thì vẫn phải có backdrop, không thì chạm xuyên xuống layer bên dưới.
            if (backdrop == null) backdrop = UIBackdrop.CreateDefault(transform, popup.BackdropColor(config));

            backdrop.GetComponent<CanvasGroup>().alpha = 0f;   // OnWillShowAsync có thể kéo dài nhiều frame
            backdrop.transform.SetSiblingIndex(popup.transform.GetSiblingIndex());
            backdrop.Clicked += () =>
            {
                if (popup.CloseOnBackdropClick(Config)) CloseAsync(popup).Forget();
            };

            _backdrops[popup] = backdrop;
            return backdrop;
        }
    }
}
