using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Clouds.Manager
{
    public static class ConfigLoader
    {
        private const string LABEL = "Game.Config";

        public static async UniTask LoadAllAsync()
        {
            var handle = Addressables.LoadAssetsAsync<ScriptableObject>(LABEL, null);
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var config in handle.Result)
                    ConfigService.Register(config);
            }
            else
            {
                Debug.LogError($"[ConfigLoader] Failed to load configs with label '{LABEL}'.");
            }
        }
    }
}
