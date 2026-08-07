/// <summary>
/// 供摆放/预览/相机等玩法脚本查询当前是否允许世界输入。
/// 底层由 InputModeVariable（ScriptableObject）驱动，玩法层不直接依赖 UIManager 单例。
/// </summary>
public static class GameInputGate
{
    private static InputModeVariable _modeVariable;

    /// <summary>
    /// 由 UIManager 在启动时绑定输入模式变量。未绑定时默认允许世界输入，避免空引用。
    /// </summary>
    public static void Bind(InputModeVariable variable) => _modeVariable = variable;

    public static bool AllowsWorldInput =>
        _modeVariable == null || _modeVariable.AllowsWorldInput;
}
