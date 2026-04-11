using UnityEngine;
//网格吸附系统
public class GridSystem : MonoBehaviour
{
    [Header("网格设置")]
    public float cellSize = 1f;        // 格子大小
    public Vector2 gridOrigin = Vector2.zero;  // 网格原点
    public Vector2Int gridSize = new Vector2Int(20, 20);  // 网格范围
    
    private bool[,] occupiedGrid;  // 占用标记
    
    void Start()
    {
        occupiedGrid = new bool[gridSize.x, gridSize.y];
    }
    
    /// <summary>
    /// 将世界坐标吸附到网格点
    /// </summary>
    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        float x = Mathf.Round((worldPos.x - gridOrigin.x) / cellSize) * cellSize + gridOrigin.x;
        float z = Mathf.Round((worldPos.z - gridOrigin.y) / cellSize) * cellSize + gridOrigin.y;
        return new Vector3(x, 0, z);
    }
    
    /// <summary>
    /// 获取家具占用的所有格子坐标
    /// </summary>
    public Vector2Int[] GetOccupiedCells(Vector3 position, Vector2Int size)
    {
        int startX = Mathf.RoundToInt((position.x - gridOrigin.x) / cellSize);
        int startZ = Mathf.RoundToInt((position.z - gridOrigin.y) / cellSize);
        
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
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.gray;
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = new Vector3(gridOrigin.x + x * cellSize, 0, gridOrigin.y);
            Vector3 end = new Vector3(gridOrigin.x + x * cellSize, 0, gridOrigin.y + gridSize.y * cellSize);
            Gizmos.DrawLine(start, end);
        }
        
        for (int z = 0; z <= gridSize.y; z++)
        {
            Vector3 start = new Vector3(gridOrigin.x, 0, gridOrigin.y + z * cellSize);
            Vector3 end = new Vector3(gridOrigin.x + gridSize.x * cellSize, 0, gridOrigin.y + z * cellSize);
            Gizmos.DrawLine(start, end);
        }
    }
}