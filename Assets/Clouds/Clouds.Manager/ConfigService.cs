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

        internal static void Register(ScriptableObject config)
            => _registry[config.GetType()] = config;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic() => _registry.Clear();
#endif
    }
}
