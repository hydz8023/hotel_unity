using System;
using System.Collections.Generic;
using UnityEngine;
//客栈布局数据
[Serializable]
public class InnLayoutData
{
    public List<PlacedFurniture> furnitures = new List<PlacedFurniture>();
    public string innId;
    public int version = 1;
}

[Serializable]
public class PlacedFurniture
{
    public string furnitureId;
    public float x, z;          // 位置
    public float rotationY;     // 旋转角度
    public string instanceId;   // 实例唯一标识
    
    public PlacedFurniture(string id, Vector3 pos, float rot)
    {
        furnitureId = id;
        x = pos.x;
        z = pos.z;
        rotationY = rot;
        instanceId = Guid.NewGuid().ToString();
    }
    
    public Vector3 GetPosition() => new Vector3(x, 0, z);
}