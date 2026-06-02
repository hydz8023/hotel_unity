using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 常驻 HUD：银两与满意度。第一阶段仅展示，数据由外部 Refresh 注入。
/// </summary>
public class HUDPanel : UIPanelBase
{
    [SerializeField] private Text silverText;
    [SerializeField] private Text satisfactionText;

    public void SetReferences(Text silver, Text satisfaction)
    {
        silverText = silver;
        satisfactionText = satisfaction;
    }

    public override void OnOpen(object param)
    {
        RefreshDisplay(param as HUDData ?? new HUDData(0, 0f));
    }

    public override void OnRefresh(object param)
    {
        RefreshDisplay(param as HUDData ?? new HUDData(0, 0f));
    }

    private void RefreshDisplay(HUDData data)
    {
        if (silverText != null)
        {
            silverText.text = $"银两：{data.silver}";
        }

        if (satisfactionText != null)
        {
            satisfactionText.text = $"满意度：{data.satisfaction:F0}";
        }
    }
}
