using System.Collections;
using System.Collections.Generic;
using Clouds.Ultilities;
using UnityEngine;

public abstract class Spawner<T> : Singleton<Spawner<T>> where T : Spawner<T>
{
    [SerializeField] public Transform Holder;
    public List<Transform> prefabs;
    [SerializeField] protected List<Transform> PoolObjs;
    protected virtual void LoadComponents()
    {
        this.LoadPrefabs();
    }
    protected virtual void LoadPrefabs()
    {
        if(this.prefabs.Count > 0 ) return;
        Transform PrefapObj = transform.Find("Prefabs");
        foreach(Transform pre in PrefapObj)
        {
            this.prefabs.Add(pre);
        }
        this.HidePrefabs();
        Debug.Log( transform.name + " : loadPrefab",gameObject);
    }
    protected void HidePrefabs()
    {
        foreach( Transform pre in prefabs)
        {
            pre.gameObject.SetActive(false);
        }
    }
    public virtual Transform Spawn(string PrefabName, Vector3 position, Quaternion rotation)
    {
        Transform Prefab = this.GetPrefabByName(PrefabName);
        if(Prefab == null)
        {
            Debug.LogWarning(" Prefab Can not Found :" + PrefabName );
        }
        Transform NewPrefab = GetObjectFromPool(Prefab,position,rotation);
        NewPrefab.gameObject.SetActive(true);
        NewPrefab.transform.SetParent(Holder);
        return NewPrefab;
    }
    public virtual Transform GetObjectFromPool(Transform Prefab, Vector3 position, Quaternion rotation)
    {
        foreach(Transform obj in PoolObjs)
        {
            if(obj != null)
            {
                if(Prefab.name == obj.name)
                {
                    PoolObjs.Remove(obj);
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    return obj;
                }
            }
        }
        Transform NewPrefab = Instantiate(Prefab,position,rotation);
        NewPrefab.name = Prefab.name;
        return NewPrefab;
    }
    public virtual Transform GetPrefabByName (string PrefabName)
    {
        foreach (Transform pre in prefabs)
        {
            if(pre.name == PrefabName)
            {
                return pre;
            }
        }
            return null;
    }
    public virtual void DeSpawnToPool(Transform obj)
    {
        PoolObjs.Add(obj);
        obj.gameObject.SetActive(false);
    }
}
