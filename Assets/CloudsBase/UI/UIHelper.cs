using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIHelper
{
    public static void SetAnchorPreset(RectTransform rt, RectAnchorPreset preset)
    {
        switch (preset)
        {
            case RectAnchorPreset.TopLeft:
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                break;

            case RectAnchorPreset.TopCenter:
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                break;

            case RectAnchorPreset.TopRight:
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                break;

            case RectAnchorPreset.MiddleCenter:
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;

            case RectAnchorPreset.BottomStretch:
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                break;

            case RectAnchorPreset.StretchAll:
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                break;
        }
    }
    public static float GetCanvasUnitsFromDp(float dp,Canvas mainCanvas)
    {
        // Lấy DPI của màn hình. Trả về một giá trị mặc định nếu không lấy được (ví dụ: trong Editor).
        float dpi = Screen.dpi;
        if (dpi == 0)
        {
            // DPI có thể là 0 trong một số trường hợp trong Editor. 
            // Dùng một giá trị trung bình để test. 320 (xhdpi) là một lựa chọn tốt.
            dpi = 320;
            Debug.LogWarning($"Screen.dpi is 0 (likely in Editor). Using default value: {dpi}");
        }

        // 1. Tính "density" của Android
        float density = dpi / 160f;

        // 2. Chuyển đổi DP sang Pixel
        float pixels = dp * density;

        // 3. Chuyển đổi Pixel sang đơn vị Canvas
        float canvasUnits = pixels / mainCanvas.scaleFactor;

        return canvasUnits;
    }
    public static void ScrollToChildIndex(
       ScrollRect scrollRect,
       float targetIndex,
       GridLayoutGroup gridLayout)
    {
        RectTransform content = scrollRect.content;
        int columnCount = Mathf.FloorToInt((content.rect.width + gridLayout.spacing.x) /
                                           (gridLayout.cellSize.x + gridLayout.spacing.x));
        if (columnCount <= 0) columnCount = 1;

        float row = targetIndex / columnCount;
        float totalHeight = content.rect.height;
        float cellHeight = gridLayout.cellSize.y + gridLayout.spacing.y;
        float targetY = row * cellHeight;

        float normalizedY = 1f - Mathf.Clamp01(targetY / (totalHeight - scrollRect.viewport.rect.height));
        scrollRect.verticalNormalizedPosition = normalizedY;
    }
    public static void ScrollToChildIndex(
    ScrollRect scrollRect,
    float targetIndex,
    GridLayoutGroup gridLayout,
    ScrollPivot pivot = ScrollPivot.Top)
    {
        RectTransform content = scrollRect.content;

        // Tính số cột
        int columnCount = Mathf.FloorToInt((content.rect.width + gridLayout.spacing.x) /
                                        (gridLayout.cellSize.x + gridLayout.spacing.x));
        if (columnCount <= 0) columnCount = 1;

        // Tính dòng của phần tử
        float row = targetIndex / columnCount;

        float totalHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        float cellHeight = gridLayout.cellSize.y + gridLayout.spacing.y;

        // Vị trí Y của phần tử tính từ top của content
        float targetY = row * cellHeight;

        // Điều chỉnh theo pivot
        switch (pivot)
        {
            case ScrollPivot.Middle:
                targetY -= viewportHeight * 0.5f - (gridLayout.cellSize.y * 0.5f);
                break;

            case ScrollPivot.Bottom:
                targetY -= viewportHeight - gridLayout.cellSize.y;
                break;
        }

        // Clamp để không kéo quá giới hạn
        targetY = Mathf.Clamp(targetY, 0, totalHeight - viewportHeight);

        // Convert sang normalized
        float normalizedY =
            1f - Mathf.Clamp01(targetY / (totalHeight - viewportHeight));

        scrollRect.verticalNormalizedPosition = normalizedY;
    }
    public static void ScrollToChildIndex(
        ScrollRect scrollRect,
        float targetIndex,
        VerticalLayoutGroup layoutGroup,
        ScrollPivot pivot = ScrollPivot.Top)
    {
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        int childCount = content.childCount;
        if (childCount == 0)
            return;

        int indexFloor = Mathf.FloorToInt(targetIndex);
        float t = targetIndex - indexFloor;

        float spacing = layoutGroup.spacing;
        float paddingTop = layoutGroup.padding.top;

        float targetY = paddingTop;
        int clampindex = indexFloor >=0 ? indexFloor : childCount + (indexFloor % childCount);
        float childSpacing = Mathf.Abs(content.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y - content.GetChild(0).GetComponent<RectTransform>().anchoredPosition.y);
        for (int i = 0; i < Mathf.Abs(indexFloor); i++)
        {
            targetY += childSpacing * Mathf.Sign(indexFloor);
        }

        // Blend chiều cao nếu index là float
        float blendedHeight = 0;

        if (t > 0)
        {
            RectTransform itemB = content.GetChild((clampindex + 1) % childCount) as RectTransform;
            blendedHeight = Mathf.Lerp(0, childSpacing, t) * Mathf.Sign(indexFloor);
        }
        float offset = targetY  + blendedHeight;
        content.anchoredPosition = -(content.rect.height/2f - content.GetChild(0).GetComponent<RectTransform>().rect.height/2f) * Vector3.up - offset *Vector3.up;
    }

    public static bool IsChildVisibleInRectMask(RectTransform rectMask, RectTransform grid, int childIndex)
    {
        if (grid == null || rectMask == null) return false;
        if (childIndex < 0 || childIndex >= grid.childCount) return false;

        RectTransform child = grid.GetChild(childIndex) as RectTransform;
        if (child == null) return false;

        // Lấy world corners của rectMask (viewport)
        Vector3[] maskCorners = new Vector3[4];
        rectMask.GetWorldCorners(maskCorners);

        // Lấy world corners của phần tử con
        Vector3[] childCorners = new Vector3[4];
        child.GetWorldCorners(childCorners);

        // Tính Rect của từng cái theo không gian thế giới
        Rect maskRect = new Rect(maskCorners[0].x, maskCorners[0].y,
                                 maskCorners[2].x - maskCorners[0].x,
                                 maskCorners[2].y - maskCorners[0].y);

        Rect childRect = new Rect(childCorners[0].x, childCorners[0].y,
                                  childCorners[2].x - childCorners[0].x,
                                  childCorners[2].y - childCorners[0].y);

        // Kiểm tra giao nhau
        return maskRect.Overlaps(childRect, true);
    }
    public static int GetCountPerRow(RectTransform content, GridLayoutGroup grid, float scale)
    {
        float scaledCellWidth = grid.cellSize.x * scale;
        float spacingX = grid.spacing.x;
        float totalWidth = content.rect.width;

        int countPerRow = Mathf.FloorToInt((totalWidth + spacingX) / (scaledCellWidth + spacingX));
        return Mathf.Max(1, countPerRow);
    }
    public static int GetCountPerRow(float containerWidth, float cellWidth, float spacing, float scale)
    {
        float scaledCellWidth = cellWidth * scale;
        float totalUnitWidth = scaledCellWidth + spacing;

        int count = Mathf.FloorToInt((containerWidth + spacing) / totalUnitWidth);
        return Mathf.Max(1, count);
    }
    public static string FormatTime(float timeInSeconds)
    {
        int totalCentiseconds = Mathf.FloorToInt(timeInSeconds * 100);

        int hours = totalCentiseconds / (3600 * 100);
        int minutes = (totalCentiseconds / (60 * 100)) % 60;
        int seconds = (totalCentiseconds / 100) % 60;
        int centiseconds = totalCentiseconds % 100;

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, centiseconds);
    }
    public static string FormatNumberString(string numberString)
    {
        if (string.IsNullOrEmpty(numberString))
            return "0";

        if (!decimal.TryParse(numberString, out decimal number))
            return numberString;

        return number.ToString("#,0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
    }
    public static void FitRectToText(Text textComponent)
    {
        if (textComponent == null) return;

        // Lấy rect hiện tại
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // Dùng TextGenerator để tính kích thước text thực tế
        TextGenerator generator = new TextGenerator();
        Vector2 extents = rectTransform.rect.size;

        var settings = textComponent.GetGenerationSettings(extents);
        float preferredWidth = generator.GetPreferredWidth(textComponent.text, settings) / textComponent.pixelsPerUnit;

        // Set lại width cho rect
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
    }
    public static void FitRectToText(TextMeshProUGUI tmpText)
    {
        if (tmpText == null) return;
        RectTransform rectTransform = tmpText.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        float preferredWidth = tmpText.preferredWidth;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
    }
public static void FitRectToText(TextMeshProUGUI tmpText, Vector2 MaxSize)
{
    if (tmpText == null) return;

    RectTransform rectTransform = tmpText.GetComponent<RectTransform>();
    if (rectTransform == null) return;

        // Gán tạm chiều rộng để TMP cập nhật lại layout
    float preferredWidth = Mathf.Min(tmpText.preferredWidth,MaxSize.x);
    
    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);

    // Bắt TMP cập nhật lại dữ liệu layout (rất quan trọng)
    Canvas.ForceUpdateCanvases();
    tmpText.ForceMeshUpdate();

    // Lấy lại chiều cao sau khi text xuống dòng thật
    float preferredHeight = Mathf.Min(MaxSize.y, tmpText.preferredHeight);

    // Cập nhật lại kích thước cuối cùng
    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
}

    public static Vector3 SameCamScreentoWorldPoint(Camera camera, Vector3 rectpos)
    {
        Vector3 ScreenPos = RectTransformUtility.WorldToScreenPoint(camera, rectpos);
        return camera.ScreenToWorldPoint(ScreenPos);
    }
    public static readonly Dictionary<string, string> CurrencySymbols = new Dictionary<string, string>
    {
        { "USD", "$" },
        { "VND", "₫" },
        { "JPY", "¥" },
        { "EUR", "€" },
        { "GBP", "£" }
    };
     public static Vector2 GetUIDistance(RectTransform rectA, RectTransform rectB, Canvas canvas = null, Camera worldCamera = null)
    {
        if (rectA == null || rectB == null)
        {
            Debug.LogWarning("GetUIDistance: rectA hoặc rectB bị null!");
            return Vector2.zero;
        }

        // Tìm Canvas nếu chưa có
        if (canvas == null)
            canvas = rectA.GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("GetUIDistance: không tìm thấy Canvas!");
            return Vector2.zero;
        }

        // Camera để tính toán UI (nếu Canvas là Screen Space - Camera)
        if (worldCamera == null)
            worldCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // Lấy vị trí của 2 RectTransform trong tọa độ màn hình
        Vector3 screenPosA = RectTransformUtility.WorldToScreenPoint(worldCamera, rectA.position);
        Vector3 screenPosB = RectTransformUtility.WorldToScreenPoint(worldCamera, rectB.position);

        // Chuyển về tọa độ local trong Canvas
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosA, worldCamera, out Vector2 localA);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosB, worldCamera, out Vector2 localB);

        // Khoảng cách Vector2 trong local space của Canvas
        return localA - localB;
    }

    /// <summary>
    /// Trả về khoảng cách tuyệt đối (theo pixel) giữa hai UI element.
    /// </summary>
    public static float GetUIDistanceMagnitude(RectTransform rectA, RectTransform rectB, Canvas canvas = null, Camera worldCamera = null)
    {
        return GetUIDistance(rectA, rectB, canvas, worldCamera).magnitude;
    }
}
public enum RectAnchorPreset
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleCenter,
    BottomStretch,
    StretchAll,
    Custom
}
public enum ScrollPivot
{
    Top,
    Middle,
    Bottom
}
