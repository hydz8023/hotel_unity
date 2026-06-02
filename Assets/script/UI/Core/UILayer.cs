/// <summary>
/// UI 显示层级。第一阶段仅启用 HUD / Popup；其余层级预留给后续版本。
/// </summary>
public enum UILayer
{
    Background = 0,
    HUD = 50,
    Normal = 100,
    Popup = 200,
    Top = 300
}
