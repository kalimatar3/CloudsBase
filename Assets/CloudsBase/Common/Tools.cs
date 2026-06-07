using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Tools
{
    public static List<T> Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        List<T> newlist = new List<T>(list);
        int count = newlist.Count;
        while (count > 1)
        {
            count--;
            int k = rng.Next(count + 1);
            T value = newlist[k];
            newlist[k] = newlist[count];
            newlist[count] = value;
        }
        return newlist;
    }
    public static Vector3 GetElementVectorsMultiplier(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static Dictionary<string, int> SortDictionaryAscending(Dictionary<string, int> source)
    {
        return source
            .OrderBy(pair => pair.Value) // sắp xếp theo Value tăng dần
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }  
     public static Dictionary<string, int> SortDictionaryDescending(Dictionary<string, int> source)
    {
        return source
            .OrderByDescending(pair => pair.Value)   // giảm dần theo Value
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }  
    public static void ResizeBackGround(Image BACKGROUND,CanvasScaler canvasScaler,Vector2 DefaultRectSize)
    {
        float VerticleRatio = Screen.height / DefaultRectSize.y * canvasScaler.scaleFactor;
        float HorizontalRatio = Screen.width / DefaultRectSize.x * canvasScaler.scaleFactor;
        float ratio = 0;
        if (VerticleRatio < 1 && HorizontalRatio < 1)
        {
            ratio = Mathf.Max(VerticleRatio, HorizontalRatio);
        }
        else
        {
            ratio = Mathf.Min(VerticleRatio, HorizontalRatio);         
        }
        BACKGROUND.rectTransform.localScale = Vector3.one * ratio;
    }
     public enum Side { Left, Right, Top, Bottom }

    /// <summary>
    /// Tween scale + move RectTransform, giữ nguyên 1 cạnh cố định
    /// </summary>
    public static void RotateListToFront<T>(List<T> list, int index)
    {
        if (list == null || list.Count == 0) return;
        if (index < 0 || index >= list.Count) return;
        if (index == 0) return;

        // Cắt danh sách thành 2 phần
        List<T> head = list.GetRange(0, index);
        List<T> tail = list.GetRange(index, list.Count - index);

        // Xoá toàn bộ và ghép lại
        list.Clear();
        list.AddRange(tail);
        list.AddRange(head);
    }
    public static void TweenResizeKeepSide(RectTransform rect, Vector2 targetSize, float duration, Side side,Ease ease)
    {
        if (rect == null) return;

        Vector3 worldPos = GetSideWorldPosition(rect, side);
        DOTween.To(() => rect.sizeDelta, x => rect.sizeDelta = x, targetSize, duration)
            .SetEase(ease)
            .OnUpdate(() =>
            {
                Vector3 newWorldPos = GetSideWorldPosition(rect, side);
                Vector3 offset = worldPos - newWorldPos;
                rect.position += offset;
            });
    }
    public static void TweenResizeKeepSide(RectTransform rect, Vector2 targetSize, float duration, Side side, AnimationCurve Curve, Action OnComplete = null)
    {
        if (rect == null) return;

        Vector3 worldPos = GetSideWorldPosition(rect, side);
        float time = 0;
        DOTween.To(() => time, x => time = x, 1, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                float value = Curve.Evaluate(time);
                rect.sizeDelta = targetSize * value;
                Vector3 newWorldPos = GetSideWorldPosition(rect, side);
                Vector3 offset = worldPos - newWorldPos;
                rect.position += offset;
            }).OnComplete(() => OnComplete());
            
    }

    private static Vector3 GetSideWorldPosition(RectTransform rect, Side side)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        switch (side)
        {
            case Side.Left:   return (corners[0] + corners[1]) * 0.5f; // cạnh trái
            case Side.Right:  return (corners[2] + corners[3]) * 0.5f; // cạnh phải
            case Side.Top:    return (corners[1] + corners[2]) * 0.5f; // cạnh trên
            case Side.Bottom: return (corners[0] + corners[3]) * 0.5f; // cạnh dưới
        }
        return rect.position;
    }
    public static void SetChildrenOpacity(GameObject parent, float alpha)
    {
        // Lấy tất cả Image
        Image[] images = parent.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        // Lấy tất cả Text (UI cũ)
        Text[] texts = parent.GetComponentsInChildren<Text>(true);
        foreach (var txt in texts)
        {
            Color c = txt.color;
            c.a = alpha;
            txt.color = c;
        }

        // Lấy tất cả TextMeshProUGUI (nếu dùng TMP)
        TextMeshProUGUI[] tmps = parent.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmps)
        {
            Color c = tmp.color;
            c.a = alpha;
            tmp.color = c;
        }
    }
    #region  Animation
    public static void SeekCurrentAnimation(Animator animator, float time, int layer = 0)
    {
        if (!animator.isActiveAndEnabled) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(layer);
        if (!state.IsName(state.shortNameHash.ToString())) { } // không cần check tên

        float length = state.length;
        if (length <= 0f) return;

        float normalizedTime = Mathf.Clamp01(time / length);

        animator.Play(state.fullPathHash, layer, normalizedTime);
        animator.Update(0f); // ép Animator update ngay
    }
    #endregion
    
#if UNITY_EDITOR
    public static void RenameAsset(ScriptableObject obj, string newName)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        AssetDatabase.RenameAsset(path, newName);
        AssetDatabase.SaveAssets();
    }
#endif
}
