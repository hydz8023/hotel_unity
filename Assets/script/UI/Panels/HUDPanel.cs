using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 面板 —— 显示银两和满意度。
/// 由 UIPanelRuntimeFactory 运行时动态生成。
/// </summary>
public class HUDPanel : UIPanelBase
{
    private Text silverText;
    private Text satisfactionText;

    public void SetReferences(Text silverText, Text satisfactionText)
    {
        this.silverText = silverText;
        this.satisfactionText = satisfactionText;
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
}
