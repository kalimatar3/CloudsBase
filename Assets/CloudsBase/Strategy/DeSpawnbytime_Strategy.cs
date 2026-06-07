using System.Collections;
using UnityEngine;
public class DeSpawnbytime_Strategy : DeSpawnStrategy
{
    public IDespawnable Despawnable { get;set;}
    [SerializeField] private float time;
    private Coroutine CrDeSpawnbytime;
    public float DeSpawntime {
            get {return time;}
            set{time = value;}
            }
    public DeSpawnbytime_Strategy(IDespawnable despawnable,float despawntime) {
        this.Despawnable = despawnable;
        this.time = despawntime;
    }
    protected IEnumerator DeSpawnbytimeCr() {
        yield return new WaitForSeconds(time);
        Despawnable.Despawn();
    }
    public void Excute()
    {
        Transform DeSpawnedObj = Despawnable.GetTransform();
        if(CrDeSpawnbytime != null) DeSpawnedObj.GetComponent<MyBehaviour>().StopCoroutine(CrDeSpawnbytime);
        CrDeSpawnbytime = DeSpawnedObj.GetComponent<MyBehaviour>().StartCoroutine(DeSpawnbytimeCr());
    }
    protected virtual void OnEnable() {
        this.Excute();
    }
}