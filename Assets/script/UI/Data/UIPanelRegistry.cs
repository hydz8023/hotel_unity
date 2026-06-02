using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIPanelRegistry", menuName = "客栈/UI面板注册表")]
public class UIPanelRegistry : ScriptableObject
{
    public List<UIPanelConfig> panels = new List<UIPanelConfig>();

    private Dictionary<string, UIPanelConfig> panelMap;

    void OnEnable()
    {
        RebuildMap();
    }

    public void RebuildMap()
    {
        panelMap = new Dictionary<string, UIPanelConfig>();
        foreach (UIPanelConfig config in panels)
        {
            if (config == null || string.IsNullOrEmpty(config.panelId))
            {
                continue;
            }

            if (!panelMap.ContainsKey(config.panelId))
            {
                panelMap.Add(config.panelId, config);
            }
        }
    }

    public UIPanelConfig GetPanel(string panelId)
    {
        if (panelMap == null)
        {
            RebuildMap();
        }

        panelMap.TryGetValue(panelId, out UIPanelConfig config);
        return config;
    }
}
