using UnityEditor;
using UnityEngine;

/// <summary>
/// 一键生成 v0.1 基础家具：primitive Prefab + FurnitureData 资产 + FurnitureDatabase。
/// - 菜单手动触发：客栈/生成基础家具
/// 采用两步法：先生成 prefab 并刷新，再重新加载 prefab 资产引用以生成 FurnitureData，
/// 避免 SaveAsPrefabAsset 后立即序列化导致的 prefab 引用为空（{fileID: 0}）。
/// </summary>
public static class FurnitureGenerator
{
    private const string PrefabFolder = "Assets/Prefabs/Furniture";
    private const string DataFolder = "Assets/Data";
    private const string DatabasePath = "Assets/Data/FurnitureDatabase.asset";

    private static readonly string[] FurnitureIds =
    {
        "table_01", "chair_01", "counter_01", "screen_01", "lantern_01", "vase_01"
    };

    private struct Spec
    {
        public string displayName;
        public Vector3 scale;
        public Vector2 gridSize;
        public int price;
        public FurnitureCategory category;
    }

    [MenuItem("客栈/生成基础家具")]
    public static void Generate()
    {
        Cleanup();
        EnsureFolder(PrefabFolder);
        EnsureFolder(DataFolder);

        // 第一步：生成所有 prefab，随后刷新确保资产可被稳定引用
        foreach (string id in FurnitureIds)
        {
            Spec spec = GetSpec(id);
            CreatePrefab(id, spec);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 第二步：重新加载 prefab 资产引用，生成 FurnitureData，再组装数据库
        FurnitureDatabase db = ScriptableObject.CreateInstance<FurnitureDatabase>();
        foreach (string id in FurnitureIds)
        {
            Spec spec = GetSpec(id);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{id}.prefab");
            db.allFurniture.Add(CreateFurnitureData(id, prefab, spec));
        }

        AssetDatabase.CreateAsset(db, DatabasePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[FurnitureGenerator] 已生成 {db.allFurniture.Count} 种基础家具与 FurnitureDatabase。");
    }

    private static void Cleanup()
    {
        foreach (string id in FurnitureIds)
        {
            DeleteAssetIfExists($"{DataFolder}/{id}.asset");
            DeleteAssetIfExists($"{PrefabFolder}/{id}.prefab");
        }
        DeleteAssetIfExists(DatabasePath);
        AssetDatabase.Refresh();
    }

    private static void DeleteAssetIfExists(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }
    }

    private static void CreatePrefab(string id, Spec spec)
    {
        // 根节点 pivot 贴地；Cube 子物体抬高，避免 Instantiate(y=0) 时家具半埋入地面。
        GameObject root = new GameObject(spec.displayName);
        GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.name = "Mesh";
        mesh.transform.SetParent(root.transform, false);
        mesh.transform.localPosition = new Vector3(0f, spec.scale.y * 0.5f, 0f);
        mesh.transform.localScale = spec.scale;

        PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{id}.prefab");
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static FurnitureData CreateFurnitureData(string id, GameObject prefab, Spec spec)
    {
        FurnitureData data = ScriptableObject.CreateInstance<FurnitureData>();
        data.furnitureId = id;
        data.furnitureName = spec.displayName;
        data.prefab = prefab;
        data.gridSize = spec.gridSize;
        data.price = spec.price;
        data.category = spec.category;

        AssetDatabase.CreateAsset(data, $"{DataFolder}/{id}.asset");
        return data;
    }

    private static Spec GetSpec(string id)
    {
        switch (id)
        {
            case "table_01":   return new Spec { displayName = "木桌", scale = new Vector3(1.0f, 0.5f, 1.0f), gridSize = new Vector2(1, 1), price = 80, category = FurnitureCategory.桌椅 };
            case "chair_01":   return new Spec { displayName = "木椅", scale = new Vector3(0.6f, 0.45f, 0.6f), gridSize = new Vector2(1, 1), price = 40, category = FurnitureCategory.桌椅 };
            case "counter_01": return new Spec { displayName = "柜台", scale = new Vector3(2.0f, 1.0f, 0.8f), gridSize = new Vector2(2, 1), price = 200, category = FurnitureCategory.柜台 };
            case "screen_01":  return new Spec { displayName = "屏风", scale = new Vector3(2.0f, 1.6f, 0.15f), gridSize = new Vector2(2, 1), price = 120, category = FurnitureCategory.屏风 };
            case "lantern_01": return new Spec { displayName = "灯笼", scale = new Vector3(0.5f, 0.9f, 0.5f), gridSize = new Vector2(1, 1), price = 60, category = FurnitureCategory.照明 };
            case "vase_01":    return new Spec { displayName = "花瓶", scale = new Vector3(0.4f, 0.7f, 0.4f), gridSize = new Vector2(1, 1), price = 50, category = FurnitureCategory.装饰 };
            default: throw new System.ArgumentException($"未知家具 id: {id}");
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = string.Empty;
        foreach (string segment in path.Split('/'))
        {
            string current = string.IsNullOrEmpty(parent) ? segment : $"{parent}/{segment}";
            if (!AssetDatabase.IsValidFolder(current))
            {
                AssetDatabase.CreateFolder(parent, segment);
            }
            parent = current;
        }
    }
}
