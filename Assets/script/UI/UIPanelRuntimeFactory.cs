using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 当 Registry 未配置 Prefab 时，运行时生成最小可用 UI 结构。
/// </summary>
public static class UIPanelRuntimeFactory
{
    public static GameObject CreateHUDPanel(Transform parent)
    {
        GameObject root = CreatePanelRoot("HUDPanel", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(320f, 80f));

        Text silverText = CreateText(root.transform, "SilverText", "银两：0", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 10f), new Vector2(300f, 30f), TextAnchor.MiddleLeft);
        Text satisfactionText = CreateText(root.transform, "SatisfactionText", "满意度：0", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, -25f), new Vector2(300f, 30f), TextAnchor.MiddleLeft);

        HUDPanel panel = root.AddComponent<HUDPanel>();
        panel.SetReferences(silverText, satisfactionText);
        return root;
    }

    public static GameObject CreateDailyReportPanel(Transform parent)
    {
        GameObject root = CreatePanelRoot("DailyReportPanel", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 320f));
        Image background = root.GetComponent<Image>();
        background.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        Text titleText = CreateText(root.transform, "TitleText", "第 1 日结算", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(360f, 36f), TextAnchor.MiddleCenter);
        Text incomeText = CreateText(root.transform, "IncomeText", "收入：0", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(360f, 28f), TextAnchor.MiddleLeft);
        Text expenseText = CreateText(root.transform, "ExpenseText", "支出：0", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(360f, 28f), TextAnchor.MiddleLeft);
        Text profitText = CreateText(root.transform, "ProfitText", "盈亏：0", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(360f, 28f), TextAnchor.MiddleLeft);
        Text satisfactionText = CreateText(root.transform, "SatisfactionText", "满意度：0", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(360f, 28f), TextAnchor.MiddleLeft);

        Button closeButton = CreateButton(root.transform, "CloseButton", "关闭", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(120f, 36f));

        DailyReportPanel panel = root.AddComponent<DailyReportPanel>();
        panel.SetReferences(titleText, incomeText, expenseText, profitText, satisfactionText, closeButton);
        return root;
    }

    private static GameObject CreatePanelRoot(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(parent, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMin.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = root.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.35f);
        image.raycastTarget = true;
        return root;
    }

    private static Text CreateText(
        Transform parent,
        string name,
        string content,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = alignment;
        text.text = content;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.25f, 0.45f, 0.25f, 1f);

        Text buttonText = CreateText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        RectTransform labelRect = buttonText.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return buttonObject.GetComponent<Button>();
    }
}
