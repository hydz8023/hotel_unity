using UnityEngine;
using System.Collections.Generic;

//管理所有家具
[CreateAssetMenu(fileName = "FurnitureDatabase", menuName = "客栈/家具数据库")]
public class FurnitureDatabase : ScriptableObject
{
    public List<FurnitureData> allFurniture = new List<FurnitureData>();
    
    private Dictionary<string, FurnitureData> idMap;
    
    void OnEnable()
    {
        idMap = new Dictionary<string, FurnitureData>();
        foreach (FurnitureData data in allFurniture)
        {
            if (!idMap.ContainsKey(data.furnitureId))
            {
                idMap.Add(data.furnitureId, data);
            }
        }
    }
    
    public FurnitureData GetFurnitureById(string id)
    {
        idMap.TryGetValue(id, out FurnitureData data);
        return data;
    }
    
    public List<FurnitureData> GetFurnitureByCategory(FurnitureCategory category)
    {
        return allFurniture.FindAll(f => f.category == category);
    }
}