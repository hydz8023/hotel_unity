using UnityEngine;

//家具基础数据
[CreateAssetMenu(fileName = "New Furniture", menuName = "客栈/家具数据")]
public class FurnitureData : ScriptableObject
{
    public string furnitureId;      // 唯一ID
    public string furnitureName;    // 显示名称
    public GameObject prefab;       // 预制体
    public Vector2 gridSize = Vector2.one;  // 占用格子大小 (宽, 深)
    public int price;               // 购买价格
    public FurnitureCategory category;
    public Sprite icon;             // UI图标
}

public enum FurnitureCategory
{
    桌椅,
    柜台,
    屏风,
    装饰,
    照明
}