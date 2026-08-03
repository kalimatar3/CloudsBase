using System;
using System.Threading;
using Clouds.Spawner;
using Cysharp.Threading.Tasks;

namespace Clouds.Strategy
{
    public class DeSpawnbytime_Strategy : DeSpawnStrategy
    {
        public IDespawnable Despawnable { get; set; }
        public float DeSpawntime { get; set; }

        private CancellationTokenSource _cts;

        public DeSpawnbytime_Strategy(IDespawnable despawnable, float despawntime)
        {
            Despawnable = despawnable;
            DeSpawntime = despawntime;
        }

        public void Excute()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            DeSpawnAfterDelay(_cts.Token).Forget();
        }

        private async UniTaskVoid DeSpawnAfterDelay(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(DeSpawntime), cancellationToken: token);
            Despawnable.Despawn();
        }
    }
}
