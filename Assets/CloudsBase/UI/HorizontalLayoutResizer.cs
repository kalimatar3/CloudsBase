using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(HorizontalLayoutGroup))]
public class HorizontalLayoutResizer : MonoBehaviour
{
    [Tooltip("Giá trị cộng thêm ngoài kích thước thực (nếu muốn dư ra ngoài).")]
    public Vector2 Offset;

    private RectTransform rect;
    private HorizontalLayoutGroup layout;

    [Sirenix.OdinInspector.Button(ButtonSizes.Large)]
    public void ResizeToFitChildren()
    {
        rect = GetComponent<RectTransform>();
        layout = GetComponent<HorizontalLayoutGroup>();
        if (rect == null || layout == null)
            return;

        // Buộc layout tính toán lại vị trí & size con
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        var padding = layout.padding;
        float totalWidth = padding.left + padding.right;
        float totalHeight = padding.top + padding.bottom;

        float maxChildHeight = 0f;
        int activeCount = 0;

        foreach (RectTransform child in rect)
        {
            if (!child.gameObject.activeSelf)
                continue;

            // Lấy kích thước thật của con (RectTransform)
            float w = child.rect.width;
            float h = child.rect.height;

            totalWidth += w;
            maxChildHeight = Mathf.Max(maxChildHeight, h);
            activeCount++;
        }

        // Thêm spacing giữa các con
        if (activeCount > 1)
            totalWidth += layout.spacing * (activeCount - 1);

        // Thêm chiều cao lớn nhất của các con
        totalHeight += maxChildHeight;

        // Cộng offset tuỳ chỉnh
        totalWidth += Offset.x;
        totalHeight += Offset.y;

        // Gán lại kích thước cho cha
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }
}
