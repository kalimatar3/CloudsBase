using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds.Singleton
{
    // No domain-reload reset hook needed here (unlike SignalBus/PopupService/PoolService): _instance is a
    // UnityEngine.Object reference, and Unity's overloaded == treats a destroyed object as null, so a stale
    // reference from a previous Play session already self-heals on the next Instance/Awake check.
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                    if(_instance == null)
                    {
                        var singletonObj = new GameObject();
                        singletonObj.name = typeof(T).ToString();
                        _instance = singletonObj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        public virtual void Awake()
        {
            if(_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = GetComponent<T>();

            if (_instance == null)
                return;
        }
    }
}
