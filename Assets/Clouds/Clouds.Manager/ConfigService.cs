using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds.Manager
{
    public static class ConfigService
    {
        private static readonly Dictionary<Type, ScriptableObject> _registry = new();

        public static T GetConfig<T>() where T : ScriptableObject
        {
            if (_registry.TryGetValue(typeof(T), out var config))
                return (T)config;

            throw new InvalidOperationException(
                $"[ConfigService] {typeof(T).Name} not loaded. Call ConfigLoader.LoadAllAsync() first.");
        }

        // Hỏi xem một config đã nạp chưa mà KHÔNG ném exception. Dành cho code chạy mỗi frame và có
        // thể khởi động trước khi ConfigLoader xong (component trong scene gameplay khi bấm Play thẳng
        // từ scene đó, không qua Bootstrap): bọc GetConfig trong try/catch để dò trạng thái là biến một
        // câu hỏi bình thường thành luồng điều khiển bằng exception.
        public static bool IsLoaded<T>() where T : ScriptableObject => _registry.ContainsKey(typeof(T));

        internal static void Register(ScriptableObject config)
            => _registry[config.GetType()] = config;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic() => _registry.Clear();
#endif
    }
}
