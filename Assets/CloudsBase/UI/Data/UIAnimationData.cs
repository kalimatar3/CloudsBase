using Clouds.Ultilities;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "UIAnimtation", menuName = "ScriptableObjects/UIAnimation")]
public class UIAnimationData : ScriptableObject
{
    [ListDrawerSettings(ListElementLabelName = "SummaryLabel", DraggableItems = true, ShowIndexLabels = false)]
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
