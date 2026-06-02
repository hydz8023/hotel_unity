using UnityEngine;

[CreateAssetMenu(fileName = "UIPanelConfig", menuName = "客栈/UI面板配置")]
public class UIPanelConfig : ScriptableObject
{
    public string panelId;
    public GameObject prefab;
    public UILayer layer = UILayer.Normal;
    public bool cacheOnClose = true;
    public bool blockInputBelow = false;
    public bool pauseGameplay = false;
}
