using UnityEngine;

/// <summary>
/// 所有 UI 面板 Prefab 根节点脚本基类。
/// </summary>
public abstract class UIPanelBase : MonoBehaviour
{
    public string PanelId { get; private set; }

    internal void BindPanelId(string panelId)
    {
        PanelId = panelId;
    }

    public virtual void OnOpen(object param) { }

    public virtual void OnClose() { }

    public virtual void OnRefresh(object param) { }

    protected void RequestClose()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Close(PanelId);
        }
    }
}
