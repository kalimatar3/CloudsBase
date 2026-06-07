using Clouds.Ultilities;
using UnityEngine;
[CreateAssetMenu(fileName = "UIAnimtation", menuName = "ScriptableObjects/UIAnimation")]
public class UIAnimationData : ScriptableObject
{
    public UIEffectData[] Effects;
    public float GetTotalDuration()
    {
        float maxDuration = 0f;
        foreach (var effect in Effects)
        {
            float effectDuration = effect.Delay + effect.Duration;
            if (effectDuration > maxDuration)
                maxDuration = effectDuration;
        }
        return maxDuration;
    }
}
