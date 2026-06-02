using UnityEngine;
//放置预览
public class PlacementPreview : MonoBehaviour
{
    public GameObject previewObject;      // 预览用的半透明物体
    public Material validPreviewMat;      // 绿色半透明
    public Material invalidPreviewMat;    // 红色半透明
    
    private FurnitureData pendingFurniture;
    private GridSystem gridSystem;
    private bool isPlacing = false;
    
    void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>();
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
            
            // 获取家具占用的网格尺寸（Vector2 转 Vector2Int）
            Vector2Int gridSize = new Vector2Int((int)pendingFurniture.gridSize.x, (int)pendingFurniture.gridSize.y);
            
            // 检查是否可放置
            bool isValid = gridSystem.IsPositionAvailable(snappedPos, gridSize);
            SetPreviewMaterial(isValid);
            
            // 点击放置
            if (Input.GetMouseButtonDown(0) && isValid)
            {
                PlaceFurniture();
            }
            
            // 按Esc取消（若 Popup 打开则优先关面板）
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIManager.Instance != null && UIManager.Instance.TryCloseTopPopup())
                {
                    return;
                }

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
        FurniturePlacer placer = FindObjectOfType<FurniturePlacer>();
        if (placer != null)
        {
            placer.AddFurniture(pendingFurniture, previewObject.transform.position);
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
}