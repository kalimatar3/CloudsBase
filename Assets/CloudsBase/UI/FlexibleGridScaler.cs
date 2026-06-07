using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class FlexibleGridScaler : MyBehaviour
{
    public Vector2 DefaultSpacing;
    public Vector4 Defaultpadding;
    public float minScale = 0.5f;
    public float maxScale = 1f;
    public int bestCount = 0;
    [Header("Components")]
    public FlextibleGridScalerMode flextibleGridScalerMode;
    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;
    [SerializeField] private ScrollRect scrollRect;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        AdjustGrid();
    }   
    [Button(ButtonSizes.Large)] [GUIColor(0,1,0)]
    private void SPACING()
    {
#if UNITY_EDITOR
       this.LoadComponents();
#endif
    }
    public void AdjustGrid()
    {
        
        int childCount = transform.childCount;
        if (childCount == 0) return;
        float containerWidth = rectTransform.rect.width;
        float containerHeight = rectTransform.rect.height;
        Vector2 baseCellSize = gridLayout.cellSize;
        Vector2 BestSpacing = DefaultSpacing;
        float bestScale = minScale;
        int bestCountPerRow = UIHelper.GetCountPerRow(containerWidth,baseCellSize.x,DefaultSpacing.x,1f);
        switch(flextibleGridScalerMode) {
            case FlextibleGridScalerMode.Scaler :
                for (float scale = maxScale; scale >= minScale; scale -= 0.1f)
                {
                    float scaledCellWidth = baseCellSize.x * scale;
                    Vector2 ScaledSpacing = DefaultSpacing - (1-scale) * baseCellSize;
                    int countrow = UIHelper.GetCountPerRow(containerWidth,baseCellSize.x,DefaultSpacing.x,scale);
                    float totalUnit = scaledCellWidth ;
                    float realwidth = totalUnit * bestCount + (bestCount - 1) * ScaledSpacing.x;
                    gridLayout.padding.top = (int)Defaultpadding.z + (int)((scale - 1) * gridLayout.cellSize.y/2f);
                    gridLayout.padding.bottom = (int)Defaultpadding.w + (int)((scale -1) * gridLayout.cellSize.y/2f);
                    gridLayout.padding.left = (int)Defaultpadding.x + (int)((scale - 1) * gridLayout.cellSize.x/2f);
                    gridLayout.padding.right = (int)Defaultpadding.y + (int)((scale -1) * gridLayout.cellSize.x/2f);
                    if (realwidth <= containerWidth + 2 * gridLayout.padding.left && countrow >= bestCount)
                    {
                        BestSpacing = ScaledSpacing;
                        bestScale = scale;
                        goto FoundBest;
                    }
                }
                FoundBest:
                gridLayout.spacing = BestSpacing;
                for (int i = 0; i < transform.childCount; i++)
                {
                    RectTransform child = transform.GetChild(i) as RectTransform;
                    if (child != null)
                    {
                        child.localScale = Vector3.one * bestScale;
                    }
                }
                int rowCount = Mathf.CeilToInt((float) transform.childCount / bestCountPerRow);
                if(scrollRect == null) return;
                float totalHeight = Mathf.Max(scrollRect.GetComponent<RectTransform>().rect.height,
                    gridLayout.padding.top +
                    gridLayout.padding.bottom +
                    (rowCount) * gridLayout.cellSize.y * bestScale +
                    (rowCount) * gridLayout.spacing.y);
                Vector2 size = rectTransform.sizeDelta;
                size.y = totalHeight;
                rectTransform.sizeDelta = size;
                UIHelper.ScrollToChildIndex(this.scrollRect,0,gridLayout);
            break;
            case(FlextibleGridScalerMode.Celling) :
                float CellingX = (containerWidth - this.Defaultpadding.x - this.Defaultpadding.y - this.DefaultSpacing.x * (bestCount -1))/bestCount;
                this.gridLayout.cellSize = new Vector2(CellingX,this.gridLayout.cellSize.y);
            break;
        }
    }
}
public enum FlexibleGridScalerType {
    Horizontal,
    Vertical,
}
public enum FlextibleGridScalerMode {
    Scaler,
    Celling,
}

