using UnityEngine;
//放置预览
public class PlacementPreview : MonoBehaviour
{
    public GameObject previewObject;      // 预览用的半透明物体
    public Material validPreviewMat;      // 绿色半透明
    public Material invalidPreviewMat;    // 红色半透明
    
    private FurnitureData pendingFurniture;
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private FurniturePlacer furniturePlacer;
    private bool isPlacing = false;
    
    void Start()
    {
        // 优先使用 Inspector 注入的引用；未分配时回退到场景查找（兼容旧场景）
        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridSystem>();
            if (gridSystem == null)
                Debug.LogWarning("[PlacementPreview] 未分配 GridSystem 引用，且场景中未找到。");
        }
    }
    
    void Update()
    {
        if (!isPlacing) return;
        if (!GameInputGate.AllowsWorldInput) return;
        
        // 预览跟随鼠标
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPos = ray.GetPoint(enter);
            Vector3 snappedPos = gridSystem.SnapToGrid(targetPos);
            previewObject.transform.position = snappedPos;
            
            // 获取家具占用的网格尺寸（考虑旋转后交换长宽，与落位 footprint 一致）
            Vector2Int gridSize = GetEffectiveGridSize(pendingFurniture, previewObject.transform.eulerAngles.y);
            
            // 检查是否可放置
            bool isValid = gridSystem.IsPositionAvailable(snappedPos, gridSize);
            SetPreviewMaterial(isValid);
            
            // 点击放置
            if (Input.GetMouseButtonDown(0) && isValid)
            {
                PlaceFurniture();
            }
            
            // 按Esc取消。Popup 的 Esc 关闭由 UIManager 自行处理；
            // 暂停类 Popup 打开时 AllowsWorldInput=false，本分支不会进入，Esc 优先级天然成立。
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
            
            // 按R旋转
            if (Input.GetKeyDown(KeyCode.R))
            {
                previewObject.transform.Rotate(0, 90, 0);
            }
        }
    }
    
    public void StartPlacement(FurnitureData furniture)
    {
        pendingFurniture = furniture;
        previewObject = Instantiate(furniture.prefab);
        
        // 设置半透明材质
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // 保存原始材质，以便之后恢复（可选）
            Material previewMat = new Material(validPreviewMat);
            renderer.material = previewMat;
        }
        
        isPlacing = true;
    }
    
    void PlaceFurniture()
    {
        if (furniturePlacer == null)
        {
            furniturePlacer = FindObjectOfType<FurniturePlacer>();
        }
        if (furniturePlacer != null)
        {
            // 传入预览当前旋转，确保落位 footprint 与预览校验一致
            furniturePlacer.AddFurniture(pendingFurniture, previewObject.transform.position, previewObject.transform.eulerAngles.y);
        }
        else
        {
            Debug.LogError("找不到 FurniturePlacer 组件！");
        }
        
        Destroy(previewObject);
        isPlacing = false;
        pendingFurniture = null;
    }
    
    void CancelPlacement()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        isPlacing = false;
        pendingFurniture = null;
    }
    
    void SetPreviewMaterial(bool isValid)
    {
        Material targetMat = isValid ? validPreviewMat : invalidPreviewMat;
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = targetMat;
        }
    }

    /// <summary>
    /// 计算考虑旋转后的占用尺寸：旋转 90°/270° 时交换长宽，与 FurniturePlacer 保持一致。
    /// </summary>
    private Vector2Int GetEffectiveGridSize(FurnitureData data, float rotationY)
    {
        Vector2Int size = new Vector2Int((int)data.gridSize.x, (int)data.gridSize.y);
        int quarterTurns = Mathf.RoundToInt(Mathf.Repeat(rotationY, 360f) / 90f) % 2;
        return quarterTurns == 1 ? new Vector2Int(size.y, size.x) : size;
    }
}