using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 面板 —— 显示银两和满意度。
/// 支持 Prefab 自动绑定或 SetReferences 外部注入。
/// </summary>
public class HUDPanel : UIPanelBase
{
    private Text silverText;
    private Text satisfactionText;

    private void Awake()
    {
        // Prefab 模式下自动从子对象查找绑定
        if (silverText == null)
        {
            Transform silverTr = transform.Find("SilverText");
            if (silverTr != null)
                silverText = silverTr.GetComponent<Text>();
        }
        if (satisfactionText == null)
        {
            Transform satTr = transform.Find("SatisfactionText");
            if (satTr != null)
                satisfactionText = satTr.GetComponent<Text>();
        }
    }

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
