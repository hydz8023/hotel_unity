/// <summary>
/// 运行时面板实例句柄，由 UIManager 维护。
/// </summary>
public class UIPanelHandle
{
    public string PanelId;
    public UIPanelConfig Config;
    public UnityEngine.GameObject Instance;
    public UIPanelBase View;
    public bool IsVisible;
}
