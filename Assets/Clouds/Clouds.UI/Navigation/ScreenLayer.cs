using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Clouds.UI
{
    /// <summary>
    /// Chồng màn hình có lịch sử. Chỉ màn trên cùng active; màn bị đè Hide rồi tắt nhưng vẫn nằm trong
    /// lịch sử để Pop quay lại. Gọi qua ScreenService.
    /// </summary>
    public class ScreenLayer : UILayer
    {
        private readonly List<ScreenView> _stack = new();

        public IReadOnlyList<ScreenView> Screens => _stack;
        public ScreenView Current => _stack.Count > 0 ? _stack[^1] : null;

        public UniTask<T> PushAsync<T>(string key, Action<T> beforeShow = null) where T : ScreenView
            => Enqueue(async () =>
            {
                T screen = await AcquireAsync<T>(key);
                if (screen == null) return null;

                if (_stack.Contains(screen))
                {
                    UnityEngine.Debug.LogWarning($"[ScreenLayer] Màn '{key}' đã nằm trong lịch sử — không push lần nữa.");
                    return screen;
                }

                beforeShow?.Invoke(screen);
                ScreenView previous = Current;
                screen.transform.SetAsLastSibling();
                _stack.Add(screen);

                screen.PrepareShow();
                await screen.WillShowAsync();
                if (previous != null) await previous.WillHideAsync();

                await UniTask.WhenAll(
                    screen.PlayTransitionAsync(true, Config.ScreenPushEnter, Config.IgnoreTimeScale),
                    previous != null ? previous.PlayTransitionAsync(false, Config.ScreenPushExit, Config.IgnoreTimeScale)
                                     : UniTask.CompletedTask);

                if (previous != null)
                {
                    previous.DidHide();
                    previous.gameObject.SetActive(false);   // bị đè: tắt nhưng vẫn giữ trong lịch sử
                }
                screen.DidShow();
                return screen;
            });

        /// <summary>Bỏ màn trên cùng, Show lại màn ngay dưới. Pop được cả màn cuối cùng (lịch sử rỗng).</summary>
        public UniTask PopAsync()
            => Enqueue(async () =>
            {
                if (_stack.Count == 0) return;
                await PopInternalAsync();
            });

        public override UniTask CloseAsync(UIView view)
            => view is ScreenView screen ? CloseAsync(screen) : UniTask.CompletedTask;

        /// <summary>Màn trên cùng thì Pop; màn đang bị đè thì rút khỏi lịch sử luôn, không transition.</summary>
        public UniTask CloseAsync(ScreenView screen)
            => Enqueue(async () =>
            {
                if (screen == null || !_stack.Contains(screen)) return;
                if (screen == Current)
                {
                    await PopInternalAsync();
                    return;
                }
                _stack.Remove(screen);
                Release(screen);
            });

        /// <summary>Back/Esc: pop nếu còn màn để quay về. Màn gốc thì không nhận phím.</summary>
        internal bool HandleBack()
        {
            if (_stack.Count <= 1) return false;
            PopAsync().Forget();
            return true;
        }

        private async UniTask PopInternalAsync()
        {
            ScreenView top      = Current;
            ScreenView revealed = _stack.Count > 1 ? _stack[^2] : null;

            await top.WillHideAsync();
            if (revealed != null)
            {
                revealed.PrepareShow();
                await revealed.WillShowAsync();
            }

            await UniTask.WhenAll(
                top.PlayTransitionAsync(false, Config.ScreenPopExit, Config.IgnoreTimeScale),
                revealed != null ? revealed.PlayTransitionAsync(true, Config.ScreenPopEnter, Config.IgnoreTimeScale)
                                 : UniTask.CompletedTask);

            _stack.Remove(top);
            top.DidHide();
            Release(top);
            if (revealed != null) revealed.DidShow();
        }
    }
}
