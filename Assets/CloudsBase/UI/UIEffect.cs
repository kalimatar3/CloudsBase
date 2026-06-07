using Sirenix.OdinInspector;
using UnityEngine;
namespace Clouds
{
    [System.Serializable]
    public abstract class UIEffect : MonoBehaviour
    {
        public abstract void Excute();
        public abstract void Dispose();
#if UNITY_EDITOR
        [Button(ButtonSizes.Large)]
        [GUIColor(0, 1, 0)]
        public void EXCUTE()
        {
            this.Excute();
        }
        [Button(ButtonSizes.Large)]
        [GUIColor(0, 1, 0)]
        public void DISPOSE()
        {
            this.Dispose();
        }

#endif
    }    
}
