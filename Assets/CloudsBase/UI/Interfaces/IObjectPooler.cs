// IObjectPooler.cs
using UnityEngine;
using System.Collections.Generic; // Make sure this is included for Queue/Dictionary if needed by interface methods

public interface IObjectPooler
{
    void CreatePool(GameObject prefab, int initialSize);
    GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent);
    GameObject GetPooledObject(GameObject prefab);
    void ReturnPooledObject(GameObject obj);
}