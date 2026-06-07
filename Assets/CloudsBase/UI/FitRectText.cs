using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class FitRectText : MonoBehaviour
{

    [Button(ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    public void FITRECT()
    {
        Text text = this.GetComponent<Text>();
        UIHelper.FitRectToText(text);
    }
}
