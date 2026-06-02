using UnityEngine;

/// <summary>
/// 开发期辅助：按键演示 HUD 刷新与日结面板开关。
/// 挂到场景中任意对象即可；正式经营逻辑接入后可移除此组件。
/// </summary>
public class UIDemoController : MonoBehaviour
{
    [Header("演示数据")]
    public int demoSilver = 120;
    public float demoSatisfaction = 75f;
    public int demoDay = 1;
    public int demoIncome = 80;
    public int demoExpense = 30;

    private void Update()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            UIManager.Instance.Refresh(UIManager.PanelHUD, new HUDData(demoSilver, demoSatisfaction));
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            UIManager.Instance.Open(
                UIManager.PanelDailyReport,
                new DailyReportData(demoDay, demoIncome, demoExpense, demoSatisfaction));
        }
    }
}
