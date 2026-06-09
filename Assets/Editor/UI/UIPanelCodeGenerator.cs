using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Hotel.Editor.UI
{
    /// <summary>
    /// UI 面板代码生成器。
    /// 选中 Assets/Prefabs/UI/ 下的预制体，自动生成 Base 类 + Concrete 类，并自动绑定引用。
    /// </summary>
    public static class UIPanelCodeGenerator
    {
        private const string TargetPrefabRoot = "Assets/Prefabs/UI";
        private const string OutputRoot = "Assets/script/UI/Panels";

        private static readonly Dictionary<Type, string> ControlPrefixMap = new Dictionary<Type, string>
        {
            { typeof(Button),      "btn_"   },
            { typeof(Image),       "img_"   },
            { typeof(RawImage),    "raw_"   },
            { typeof(Text),        "txt_"   },
            { typeof(Slider),      "sld_"   },
            { typeof(Toggle),      "tog_"   },
            { typeof(Dropdown),    "dd_"    },
            { typeof(InputField),  "input_" },
            { typeof(ScrollRect),  "scroll_"},
        };

        private static readonly Dictionary<Type, string> TMPControlPrefixMap = new Dictionary<Type, string>
        {
            { TryGetTMPType("TMP_Text"),       "txt_"   },
            { TryGetTMPType("TMP_InputField"), "input_" },
            { TryGetTMPType("TMP_Dropdown"),   "dd_"    },
        };

        private static Type TryGetTMPType(string typeName)
        {
            return Type.GetType($"TMPro.{typeName}, Unity.TextMeshPro");
        }

        [MenuItem("Assets/生成UIPanel代码", true)]
        private static bool ValidateGenerateMenu()
        {
            Object selected = Selection.activeObject;
            return IsValidTargetPrefab(selected);
        }

        [MenuItem("Assets/生成UIPanel代码", false, 1000)]
        private static void GenerateFromContextMenu()
        {
            Object selected = Selection.activeObject;
            if (!IsValidTargetPrefab(selected))
            {
                EditorUtility.DisplayDialog("生成失败", "只能对 Assets/Prefabs/UI/ 目录下的预制体使用此功能。", "确定");
                return;
            }

            GeneratePanelCode(selected as GameObject);
        }

        [MenuItem("Tools/UI/生成选中预制体UIPanel代码", false, 1000)]
        private static void GenerateFromMainMenu()
        {
            Object selected = Selection.activeObject;
            if (!IsValidTargetPrefab(selected))
            {
                EditorUtility.DisplayDialog("生成失败", "请先在 Project 视图中选中 Assets/Prefabs/UI/ 目录下的预制体。", "确定");
                return;
            }

            GeneratePanelCode(selected as GameObject);
        }

        private static bool IsValidTargetPrefab(Object obj)
        {
            if (obj == null) return false;
            string path = AssetDatabase.GetAssetPath(obj);
            return path.StartsWith(TargetPrefabRoot + "/", StringComparison.OrdinalIgnoreCase)
                   && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
        }

        private static void GeneratePanelCode(GameObject prefabAsset)
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);

            // 1. 收集控件
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载预制体内容。", "确定");
                return;
            }

            List<CollectedControl> controls = CollectControls(prefabRoot);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            if (controls.Count == 0)
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "提示",
                    $"预制体 \"{prefabName}\" 中未找到符合命名规范的控件（btn_、img_、txt_ 等）。\n是否仍生成空 Base 类？",
                    "生成", "取消");

                if (!proceed) return;
            }

            // 2. 准备输出目录
            string outputDir = Path.Combine(OutputRoot, prefabName);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 3. 生成 Base 类（总是覆盖）
            string baseClassName = $"{prefabName}Base";
            string baseFilePath = Path.Combine(outputDir, $"{baseClassName}.cs");
            string baseCode = GenerateBaseClassCode(prefabName, baseClassName, controls);
            File.WriteAllText(baseFilePath, baseCode, Encoding.UTF8);
            AssetDatabase.ImportAsset(baseFilePath);

            // 4. 生成 Concrete 类（仅首次）
            string concreteClassName = prefabName;
            string concreteFilePath = Path.Combine(outputDir, $"{concreteClassName}.cs");
            if (!File.Exists(concreteFilePath))
            {
                string concreteCode = GenerateConcreteClassCode(prefabName, concreteClassName, baseClassName);
                File.WriteAllText(concreteFilePath, concreteCode, Encoding.UTF8);
                AssetDatabase.ImportAsset(concreteFilePath);
            }

            // 5. 自动绑定引用到 Prefab（支持刷新）
            bool bindSuccess = AutoBindComponents(prefabPath, baseClassName, controls);

            AssetDatabase.Refresh();

            string message = bindSuccess
                ? $"生成完成！\n- {baseClassName}.cs 已更新\n- {concreteClassName}.cs {(File.Exists(concreteFilePath) ? "已存在（未覆盖）" : "已创建")}\n- 控件引用已自动绑定到预制体"
                : $"代码已生成，但自动绑定失败。\n请手动将 {baseClassName} 挂到预制体根节点并拖拽引用。";

            EditorUtility.DisplayDialog("UI 面板代码生成器", message, "确定");
        }

        private static List<CollectedControl> CollectControls(GameObject root)
        {
            var result = new List<CollectedControl>();
            var allUI = root.GetComponentsInChildren<UIBehaviour>(true);

            // 合并 TMP 类型（如果存在）
            var prefixMap = new Dictionary<Type, string>(ControlPrefixMap);
            foreach (var kvp in TMPControlPrefixMap)
            {
                if (kvp.Key != null && !prefixMap.ContainsKey(kvp.Key))
                {
                    prefixMap[kvp.Key] = kvp.Value;
                }
            }

            foreach (var comp in allUI)
            {
                Type type = comp.GetType();
                if (!prefixMap.TryGetValue(type, out string requiredPrefix))
                    continue;

                string goName = comp.gameObject.name;
                if (!goName.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string rawName = goName.Substring(requiredPrefix.Length);
                if (string.IsNullOrEmpty(rawName)) continue;

                // 生成字段名：首字母小写 + 驼峰 + 类型名（简化处理）
                string fieldName = char.ToLowerInvariant(rawName[0]) + rawName.Substring(1);
                // 避免与常见类型后缀冲突，简单追加类型简称
                string typeSuffix = GetTypeSuffix(type);
                if (!fieldName.EndsWith(typeSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    fieldName += typeSuffix;
                }

                result.Add(new CollectedControl
                {
                    Component = comp,
                    FieldName = fieldName,
                    GameObjectName = goName,
                    FieldType = type
                });
            }

            // 去重（同一 GameObject 上的多个组件理论上不会，但保险起见）
            return result
                .GroupBy(c => c.FieldName)
                .Select(g => g.First())
                .OrderBy(c => c.FieldName)
                .ToList();
        }

        private static string GetTypeSuffix(Type type)
        {
            if (type == typeof(Button)) return "Button";
            if (type == typeof(Image)) return "Image";
            if (type == typeof(RawImage)) return "RawImage";
            if (type == typeof(Text)) return "Text";
            if (type == typeof(Slider)) return "Slider";
            if (type == typeof(Toggle)) return "Toggle";
            if (type == typeof(Dropdown)) return "Dropdown";
            if (type == typeof(InputField)) return "InputField";
            if (type == typeof(ScrollRect)) return "ScrollRect";

            // TMP
            string fullName = type.FullName ?? type.Name;
            if (fullName.Contains("TMP_Text")) return "TMPText";
            if (fullName.Contains("TMP_InputField")) return "TMPInputField";
            if (fullName.Contains("TMP_Dropdown")) return "TMPDropdown";

            return type.Name;
        }

        private static string GenerateBaseClassCode(string prefabName, string baseClassName, List<CollectedControl> controls)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// 由代码生成器自动生成。");
            sb.AppendLine($"/// 对应预制体：{prefabName}.prefab");
            sb.AppendLine("/// 重复生成时会刷新字段与绑定。");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public abstract class {baseClassName} : UIPanelBase");
            sb.AppendLine("{");

            if (controls.Count == 0)
            {
                sb.AppendLine("    // 未找到符合命名规范的控件");
            }
            else
            {
                foreach (var ctrl in controls)
                {
                    string typeName = GetTypeDisplayName(ctrl.FieldType);
                    sb.AppendLine($"    [SerializeField] protected {typeName} {ctrl.FieldName};");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GetTypeDisplayName(Type type)
        {
            string full = type.FullName ?? type.Name;
            if (full.Contains("TMPro.TMP_Text")) return "TMP_Text";
            if (full.Contains("TMPro.TMP_InputField")) return "TMP_InputField";
            if (full.Contains("TMPro.TMP_Dropdown")) return "TMP_Dropdown";
            return type.Name;
        }

        private static string GenerateConcreteClassCode(string prefabName, string concreteClassName, string baseClassName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {prefabName} 面板逻辑类。");
            sb.AppendLine("/// 在此类中编写面板业务逻辑。");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public class {concreteClassName} : {baseClassName}");
            sb.AppendLine("{");
            sb.AppendLine("    protected override void OnOpen(object param)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 初始化逻辑");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    protected override void OnRefresh(object param)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 刷新逻辑");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    protected override void OnClose()");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 清理逻辑");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static bool AutoBindComponents(string prefabAssetPath, string baseClassName, List<CollectedControl> controls)
        {
            // 重新加载 prefab 进行修改
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabAssetPath);
            if (prefabRoot == null) return false;

            try
            {
                // 查找 Base 类类型
                Type baseType = FindTypeInCurrentAssembly(baseClassName);
                if (baseType == null)
                {
                    Debug.LogWarning($"[UIPanelCodeGenerator] 无法找到类型 {baseClassName}，跳过自动绑定。请手动挂载。");
                    return false;
                }

                Component existing = prefabRoot.GetComponent(baseType);
                if (existing == null)
                {
                    existing = prefabRoot.AddComponent(baseType);
                }

                SerializedObject so = new SerializedObject(existing);
                so.Update();

                bool anyBound = false;
                foreach (var ctrl in controls)
                {
                    SerializedProperty prop = so.FindProperty(ctrl.FieldName);
                    if (prop == null) continue;

                    // ctrl.Component 是从临时加载的 prefabRoot 里拿到的，需要找到对应路径上的组件
                    // 这里直接用 GameObject 名字 + 类型重新查找更可靠
                    Component targetComp = FindComponentByNameAndType(prefabRoot, ctrl.GameObjectName, ctrl.FieldType);
                    if (targetComp != null)
                    {
                        prop.objectReferenceValue = targetComp;
                        anyBound = true;
                    }
                }

                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);
                return anyBound || controls.Count == 0;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static Type FindTypeInCurrentAssembly(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Assembly-CSharp"))
                {
                    Type t = assembly.GetType(typeName);
                    if (t != null) return t;
                }
            }
            return null;
        }

        private static Component FindComponentByNameAndType(GameObject root, string goName, Type type)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t.name == goName)
                {
                    Component comp = t.GetComponent(type);
                    if (comp != null) return comp;
                }
            }
            return null;
        }

        private class CollectedControl
        {
            public Component Component;
            public string FieldName;
            public string GameObjectName;
            public Type FieldType;
        }
    }
}
