using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Clouds.SignalSystem
{
    /// <summary>
    /// Static pub/sub bus. Message có thể là bất kỳ struct nào — không cần marker interface.
    ///
    /// Global publish:
    ///   SignalBus.Publish(new OpenShopMsg { Source = "hud" });
    ///   await SignalBus.PublishAsync(new OpenShopMsg { Source = "hud" });
    ///
    /// Scoped publish (chỉ receivers đăng ký dưới TScope mới nhận):
    ///   SignalBus.Scope&lt;ShopPopup&gt;().Publish(new OpenShopMsg());
    ///   await SignalBus.Scope&lt;ShopPopup&gt;().PublishAsync(new OpenShopMsg());
    ///
    /// Subscribe / Unsubscribe:
    ///   SignalBus.Subscribe&lt;OpenShopMsg&gt;(OnReceive);
    ///   SignalBus.Scope&lt;ShopPopup&gt;().Subscribe&lt;OpenShopMsg&gt;(OnReceive);
    ///   SignalBus.Unsubscribe&lt;OpenShopMsg&gt;(OnReceive);
    /// </summary>
    public static class SignalBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _globalSync  = new();
        private static readonly Dictionary<Type, List<Delegate>> _globalAsync = new();
        private static readonly Dictionary<(Type, Type), List<Delegate>> _scopedSync  = new();
        private static readonly Dictionary<(Type, Type), List<Delegate>> _scopedAsync = new();

        // ── Scoped entry point ────────────────────────────────────────────────

        public static ScopedBus<TScope> Scope<TScope>() => default;

        // ── Global Subscribe ──────────────────────────────────────────────────

        public static void Subscribe<TMessage>(Action<TMessage> handler)
            where TMessage : struct
        {
            var list = GetOrCreate(_globalSync, typeof(TMessage));
            if (!list.Contains(handler)) list.Add(handler);
        }

        public static void Subscribe<TMessage>(Func<TMessage, UniTask> handler)
            where TMessage : struct
        {
            var list = GetOrCreate(_globalAsync, typeof(TMessage));
            if (!list.Contains(handler)) list.Add(handler);
        }

        // ── Global Unsubscribe ────────────────────────────────────────────────

        public static void Unsubscribe<TMessage>(Action<TMessage> handler)
            where TMessage : struct
        {
            if (_globalSync.TryGetValue(typeof(TMessage), out var list)) list.Remove(handler);
        }

        public static void Unsubscribe<TMessage>(Func<TMessage, UniTask> handler)
            where TMessage : struct
        {
            if (_globalAsync.TryGetValue(typeof(TMessage), out var list)) list.Remove(handler);
        }

        // ── Global Publish ────────────────────────────────────────────────────

        public static void Publish<TMessage>(TMessage message)
            where TMessage : struct
        {
            DispatchSync(_globalSync, typeof(TMessage), message);
            DispatchForget(_globalAsync, typeof(TMessage), message);
        }

        public static UniTask PublishAsync<TMessage>(TMessage message)
            where TMessage : struct
        {
            DispatchSync(_globalSync, typeof(TMessage), message);
            return DispatchAwait(_globalAsync, typeof(TMessage), message);
        }

        // ── ClearAll (scene reload / test teardown) ───────────────────────────

        public static void ClearAll()
        {
            _globalSync.Clear();
            _globalAsync.Clear();
            _scopedSync.Clear();
            _scopedAsync.Clear();
        }

        // ── Nested ScopedBus ──────────────────────────────────────────────────

        public readonly struct ScopedBus<TScope>
        {
            public void Subscribe<TMessage>(Action<TMessage> handler)
                where TMessage : struct
            {
                var list = GetOrCreate(_scopedSync, (typeof(TScope), typeof(TMessage)));
                if (!list.Contains(handler)) list.Add(handler);
            }

            public void Subscribe<TMessage>(Func<TMessage, UniTask> handler)
                where TMessage : struct
            {
                var list = GetOrCreate(_scopedAsync, (typeof(TScope), typeof(TMessage)));
                if (!list.Contains(handler)) list.Add(handler);
            }

            public void Unsubscribe<TMessage>(Action<TMessage> handler)
                where TMessage : struct
            {
                if (_scopedSync.TryGetValue((typeof(TScope), typeof(TMessage)), out var list))
                    list.Remove(handler);
            }

            public void Unsubscribe<TMessage>(Func<TMessage, UniTask> handler)
                where TMessage : struct
            {
                if (_scopedAsync.TryGetValue((typeof(TScope), typeof(TMessage)), out var list))
                    list.Remove(handler);
            }

            public void Publish<TMessage>(TMessage message)
                where TMessage : struct
            {
                DispatchSync(_scopedSync, (typeof(TScope), typeof(TMessage)), message);
                DispatchForget(_scopedAsync, (typeof(TScope), typeof(TMessage)), message);
            }

            public UniTask PublishAsync<TMessage>(TMessage message)
                where TMessage : struct
            {
                DispatchSync(_scopedSync, (typeof(TScope), typeof(TMessage)), message);
                return DispatchAwait(_scopedAsync, (typeof(TScope), typeof(TMessage)), message);
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static List<Delegate> GetOrCreate(Dictionary<Type, List<Delegate>> dict, Type key)
        {
            if (!dict.TryGetValue(key, out var list)) dict[key] = list = new List<Delegate>();
            return list;
        }

        private static List<Delegate> GetOrCreate(Dictionary<(Type, Type), List<Delegate>> dict, (Type, Type) key)
        {
            if (!dict.TryGetValue(key, out var list)) dict[key] = list = new List<Delegate>();
            return list;
        }

        private static void DispatchSync<TMessage>(Dictionary<Type, List<Delegate>> dict, Type key, TMessage msg)
        {
            if (!dict.TryGetValue(key, out var list) || list.Count == 0) return;
            var snap = list.ToArray();
            foreach (var d in snap) if (d is Action<TMessage> a) a(msg);
        }

        private static void DispatchSync<TMessage>(Dictionary<(Type, Type), List<Delegate>> dict, (Type, Type) key, TMessage msg)
        {
            if (!dict.TryGetValue(key, out var list) || list.Count == 0) return;
            var snap = list.ToArray();
            foreach (var d in snap) if (d is Action<TMessage> a) a(msg);
        }

        private static void DispatchForget<TMessage>(Dictionary<Type, List<Delegate>> dict, Type key, TMessage msg)
        {
            if (!dict.TryGetValue(key, out var list) || list.Count == 0) return;
            var snap = list.ToArray();
            foreach (var d in snap) if (d is Func<TMessage, UniTask> f) f(msg).Forget();
        }

        private static void DispatchForget<TMessage>(Dictionary<(Type, Type), List<Delegate>> dict, (Type, Type) key, TMessage msg)
        {
            if (!dict.TryGetValue(key, out var list) || list.Count == 0) return;
            var snap = list.ToArray();
            foreach (var d in snap) if (d is Func<TMessage, UniTask> f) f(msg).Forget();
        }

        private static async UniTask DispatchAwait<TMessage>(Dictionary<Type, List<Delegate>> dict, Type key, TMessage msg)
        {
            if (!dict.TryGetValue(key, out var list) || list.Count == 0) return;
            var snap = list.ToArray();
            var tasks = new List<UniTask>(snap.Length);
            foreach (var d in snap) if (d is Func<TMessage, UniTask> f) tasks.Add(f(msg));
            if (tasks.Count > 0) await UniTask.WhenAll(tasks);
        }

        private static async UniTask DispatchAwait<TMessage>(Dictionary<(Type, Type), List<Delegate>> dict, (Type, Type) key, TMessage msg)
        {
            if (!dict.TryGetValue(key, out var list) || list.Count == 0) return;
            var snap = list.ToArray();
            var tasks = new List<UniTask>(snap.Length);
            foreach (var d in snap) if (d is Func<TMessage, UniTask> f) tasks.Add(f(msg));
            if (tasks.Count > 0) await UniTask.WhenAll(tasks);
        }
    }
}
