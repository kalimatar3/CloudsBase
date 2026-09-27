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

        // Có effect nào lấy From từ giá trị hiện tại không. Tween được dựng sẵn một lần rồi phát lại
        // nhiều lần, nên "hiện tại" chỉ đúng nếu component dựng lại tween ngay trước mỗi lần phát —
        // cờ này để nó biết khi nào phải trả cái giá đó, thay vì rebuild vô ích cho preset thường.
        public bool HasFromCurrent
        {
            get
            {
                if (Effects == null) return false;
                foreach (var effect in Effects) if (effect.FromCurrent) return true;
                return false;
            }
        }

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
