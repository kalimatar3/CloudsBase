using Clouds.Manager;
using UnityEngine;

namespace Clouds.Timeline
{
    public abstract class TriggerableEnvironment : MyBehaviour {
        public abstract void Trigger();
        public abstract void TriggerOut();
    }
}
