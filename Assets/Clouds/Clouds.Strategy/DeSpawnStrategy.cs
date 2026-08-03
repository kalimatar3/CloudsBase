using Clouds.Spawner;

namespace Clouds.Strategy
{
    public interface DeSpawnStrategy {
        public IDespawnable Despawnable { get; set; }
        public void Excute();
    }
}
