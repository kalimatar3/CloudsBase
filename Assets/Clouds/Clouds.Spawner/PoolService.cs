using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Clouds.Spawner
{
    public static class PoolService
    {
        private class PoolHolder
        {
            public Transform Root;
            public GameObject Prefab;
            public readonly Queue<GameObject> Instances = new();
        }

        private static readonly Dictionary<string, PoolHolder> _pools = new();
        private static Transform _root;
        private static bool _isDestroyed;
        private static bool _quitHandlerRegistered;

        public static async UniTask<GameObject> SpawnAsync(string key, Vector3 position, Quaternion rotation)
        {
            PoolHolder holder = await GetOrCreateHolderAsync(key);
            GameObject instance = Dequeue(holder);
            if (instance == null)
            {
                instance = Object.Instantiate(holder.Prefab, position, rotation, holder.Root);
                instance.name = key;
            }
            else
            {
                instance.transform.SetPositionAndRotation(position, rotation);
            }
            instance.SetActive(true);
            return instance;
        }

        public static void Despawn(string key, GameObject instance)
        {
            if (instance == null) return;
            if (!_pools.TryGetValue(key, out PoolHolder holder))
            {
                Object.Destroy(instance);
                return;
            }
            instance.SetActive(false);
            instance.transform.SetParent(holder.Root);
            holder.Instances.Enqueue(instance);
        }

        public static void ReleasePool(string key)
        {
            if (!_pools.TryGetValue(key, out PoolHolder holder)) return;
            while (holder.Instances.Count > 0)
            {
                GameObject instance = holder.Instances.Dequeue();
                if (instance != null) Object.Destroy(instance);
            }
            if (holder.Root != null) Object.Destroy(holder.Root.gameObject);
            Addressables.Release(holder.Prefab);
            _pools.Remove(key);
        }

        public static void ReleaseAllPools()
        {
            foreach (string key in new List<string>(_pools.Keys)) ReleasePool(key);
        }

        private static async UniTask<PoolHolder> GetOrCreateHolderAsync(string key)
        {
            if (_pools.TryGetValue(key, out PoolHolder holder)) return holder;

            GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(key).ToUniTask();
            var rootGo = new GameObject(key);
            rootGo.transform.SetParent(GetOrCreateRoot());

            holder = new PoolHolder { Root = rootGo.transform, Prefab = prefab };
            _pools[key] = holder;
            return holder;
        }

        private static GameObject Dequeue(PoolHolder holder)
        {
            while (holder.Instances.Count > 0)
            {
                GameObject instance = holder.Instances.Dequeue();
                if (instance != null) return instance;
            }
            return null;
        }

        private static Transform GetOrCreateRoot()
        {
            if (_root != null || _isDestroyed) return _root;

            var go = new GameObject(nameof(PoolService));
            Object.DontDestroyOnLoad(go);
            _root = go.transform;

            if (!_quitHandlerRegistered)
            {
                _quitHandlerRegistered = true;
                Application.quitting += () => _isDestroyed = true;
            }
            return _root;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _pools.Clear();
            _root = null;
            _isDestroyed = false;
            _quitHandlerRegistered = false;
        }
#endif
    }
}
