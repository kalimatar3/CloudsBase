using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clouds.Data;
using UnityEngine;

namespace Clouds.Manager
{
    // Extension point for game-specific static data access, e.g.:
    //   public static PlayerData Player => Repository<PlayerData>.Data;
    public static class DataService
    {
        // Reflection-discovers every concrete DynamicData subclass and eagerly loads its Repository<T>,
        // so adding a new data type never requires touching this class or Bootstrap.
        public static void PreloadAll()
        {
            foreach (Type dataType in GetConcreteDynamicDataTypes())
            {
                try
                {
                    Type repositoryType = typeof(Repository<>).MakeGenericType(dataType);
                    repositoryType.GetMethod("Load").Invoke(null, new object[] { null });
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataService] Failed to preload {dataType}: {e}");
                }
            }
        }

        private static IEnumerable<Type> GetConcreteDynamicDataTypes()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (Type type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericTypeDefinition && typeof(DynamicData).IsAssignableFrom(type))
                        yield return type;
                }
            }
        }
    }
}
