/// <summary>
/// 供摆放/预览等玩法脚本查询当前是否允许世界输入。
/// </summary>
public static class GameInputGate
{
    public static bool AllowsWorldInput =>
        UIManager.Instance == null || UIManager.Instance.InputMode == GameInputMode.World;
}
