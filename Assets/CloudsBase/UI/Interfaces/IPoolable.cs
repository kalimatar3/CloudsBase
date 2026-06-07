// IPoolable.cs
using UnityEngine;

public interface IPoolable
{
    void Init();
    void OnGetFromPool();
    void OnReturnToPool();
    GameObject GameObject { get; }
}