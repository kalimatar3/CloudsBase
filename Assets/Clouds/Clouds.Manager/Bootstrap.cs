using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Clouds.Manager
{
    // Single entry point that installs framework services at startup. Override InitializeAsync() in a
    // game-specific bootstrap to add further ordered async steps (config load, backend auth, ...) after
    // calling base.InitializeAsync(), matching AppBootstrap -> GameBootFlow from the reference architecture.
    public class Bootstrap : MonoBehaviour
    {
        protected virtual void Start()
        {
            InitializeAsync().Forget();
        }

        protected virtual UniTask InitializeAsync()
        {
            DataService.PreloadAll();
            return UniTask.CompletedTask;
        }
    }
}
