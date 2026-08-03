using Sirenix.OdinInspector;
using UnityEngine;

namespace Clouds.UI
{
    [CreateAssetMenu(fileName = "UIAnimtation", menuName = "ScriptableObjects/UIAnimation")]
    public class UIAnimationData : ScriptableObject
    {
        [ListDrawerSettings(ListElementLabelName = "SummaryLabel", DraggableItems = true, ShowIndexLabels = false)]
        public UIEffectData[] Effects;

        [Title("Animation Loop")]
        [HorizontalGroup("Loop"), LabelWidth(38)] public bool Loop;
        [HorizontalGroup("Loop"), ShowIf("Loop"), LabelText("Mode"),  LabelWidth(42)] public LoopType LoopMode;
        [HorizontalGroup("Loop"), ShowIf("Loop"), LabelText("Count"), LabelWidth(44)] public int LoopCount;

        // Duration of one full cycle (ignores global loop)
        public float GetTotalDuration()
        {
            float max = 0f;
            foreach (var effect in Effects)
            {
                float d = effect.Delay + effect.Duration;
                if (d > max) max = d;
            }
            return max;
        }
    }
}
