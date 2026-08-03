using UnityEngine;

namespace Clouds.Manager
{
    public class MyBehaviour : MonoBehaviour
    {
        protected virtual void Reset()
        {
            this.LoadComponents();
        }
        protected virtual void Awake()
        {
            this.LoadComponents();
        }
        protected virtual void LoadComponents()
        {
            //loadcomponent in childrent;
        }
    }
}
