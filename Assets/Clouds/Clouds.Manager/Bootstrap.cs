using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clouds.Manager
{
    // Single entry point that installs framework services at startup. Override InitializeAsync() in a
    // game-specific bootstrap to add further ordered async steps (backend auth, ...) after calling
    // base.InitializeAsync(), matching AppBootstrap -> GameBootFlow from the reference architecture.
    // Once InitializeAsync() completes, loads the next scene by Build Settings index.
    public class Bootstrap : MonoBehaviour
    {
        protected virtual void Start()
        {
            RunAsync().Forget();
        }

        private async UniTaskVoid RunAsync()
        {
            await InitializeAsync();
            LoadNextScene();
        }

        protected virtual async UniTask InitializeAsync()
        {
            DataService.PreloadAll();
            await ConfigLoader.LoadAllAsync();
        }

        protected virtual void LoadNextScene()
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogWarning("[Bootstrap] No next scene in Build Settings after the current one.");
                return;
            }
            SceneManager.LoadScene(nextIndex);
        }
    }
}
