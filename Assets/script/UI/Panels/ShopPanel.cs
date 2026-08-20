using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店面板：列出家具商品，点击购买扣银两后进入放置预览。
/// 依赖 PlayerWallet 扣款、PlacementPreview 进入放置。
/// 由 ShopPanel.prefab 实例化，脚本在 Awake 自动绑定 UI 引用。
/// </summary>
public class ShopPanel : UIPanelBase
{
    [Header("数据（可留空，运行时回退查找）")]
    [SerializeField] private FurnitureDatabase database;
    [SerializeField] private PlacementPreview placementPreview;

    private Text silverText;
    private Text messageText;
    private Button closeButton;
    private RectTransform content;

    private PlayerWallet wallet;
    private readonly List<GameObject> itemInstances = new List<GameObject>();

    private void Awake()
    {
        // Prefab 模式下自动从子对象（含深层）查找绑定
        if (silverText == null)
            silverText = FindRecursive(transform, "SilverText")?.GetComponent<Text>();
        if (messageText == null)
            messageText = FindRecursive(transform, "MessageText")?.GetComponent<Text>();
        if (closeButton == null)
            closeButton = FindRecursive(transform, "CloseButton")?.GetComponent<Button>();
        if (content == null)
            content = FindRecursive(transform, "Content")?.GetComponent<RectTransform>();

        BindCloseButton();
    }

    public override void OnShow(object param)
    {
        ResolveDependencies();
        RefreshWalletDisplay();
        RebuildItems();
    }

    public override void OnClose()
    {
        ClearItems();
    }

    private void BindCloseButton()
    {
        if (closeButton == null)
        {
            return;
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void ResolveDependencies()
    {
        if (wallet == null)
            wallet = PlayerWallet.Instance != null ? PlayerWallet.Instance : FindObjectOfType<PlayerWallet>();

        if (placementPreview == null)
            placementPreview = FindObjectOfType<PlacementPreview>();

        if (database == null && UIManager.Instance != null)
            database = UIManager.Instance.furnitureDatabase;
    }

    private void RefreshWalletDisplay()
    {
        if (silverText != null)
            silverText.text = $"银两：{(wallet != null ? wallet.Silver : 0)}";
    }

    private void RebuildItems()
    {
        ClearItems();

        if (database == null)
        {
            SetMessage("未配置家具数据库");
            return;
        }

        if (database.allFurniture == null || database.allFurniture.Count == 0)
        {
            SetMessage("暂无家具商品");
            return;
        }

        SetMessage(null);

        foreach (FurnitureData furniture in database.allFurniture)
        {
            if (furniture != null)
                CreateItem(furniture);
        }
    }

    private void CreateItem(FurnitureData furniture)
    {
        // 条目根：Image 背景 + Button
        GameObject item = new GameObject("Item_" + furniture.furnitureId,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        item.layer = LayerMask.NameToLayer("UI");
        item.transform.SetParent(content, false);

        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 80f);

        Image bg = item.GetComponent<Image>();
        bg.color = new Color(0.22f, 0.22f, 0.26f, 1f);

        Button button = item.GetComponent<Button>();
        FurnitureData captured = furniture;
        button.onClick.AddListener(() => OnBuyClicked(captured));

        // 图标（无图标则跳过）
        if (furniture.icon != null)
        {
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.layer = LayerMask.NameToLayer("UI");
            iconObj.transform.SetParent(item.transform, false);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(16f, 0f);
            iconRect.sizeDelta = new Vector2(56f, 56f);

            Image iconImage = iconObj.GetComponent<Image>();
            iconImage.sprite = furniture.icon;
            iconImage.raycastTarget = false;
        }

        // 名称
        Text nameText = CreateText(item.transform, "Name", furniture.furnitureName, TextAnchor.MiddleLeft, 22);
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = Vector2.zero;
        nameRect.anchorMax = new Vector2(0.62f, 1f);
        nameRect.offsetMin = new Vector2(88f, 0f);
        nameRect.offsetMax = Vector2.zero;

        // 价格
        Text priceText = CreateText(item.transform, "Price", $"{furniture.price} 两", TextAnchor.MiddleRight, 22);
        RectTransform priceRect = priceText.GetComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0.62f, 0f);
        priceRect.anchorMax = Vector2.one;
        priceRect.offsetMin = Vector2.zero;
        priceRect.offsetMax = new Vector2(-16f, 0f);

        itemInstances.Add(item);
    }

    private Text CreateText(Transform parent, string name, string content, TextAnchor alignment, int fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        obj.layer = LayerMask.NameToLayer("UI");
        obj.transform.SetParent(parent, false);

        Text text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.text = content;
        text.raycastTarget = false;
        return text;
    }

    private void OnBuyClicked(FurnitureData furniture)
    {
        ResolveDependencies();

        if (wallet == null)
        {
            SetMessage("未找到 PlayerWallet 组件");
            return;
        }

        if (furniture.prefab == null)
        {
            SetMessage($"商品缺少 Prefab：{furniture.furnitureName}");
            return;
        }

        if (placementPreview == null)
        {
            SetMessage("未找到 PlacementPreview 组件");
            return;
        }

        if (!wallet.TrySpend(furniture.price))
        {
            SetMessage("银两不足");
            return;
        }

        RefreshWalletDisplay();

        // 关闭商店（恢复 World 输入）后进入放置预览
        RequestClose();
        placementPreview.StartPlacement(furniture);
    }

    private void ClearItems()
    {
        foreach (GameObject item in itemInstances)
        {
            if (item != null)
                Destroy(item);
        }
        itemInstances.Clear();
    }

    private void SetMessage(string message)
    {
        if (messageText == null)
        {
            return;
        }

        bool hasMessage = !string.IsNullOrEmpty(message);
        messageText.text = hasMessage ? message : string.Empty;
        messageText.gameObject.SetActive(hasMessage);
    }

    private void OnCloseClicked()
    {
        RequestClose();
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
