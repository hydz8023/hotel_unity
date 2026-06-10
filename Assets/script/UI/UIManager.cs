using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

interface IUIPanelBase
{
    UIPanelBase New(string panelId);
    string TargetPanelName();
}

/// <summary>
/// UI 加载/关闭/缓存管理器（第一阶段：HUD + Popup 两层）。
/// </summary>
public class UIManager : MonoBehaviour
{
    public const string PanelHUD = "HUD";
    public const string PanelDailyReport = "DailyReport";

    public static UIManager Instance { get; private set; }

    [Header("配置")]
    public UIPanelRegistry panelRegistry;
    public bool openHudOnStart = true;

    [Header("层级节点（可留空，运行时自动创建）")]
    public Canvas uiCanvas;
    public RectTransform hudLayer;
    public RectTransform popupLayer;
    public Image popupBlocker;

    public GameInputMode InputMode { get; private set; } = GameInputMode.World;

    public System.Action<string> OnPanelOpened;
    public System.Action<string> OnPanelClosed;

    private readonly Dictionary<string, UIPanelHandle> activePanels = new Dictionary<string, UIPanelHandle>();
    private readonly Stack<string> popupStack = new Stack<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureCanvasHierarchy();
    }

    private void Start()
    {
        if (openHudOnStart)
        {
            // Open(PanelHUD, new HUDData(0, 0f));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TryCloseTopPopup();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool IsOpen(string panelId)
    {
        return activePanels.TryGetValue(panelId, out UIPanelHandle handle) && handle.IsVisible;
    }

    public static void ShowPanel(string panelId, object param = null)
    {
        Instance?.Show(panelId, param);
    }

    public bool HasOpenPopup()
    {
        return popupStack.Count > 0;
    }

    public T GetPanel<T>(string panelId) where T : UIPanelBase
    {
        if (activePanels.TryGetValue(panelId, out UIPanelHandle handle) && handle.View != null)
        {
            return handle.View as T;
        }

        return null;
    }

    private UIPanelBase Show(string panelId, object param = null)
    {
        UIPanelConfig config = ResolveConfig(panelId);
        if (config == null)
        {
            Debug.LogError($"未找到 UI 面板配置：{panelId}");
            return null;
        }

        if (activePanels.TryGetValue(panelId, out UIPanelHandle existing))
        {
            ShowHandle(existing, param);
            return existing.View;
        }

        UIPanelHandle handle = CreateHandle(config);
        if (handle.View == null)
        {
            Debug.LogError($"面板 Prefab 缺少 UIPanelBase 组件：{panelId}");
            Destroy(handle.Instance);
            return null;
        }

        activePanels.Add(panelId, handle);
        ShowHandle(handle, param);
        return handle.View;
    }

    public void Refresh(string panelId, object param = null)
    {
        if (!activePanels.TryGetValue(panelId, out UIPanelHandle handle) || handle.View == null)
        {
            return;
        }

        handle.View.OnRefresh(param);
    }

    public void Close(string panelId)
    {
        if (!activePanels.TryGetValue(panelId, out UIPanelHandle handle))
        {
            return;
        }

        handle.View?.OnClose();

        if (handle.Config != null && handle.Config.cacheOnClose)
        {
            handle.Instance.SetActive(false);
            handle.IsVisible = false;
        }
        else
        {
            Destroy(handle.Instance);
            activePanels.Remove(panelId);
        }

        RemoveFromPopupStack(panelId);
        RefreshInputMode();
        RefreshPopupBlocker();
        OnPanelClosed?.Invoke(panelId);
    }

    public bool TryCloseTopPopup()
    {
        if (popupStack.Count == 0)
        {
            return false;
        }

        Close(popupStack.Peek());
        return true;
    }

    public void CloseAll(UILayer? layer = null)
    {
        List<string> panelIds = new List<string>(activePanels.Keys);
        foreach (string panelId in panelIds)
        {
            UIPanelHandle handle = activePanels[panelId];
            if (layer.HasValue && handle.Config.layer != layer.Value)
            {
                continue;
            }

            Close(panelId);
        }
    }

    private UIPanelHandle CreateHandle(UIPanelConfig config)
    {
        Transform parent = GetLayerTransform(config.layer);
        GameObject instance = InstantiatePanel(config, parent);
        UIPanelBase view = instance.GetComponent<UIPanelBase>();
        if (view == null)
        {
            view = instance.GetComponentInChildren<UIPanelBase>();
        }

        view?.BindPanelId(config.panelId);

        return new UIPanelHandle
        {
            PanelId = config.panelId,
            Config = config,
            Instance = instance,
            View = view,
            IsVisible = false
        };
    }

    private GameObject InstantiatePanel(UIPanelConfig config, Transform parent)
    {
        if (config.prefab != null)
        {
            return Instantiate(config.prefab, parent);
        }

        if (config.panelId == PanelHUD)
        {
            return UIPanelRuntimeFactory.CreateHUDPanel(parent);
        }

        if (config.panelId == PanelDailyReport)
        {
            return UIPanelRuntimeFactory.CreateDailyReportPanel(parent);
        }

        GameObject fallback = new GameObject(config.panelId, typeof(RectTransform));
        fallback.transform.SetParent(parent, false);
        Debug.LogWarning($"未配置 Prefab，且 panelId={config.panelId} 无内置 UI 模板。");
        return fallback;
    }

    private void ShowHandle(UIPanelHandle handle, object param)
    {
        handle.Instance.SetActive(true);
        handle.IsVisible = true;
        handle.View?.OnShow(param);

        if (handle.Config.layer == UILayer.Popup)
        {
            PushPopup(handle.PanelId);
        }

        RefreshInputMode();
        RefreshPopupBlocker();
        OnPanelOpened?.Invoke(handle.PanelId);
    }

    private UIPanelConfig ResolveConfig(string panelId)
    {
        UIPanelConfig config = panelRegistry != null ? panelRegistry.GetPanel(panelId) : null;
        if (config != null)
        {
            return config;
        }

        return CreateBuiltinConfig(panelId);
    }

    private static UIPanelConfig CreateBuiltinConfig(string panelId)
    {
        UIPanelConfig config = ScriptableObject.CreateInstance<UIPanelConfig>();
        config.panelId = panelId;

        if (panelId == PanelHUD)
        {
            config.layer = UILayer.HUD;
            config.cacheOnClose = true;
            config.blockInputBelow = false;
            config.pauseGameplay = false;
            return config;
        }

        if (panelId == PanelDailyReport)
        {
            config.layer = UILayer.Popup;
            config.cacheOnClose = true;
            config.blockInputBelow = true;
            config.pauseGameplay = true;
            return config;
        }

        Object.Destroy(config);
        return null;
    }

    private Transform GetLayerTransform(UILayer layer)
    {
        EnsureCanvasHierarchy();

        switch (layer)
        {
            case UILayer.HUD:
                return hudLayer;
            case UILayer.Popup:
            case UILayer.Normal:
            case UILayer.Background:
            case UILayer.Top:
            default:
                return popupLayer;
        }
    }

    private void PushPopup(string panelId)
    {
        RemoveFromPopupStack(panelId);
        popupStack.Push(panelId);
    }

    private void RemoveFromPopupStack(string panelId)
    {
        if (popupStack.Count == 0)
        {
            return;
        }

        Stack<string> temp = new Stack<string>();
        while (popupStack.Count > 0)
        {
            string id = popupStack.Pop();
            if (id != panelId)
            {
                temp.Push(id);
            }
        }

        while (temp.Count > 0)
        {
            popupStack.Push(temp.Pop());
        }
    }

    private void RefreshInputMode()
    {
        bool pauseGameplay = false;
        foreach (UIPanelHandle handle in activePanels.Values)
        {
            if (!handle.IsVisible || handle.Config == null || !handle.Config.pauseGameplay)
            {
                continue;
            }

            pauseGameplay = true;
            break;
        }

        InputMode = pauseGameplay ? GameInputMode.UIOnly : GameInputMode.World;
    }

    private void RefreshPopupBlocker()
    {
        if (popupBlocker == null)
        {
            return;
        }

        bool showBlocker = false;
        foreach (string panelId in popupStack)
        {
            if (!activePanels.TryGetValue(panelId, out UIPanelHandle handle) || !handle.IsVisible)
            {
                continue;
            }

            if (handle.Config != null && handle.Config.blockInputBelow)
            {
                showBlocker = true;
                break;
            }
        }

        popupBlocker.gameObject.SetActive(showBlocker);
        if (showBlocker)
        {
            popupBlocker.transform.SetAsLastSibling();
        }
    }

    private void EnsureCanvasHierarchy()
    {
        if (uiCanvas == null)
        {
            GameObject canvasObject = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            uiCanvas = canvasObject.GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (hudLayer == null)
        {
            hudLayer = CreateLayerRect("HUDLayer", uiCanvas.transform);
        }

        if (popupLayer == null)
        {
            popupLayer = CreateLayerRect("PopupLayer", uiCanvas.transform);
        }

        if (popupBlocker == null)
        {
            popupBlocker = CreatePopupBlocker(popupLayer);
        }
    }

    private static RectTransform CreateLayerRect(string name, Transform parent)
    {
        GameObject layerObject = new GameObject(name, typeof(RectTransform));
        layerObject.transform.SetParent(parent, false);
        RectTransform rect = layerObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static Image CreatePopupBlocker(RectTransform parent)
    {
        GameObject blockerObject = new GameObject("PopupBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        blockerObject.transform.SetParent(parent, false);
        blockerObject.transform.SetAsFirstSibling();

        RectTransform rect = blockerObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = blockerObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.45f);
        image.raycastTarget = true;
        blockerObject.SetActive(false);
        return image;
    }
}
