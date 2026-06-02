using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 每日结算 Popup。Esc 或关闭按钮可关闭。
/// </summary>
public class DailyReportPanel : UIPanelBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text incomeText;
    [SerializeField] private Text expenseText;
    [SerializeField] private Text profitText;
    [SerializeField] private Text satisfactionText;
    [SerializeField] private Button closeButton;

    public void SetReferences(
        Text title,
        Text income,
        Text expense,
        Text profit,
        Text satisfaction,
        Button close)
    {
        titleText = title;
        incomeText = income;
        expenseText = expense;
        profitText = profit;
        satisfactionText = satisfaction;
        closeButton = close;
        WireCloseButton();
    }

    private void Awake()
    {
        WireCloseButton();
    }

    private void WireCloseButton()
    {
        if (closeButton == null)
        {
            return;
        }

        closeButton.onClick.RemoveListener(RequestClose);
        closeButton.onClick.AddListener(RequestClose);
    }

    public override void OnOpen(object param)
    {
        RefreshDisplay(param as DailyReportData ?? CreatePlaceholderReport());
    }

    public override void OnRefresh(object param)
    {
        RefreshDisplay(param as DailyReportData ?? CreatePlaceholderReport());
    }

    private void RefreshDisplay(DailyReportData data)
    {
        if (titleText != null)
        {
            titleText.text = $"第 {data.day} 日结算";
        }

        if (incomeText != null)
        {
            incomeText.text = $"收入：{data.income}";
        }

        if (expenseText != null)
        {
            expenseText.text = $"支出：{data.expense}";
        }

        if (profitText != null)
        {
            profitText.text = $"盈亏：{data.profit}";
        }

        if (satisfactionText != null)
        {
            satisfactionText.text = $"满意度：{data.satisfaction:F0}";
        }
    }

    private static DailyReportData CreatePlaceholderReport()
    {
        return new DailyReportData(1, 0, 0, 0f);
    }
}
