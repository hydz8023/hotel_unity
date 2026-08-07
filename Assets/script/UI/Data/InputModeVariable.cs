using System;
using UnityEngine;

/// <summary>
/// 全局输入模式的 ScriptableObject 载体。
/// 作为玩法层（摆放/预览/相机）与 UI 层（UIManager）之间的解耦媒介：
/// UIManager 写入当前模式，玩法层通过 GameInputGate 读取，互不持有对方引用。
/// </summary>
[CreateAssetMenu(menuName = "Variables/Input Mode")]
public class InputModeVariable : ScriptableObject
{
    [SerializeField] private GameInputMode _value = GameInputMode.World;

    public GameInputMode Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnValueChanged?.Invoke(value);
        }
    }

    public event Action<GameInputMode> OnValueChanged;

    /// <summary>
    /// 是否允许世界（场景）输入。仅 World 模式允许；UIOnly/Blocked 均拦截。
    /// </summary>
    public bool AllowsWorldInput => _value == GameInputMode.World;

    public void SetValue(GameInputMode mode) => Value = mode;
}
