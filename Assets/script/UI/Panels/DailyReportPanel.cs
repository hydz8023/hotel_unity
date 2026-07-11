using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 每日结算面板 —— 显示当日收支与满意度。
/// 由 UIPanelRuntimeFactory 运行时动态生成。
/// </summary>
public class DailyReportPanel : UIPanelBase
{
    private Text titleText;
    private Text incomeText;
    private Text expenseText;
    private Text profitText;
    private Text satisfactionText;
    private Button closeButton;

    public void SetReferences(
        Text titleText,
        Text incomeText,
        Text expenseText,
        Text profitText,
        Text satisfactionText,
        Button closeButton)
    {
        this.titleText = titleText;
        this.incomeText = incomeText;
        this.expenseText = expenseText;
        this.profitText = profitText;
        this.satisfactionText = satisfactionText;
        this.closeButton = closeButton;

        if (this.closeButton != null)
            this.closeButton.onClick.AddListener(OnCloseClicked);
    }

    public override void OnRefresh(object param)
    {
        if (param is DailyReportData data)
        {
            if (titleText != null)
                titleText.text = $"第 {data.day} 日结算";
            if (incomeText != null)
                incomeText.text = $"收入：{data.income}";
            if (expenseText != null)
                expenseText.text = $"支出：{data.expense}";
            if (profitText != null)
                profitText.text = $"盈亏：{data.profit}";
            if (satisfactionText != null)
                satisfactionText.text = $"满意度：{data.satisfaction:F1}";
        }
    }

    private void OnCloseClicked()
    {
        RequestClose();
    }
}
