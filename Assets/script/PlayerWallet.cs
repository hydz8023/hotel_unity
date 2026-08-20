using UnityEngine;

/// <summary>
/// 玩家钱包：集中管理银两。
/// 商店扣款、HUD 显示、日结结算均通过本组件。
/// </summary>
public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [SerializeField] private int initialSilver = 100;
    [SerializeField] private int silver;

    public int Silver => silver;

    /// <summary>银两变化时触发（参数为最新银两）。</summary>
    public System.Action<int> OnSilverChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        silver = initialSilver;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>尝试花费指定数量的银两。余额不足返回 false。</summary>
    public bool TrySpend(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (silver < amount)
        {
            return false;
        }

        silver -= amount;
        OnSilverChanged?.Invoke(silver);
        return true;
    }

    /// <summary>增加银两（收入结算等）。</summary>
    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        silver += amount;
        OnSilverChanged?.Invoke(silver);
    }
}
