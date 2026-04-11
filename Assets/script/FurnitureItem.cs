using UnityEngine;

//家具组件
public class FurnitureItem : MonoBehaviour
{
    public FurnitureData furnitureData;
    
    public void Initialize(FurnitureData data)
    {
        furnitureData = data;
    }
    
    public void SavePosition(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }
}