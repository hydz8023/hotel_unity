using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
//网格吸附系统
public class GridSystem : MonoBehaviour
{
    [Header("网格设置")]
    public float cellSize = 1f;        // 格子大小
    public Vector2 gridOrigin = Vector2.zero;  // 网格原点
    public Vector2Int gridSize = new Vector2Int(20, 20);  // 网格范围
    
    private bool[,] occupiedGrid;  // 占用标记

    /// <summary>
    /// 将当前配置的中心点换算成网格左下角起点
    /// </summary>
    private Vector2 GetGridCornerOrigin()
    {
        return new Vector2(
            gridOrigin.x - (gridSize.x * cellSize) * 0.5f,
            gridOrigin.y - (gridSize.y * cellSize) * 0.5f
        );
    }
    
    void Start()
    {
        occupiedGrid = new bool[gridSize.x, gridSize.y];
    }
    
    /// <summary>
    /// 将世界坐标吸附到网格点
    /// </summary>
    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        Vector2 cornerOrigin = GetGridCornerOrigin();
        float x = Mathf.Round((worldPos.x - cornerOrigin.x) / cellSize) * cellSize + cornerOrigin.x;
        float z = Mathf.Round((worldPos.z - cornerOrigin.y) / cellSize) * cellSize + cornerOrigin.y;
        return new Vector3(x, 0, z);
    }
    
    /// <summary>
    /// 获取家具占用的所有格子坐标
    /// </summary>
    public Vector2Int[] GetOccupiedCells(Vector3 position, Vector2Int size)
    {
        Vector2 cornerOrigin = GetGridCornerOrigin();
        int startX = Mathf.RoundToInt((position.x - cornerOrigin.x) / cellSize);
        int startZ = Mathf.RoundToInt((position.z - cornerOrigin.y) / cellSize);
        
        // 处理家具尺寸为奇数的情况（中心对齐）
        startX -= (size.x - 1) / 2;
        startZ -= (size.y - 1) / 2;
        
        Vector2Int[] cells = new Vector2Int[size.x * size.y];
        int index = 0;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                cells[index++] = new Vector2Int(startX + x, startZ + z);
            }
        }
        
        return cells;
    }
    
    /// <summary>
    /// 检查位置是否可用
    /// </summary>
    public bool IsPositionAvailable(Vector3 position, Vector2Int size, string ignoreInstanceId = null)
    {
        Vector2Int[] cells = GetOccupiedCells(position, size);
        
        foreach (Vector2Int cell in cells)
        {
            if (cell.x < 0 || cell.x >= gridSize.x || cell.y < 0 || cell.y >= gridSize.y)
                return false;  // 超出边界
                
            if (occupiedGrid[cell.x, cell.y])
                return false;  // 被占用
        }
        
        return true;
    }
    
    /// <summary>
    /// 占用格子
    /// </summary>
    public void OccupyCells(Vector3 position, Vector2Int size, bool occupy)
    {
        Vector2Int[] cells = GetOccupiedCells(position, size);
        
        foreach (Vector2Int cell in cells)
        {
            if (cell.x >= 0 && cell.x < gridSize.x && cell.y >= 0 && cell.y < gridSize.y)
            {
                occupiedGrid[cell.x, cell.y] = occupy;
            }
        }
    }
    
    /// <summary>
    /// 绘制网格（用于调试）
    /// </summary>
    void OnDrawGizmos()
    {
        Vector2 cornerOrigin = GetGridCornerOrigin();
        Gizmos.color = Color.gray;
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = new Vector3(cornerOrigin.x + x * cellSize, 0, cornerOrigin.y);
            Vector3 end = new Vector3(cornerOrigin.x + x * cellSize, 0, cornerOrigin.y + gridSize.y * cellSize);
            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= gridSize.y; z++)
        {
            Vector3 start = new Vector3(cornerOrigin.x, 0, cornerOrigin.y + z * cellSize);
            Vector3 end = new Vector3(cornerOrigin.x + gridSize.x * cellSize, 0, cornerOrigin.y + z * cellSize);
            Gizmos.DrawLine(start, end);
        }

#if UNITY_EDITOR
        // 在每个格子中心显示坐标，左下角为(0,0)
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                Vector3 cellCenter = new Vector3(
                    cornerOrigin.x + (x + 0.5f) * cellSize,
                    0.02f,
                    cornerOrigin.y + (z + 0.5f) * cellSize
                );
                Handles.Label(cellCenter, $"({x},{z})", labelStyle);
            }
        }
#endif
    }
}