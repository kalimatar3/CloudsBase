using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Clouds.Manager
{
    // Single entry point that installs framework services at startup. The base framework's own
    // services (LoadSaveService, Repository<T>, DataService, PopupService, PoolService) are
    // stateless/lazy and need no explicit init step. Override InitializeAsync() in a game-specific
    // bootstrap to add ordered async steps (config load, backend auth, player data load, ...),
    // matching AppBootstrap -> GameBootFlow from the reference architecture.
    public class Bootstrap : MonoBehaviour
    {
        protected virtual void Start()
        {
            InitializeAsync().Forget();
        }

        protected virtual UniTask InitializeAsync()
        {
            return UniTask.CompletedTask;
        }
    }
}
