using UnityEngine;
using System.Collections.Generic;
//家具放置核心
public class FurniturePlacer : MonoBehaviour
{
    [Header("组件引用")]
    public GridSystem gridSystem;
    public LayerMask furnitureLayer;
    
    [Header("视觉反馈")]
    public Material validMaterial;   // 绿色（可放置）
    public Material invalidMaterial; // 红色（不可放置）
    public Material normalMaterial;  // 正常材质
    
    private GameObject currentDraggingFurniture;
    private FurnitureData currentFurnitureData;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isDragging = false;
    private List<GameObject> placedFurnitures = new List<GameObject>();
    
    // 事件：当家具放置/移动时触发
    public System.Action OnFurniturePlaced;
    
    void Update()
    {
        if (!GameInputGate.AllowsWorldInput)
        {
            return;
        }

        HandleMouseInput();
    }
    
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectFurniture();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            DragFurniture();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
        
        // 旋转家具（拖拽时按R键）
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            RotateFurniture();
        }
        
        // 取消放置（按Esc；若 Popup 打开则优先关面板）
        if (isDragging && Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.Instance != null && UIManager.Instance.TryCloseTopPopup())
            {
                return;
            }

            CancelDragging();
        }
    }
    
    /// <summary>
    /// 选中家具
    /// </summary>
    void TrySelectFurniture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100, furnitureLayer))
        {
            currentDraggingFurniture = hit.collider.gameObject;
            
            // 获取家具组件
            FurnitureItem furnitureItem = currentDraggingFurniture.GetComponent<FurnitureItem>();
            if (furnitureItem != null)
            {
                currentFurnitureData = furnitureItem.furnitureData;
                originalPosition = currentDraggingFurniture.transform.position;
                originalRotation = currentDraggingFurniture.transform.rotation;
                
                // 从网格中暂时移除占用标记
                gridSystem.OccupyCells(originalPosition, GetGridSizeFromData(currentFurnitureData), false);
                
                isDragging = true;
                SetMaterial(currentDraggingFurniture, validMaterial);
            }
        }
    }
    
    /// <summary>
    /// 拖拽家具
    /// </summary>
    void DragFurniture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPos = ray.GetPoint(enter);
            Vector3 snappedPos = gridSystem.SnapToGrid(targetPos);
            
            // 检查位置是否可用
            bool isValid = gridSystem.IsPositionAvailable(snappedPos, GetGridSizeFromData(currentFurnitureData));
            
            // 更新视觉反馈
            SetMaterial(currentDraggingFurniture, isValid ? validMaterial : invalidMaterial);
            
            // 移动家具
            currentDraggingFurniture.transform.position = snappedPos;
        }
    }
    
    /// <summary>
    /// 停止拖拽（放置）
    /// </summary>
    void StopDragging()
    {
        Vector3 finalPos = currentDraggingFurniture.transform.position;
        Vector2Int gridSize = GetGridSizeFromData(currentFurnitureData);
        
        // 检查最终位置是否合法
        if (gridSystem.IsPositionAvailable(finalPos, gridSize))
        {
            // 合法：占用新格子
            gridSystem.OccupyCells(finalPos, gridSize, true);
            
            // 更新数据
            FurnitureItem item = currentDraggingFurniture.GetComponent<FurnitureItem>();
            if (item != null)
            {
                item.SavePosition(finalPos, currentDraggingFurniture.transform.rotation);
            }
            
            SetMaterial(currentDraggingFurniture, normalMaterial);
            OnFurniturePlaced?.Invoke();
            
            // 保存布局
            SaveAllFurniture();
        }
        else
        {
            // 非法：回退到原位置
            currentDraggingFurniture.transform.position = originalPosition;
            currentDraggingFurniture.transform.rotation = originalRotation;
            gridSystem.OccupyCells(originalPosition, gridSize, true);
            SetMaterial(currentDraggingFurniture, normalMaterial);
            
            Debug.Log("放置位置无效！");
        }
        
        isDragging = false;
        currentDraggingFurniture = null;
        currentFurnitureData = null;
    }
    
    /// <summary>
    /// 旋转家具
    /// </summary>
    void RotateFurniture()
    {
        // 每次旋转90度
        currentDraggingFurniture.transform.Rotate(0, 90, 0);
        
        // 旋转后重新检测碰撞
        Vector3 currentPos = currentDraggingFurniture.transform.position;
        bool isValid = gridSystem.IsPositionAvailable(currentPos, GetGridSizeFromData(currentFurnitureData));
        SetMaterial(currentDraggingFurniture, isValid ? validMaterial : invalidMaterial);
    }
    
    /// <summary>
    /// 取消拖拽
    /// </summary>
    void CancelDragging()
    {
        currentDraggingFurniture.transform.position = originalPosition;
        currentDraggingFurniture.transform.rotation = originalRotation;
        gridSystem.OccupyCells(originalPosition, GetGridSizeFromData(currentFurnitureData), true);
        SetMaterial(currentDraggingFurniture, normalMaterial);
        
        isDragging = false;
        currentDraggingFurniture = null;
        currentFurnitureData = null;
    }
    
    /// <summary>
    /// 新增家具（从商店购买）
    /// </summary>
    public void AddFurniture(FurnitureData furniture, Vector3 position)
    {
        Vector3 snappedPos = gridSystem.SnapToGrid(position);
        Vector2Int gridSize = new Vector2Int((int)furniture.gridSize.x, (int)furniture.gridSize.y);
        
        if (gridSystem.IsPositionAvailable(snappedPos, gridSize))
        {
            GameObject newFurniture = Instantiate(furniture.prefab, snappedPos, Quaternion.identity);
            FurnitureItem item = newFurniture.AddComponent<FurnitureItem>();
            item.Initialize(furniture);
            
            gridSystem.OccupyCells(snappedPos, gridSize, true);
            placedFurnitures.Add(newFurniture);
            
            // 保存数据
            SaveAllFurniture();
        }
        else
        {
            Debug.Log("无法放置家具：位置被占用");
        }
    }
    
    /// <summary>
    /// 删除家具
    /// </summary>
    public void RemoveFurniture(GameObject furniture)
    {
        FurnitureItem item = furniture.GetComponent<FurnitureItem>();
        if (item != null)
        {
            Vector2Int gridSize = new Vector2Int((int)item.furnitureData.gridSize.x, (int)item.furnitureData.gridSize.y);
            gridSystem.OccupyCells(furniture.transform.position, gridSize, false);
            placedFurnitures.Remove(furniture);
            Destroy(furniture);
            SaveAllFurniture();
        }
    }
    
    /// <summary>
    /// 保存所有家具布局
    /// </summary>
    public void SaveAllFurniture()
    {
        InnLayoutData layout = new InnLayoutData();
        layout.innId = "inn_01";
        
        foreach (GameObject furniture in placedFurnitures)
        {
            FurnitureItem item = furniture.GetComponent<FurnitureItem>();
            if (item != null)
            {
                layout.furnitures.Add(new PlacedFurniture(
                    item.furnitureData.furnitureId,
                    furniture.transform.position,
                    furniture.transform.eulerAngles.y
                ));
            }
        }
        
        LayoutSaver.SaveLayout(layout);
    }
    
    /// <summary>
    /// 加载所有家具布局
    /// </summary>
    public void LoadAllFurniture(FurnitureDatabase database)
    {
        // 清除现有家具
        foreach (GameObject furniture in placedFurnitures)
        {
            Destroy(furniture);
        }
        placedFurnitures.Clear();
        
        // 加载布局
        InnLayoutData layout = LayoutSaver.LoadLayout("inn_01");
        if (layout == null) return;
        
        foreach (PlacedFurniture data in layout.furnitures)
        {
            FurnitureData furnitureData = database.GetFurnitureById(data.furnitureId);
            if (furnitureData != null)
            {
                GameObject newFurniture = Instantiate(furnitureData.prefab, data.GetPosition(), Quaternion.Euler(0, data.rotationY, 0));
                FurnitureItem item = newFurniture.AddComponent<FurnitureItem>();
                item.Initialize(furnitureData);
                
                Vector2Int gridSize = new Vector2Int((int)furnitureData.gridSize.x, (int)furnitureData.gridSize.y);
                gridSystem.OccupyCells(data.GetPosition(), gridSize, true);
                placedFurnitures.Add(newFurniture);
            }
        }
    }
    
    /// <summary>
    /// 从 FurnitureData 获取网格尺寸
    /// </summary>
    private Vector2Int GetGridSizeFromData(FurnitureData data)
    {
        return new Vector2Int((int)data.gridSize.x, (int)data.gridSize.y);
    }
    
    private void SetMaterial(GameObject obj, Material mat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && mat != null)
        {
            renderer.material = mat;
        }
    }
}