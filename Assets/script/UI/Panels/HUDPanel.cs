using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 面板 —— 显示银两和满意度。
/// 由 HUDPanel.prefab 实例化，脚本在 Awake 自动绑定 UI 引用。
/// </summary>
public class HUDPanel : UIPanelBase
{
    private Text silverText;
    private Text satisfactionText;
    private Button saveButton;
    private Button loadButton;

    private void Awake()
    {
        // Prefab 模式下自动从子对象查找绑定
        if (silverText == null)
            silverText = FindRecursive(transform, "SilverText")?.GetComponent<Text>();
        if (satisfactionText == null)
            satisfactionText = FindRecursive(transform, "SatisfactionText")?.GetComponent<Text>();
        if (saveButton == null)
            saveButton = FindRecursive(transform, "SaveButton")?.GetComponent<Button>();
        if (loadButton == null)
            loadButton = FindRecursive(transform, "LoadButton")?.GetComponent<Button>();

        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClicked);
    }

    public override void OnRefresh(object param)
    {
        if (param is HUDData data)
        {
            if (silverText != null)
                silverText.text = $"银两：{data.silver}";
            if (satisfactionText != null)
                satisfactionText.text = $"满意度：{data.satisfaction:F1}";
        }
    }

    private void OnSaveClicked()
    {
        UIManager manager = UIManager.Instance;
        if (manager == null || manager.furniturePlacer == null)
        {
            Debug.LogWarning("无法保存布局：UIManager 未配置 furniturePlacer 引用。");
            return;
        }

        manager.furniturePlacer.SaveAllFurniture();
    }

    private void OnLoadClicked()
    {
        UIManager manager = UIManager.Instance;
        if (manager == null || manager.furniturePlacer == null || manager.furnitureDatabase == null)
        {
            Debug.LogWarning("无法读取布局：UIManager 未配置 furniturePlacer/furnitureDatabase 引用。");
            return;
        }

        manager.furniturePlacer.LoadAllFurniture(manager.furnitureDatabase);
    }

    private static Transform FindRecursive(Transform root, string name)
    {
        if (root.name == name)
            return root;

        foreach (Transform child in root)
        {
            Transform found = FindRecursive(child, name);
            if (found != null)
                return found;
        }

        return null;
    }
}
