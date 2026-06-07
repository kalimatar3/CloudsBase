using System.Collections;
using Clouds.Ultilities;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtility
{
    public static void FillText(UIContinousEffectdata effect, string text)
    {
        if (effect.TextComponent != null) effect.TextComponent.text = text;
#if UNITY_EDITOR
        if(Application.isPlaying) effect.Text = text;
#endif        
    }

    public static void ChangeOpacity(UIContinousEffectdata effect, float opa) 
    {
        if (effect.Image != null)
        {
            Color color = effect.Image.color;
            color.a = opa;
            effect.Image.color = color;
#if UNITY_EDITOR
            if(Application.isPlaying) effect.Opacity = opa;
#endif        
        }
    }
}