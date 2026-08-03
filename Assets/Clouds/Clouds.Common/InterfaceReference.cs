using System;
using UnityEngine;

namespace Clouds.Common
{
    [Serializable]
    public class InterfaceReference<T> where T : class
    {
        [SerializeField] private MonoBehaviour target;

        public T Value => target as T;
        public GameObject GetGameObject() => target.gameObject;
    }
}
