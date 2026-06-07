using System;
using UnityEngine;

public class DeSpawnbyEvent : DeSpawnStrategy
{
    public IDespawnable Despawnable { get ; set ; }
    public DeSpawnbyEvent (IDespawnable despawnable,ref Action depspawnevent) {
        this.Despawnable = despawnable;
        depspawnevent += Excute;
    }
    public virtual void Excute()
    {
        Despawnable.Despawn();
    }
}