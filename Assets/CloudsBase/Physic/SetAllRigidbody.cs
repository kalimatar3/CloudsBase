using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetAllRigidbody : MonoBehaviour
{
    [Button(ButtonSizes.Large)] [GUIColor(0,1,0)]
    private void FINISH()
    {
#if UNITY_EDITOR
        this.Trigger();
#endif
    }

    private void Trigger()
    {
        Rigidbody[] rigidbodies = this.GetComponentsInChildren<Rigidbody>();
        foreach(var ele in rigidbodies) {
            ele.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
}
