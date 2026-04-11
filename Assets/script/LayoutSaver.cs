using UnityEngine;
using System.IO;

//保存/加载系统
public static class LayoutSaver
{
    private static string GetSavePath(string innId)
    {
        return Path.Combine(Application.persistentDataPath, $"inn_{innId}_layout.json");
    }
    
    public static void SaveLayout(InnLayoutData layout)
    {
        string json = JsonUtility.ToJson(layout, true);
        File.WriteAllText(GetSavePath(layout.innId), json);
        Debug.Log($"布局已保存到：{GetSavePath(layout.innId)}");
    }
    
    public static InnLayoutData LoadLayout(string innId)
    {
        string path = GetSavePath(innId);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<InnLayoutData>(json);
        }
        return null;
    }
    
    public static bool HasSaveFile(string innId)
    {
        return File.Exists(GetSavePath(innId));
    }
    
    public static void DeleteSave(string innId)
    {
        string path = GetSavePath(innId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}