using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YangTools.Scripts.Core;

/// <summary>
/// 预制体引用替换工具。
/// </summary>
public class ChangePrefabReferenceTool : EditorWindow
{
    public static GameObject targetObj;
    private static GameObject oldObj;
    private static GameObject newObj;
    private static bool changeRootNameToAssetName;

    /// <summary>
    /// 打开预制体引用替换测试窗口。
    /// </summary>
    [MenuItem(SettingInfo.MenuPath + "Test/预制体引用替换工具")]
    public static void OpenWindow()
    {
        GetWindow<ChangePrefabReferenceTool>("预制体替换工具");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("替换目标预制体内的旧预制体实例，可选择是否将实例根节点名称同步为新预制体名称。", MessageType.Info);
        targetObj = (GameObject) EditorGUILayout.ObjectField("目标预制体", targetObj, typeof(GameObject), false);
        oldObj = (GameObject) EditorGUILayout.ObjectField("旧预制体", oldObj, typeof(GameObject), false);
        newObj = (GameObject) EditorGUILayout.ObjectField("新预制体", newObj, typeof(GameObject), false);
        changeRootNameToAssetName = EditorGUILayout.Toggle("根节点名称同步为新预制体名称", changeRootNameToAssetName);

        EditorGUILayout.Space();
        if (GUILayout.Button("开始替换"))
        {
            ReplaceReferences();
        }
    }

    /// <summary>
    /// 替换目标预制体中对旧预制体的引用。
    /// </summary>
    private static void ReplaceReferences()
    {
        if (!TryGetPrefabAssets(out string targetPath, out GameObject oldPrefab, out GameObject newPrefab))
        {
            return;
        }

        if (!EditorUtility.DisplayDialog("确认替换",
                $"将“{targetObj.name}”中对“{oldPrefab.name}”的引用替换为“{newPrefab.name}”。\n此操作会保存目标预制体。",
                "替换", "取消"))
        {
            return;
        }

        GameObject prefabContentsRoot = null;
        try
        {
            prefabContentsRoot = PrefabUtility.LoadPrefabContents(targetPath);
            List<GameObject> instanceRoots = FindOldPrefabInstanceRoots(prefabContentsRoot, oldPrefab);
            int replacedCount = instanceRoots.Count;

            if (instanceRoots.Count > 0)
            {
                PrefabReplacingSettings settings = new PrefabReplacingSettings
                {
                    objectMatchMode = ObjectMatchMode.ByHierarchy,
                    changeRootNameToAssetName = changeRootNameToAssetName
                };
                PrefabUtility.ReplacePrefabAssetOfPrefabInstances(instanceRoots.ToArray(), newPrefab, settings,
                    InteractionMode.UserAction);
            }

            replacedCount += ReplaceObjectReferences(prefabContentsRoot, oldPrefab, newPrefab);

            if (replacedCount == 0)
            {
                EditorUtility.DisplayDialog("替换结果", "目标预制体中未找到旧预制体的引用。", "确定");
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContentsRoot, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("替换结果", $"替换完成，共替换 {replacedCount} 处引用。", "确定");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("替换失败", "替换预制体引用时发生异常，请查看 Console。", "确定");
        }
        finally
        {
            if (prefabContentsRoot != null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContentsRoot);
            }
        }
    }

    /// <summary>
    /// 获取并校验参与替换的预制体资源。
    /// </summary>
    private static bool TryGetPrefabAssets(out string targetPath, out GameObject oldPrefab, out GameObject newPrefab)
    {
        targetPath = GetPrefabAssetPath(targetObj);
        string oldPath = GetPrefabAssetPath(oldObj);
        string newPath = GetPrefabAssetPath(newObj);
        oldPrefab = null;
        newPrefab = null;

        if (string.IsNullOrEmpty(targetPath) || string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath))
        {
            EditorUtility.DisplayDialog("提示", "请指定目标、旧和新预制体。", "确定");
            return false;
        }

        if (targetPath == oldPath || targetPath == newPath || oldPath == newPath)
        {
            EditorUtility.DisplayDialog("提示", "目标、旧和新预制体必须是不同资源。", "确定");
            return false;
        }

        oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(oldPath);
        newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
        if (oldPrefab == null || newPrefab == null)
        {
            EditorUtility.DisplayDialog("提示", "无法加载旧或新预制体资源。", "确定");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 查找目标预制体中使用旧预制体资源的嵌套预制体实例根节点。
    /// </summary>
    private static List<GameObject> FindOldPrefabInstanceRoots(GameObject prefabContentsRoot, GameObject oldPrefab)
    {
        var instanceRoots = new List<GameObject>();
        Transform[] transforms = prefabContentsRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject instanceRoot = transforms[i].gameObject;
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(instanceRoot) ||
                PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot) != oldPrefab)
            {
                continue;
            }

            instanceRoots.Add(instanceRoot);
        }

        return instanceRoots;
    }

    /// <summary>
    /// 替换预制体内容中指向旧预制体根节点及其 Transform 的字段引用。
    /// </summary>
    private static int ReplaceObjectReferences(GameObject prefabContentsRoot, GameObject oldPrefab,
        GameObject newPrefab)
    {
        // 初始化替换计数器，用于记录被替换的引用数量
        int replacedCount = 0;
        // 获取预制体内容根目录下所有的子组件（包括非激活的）
        Component[] components = prefabContentsRoot.GetComponentsInChildren<Component>(true);
        // 遍历所有找到的组件
        for (int i = 0; i < components.Length; i++)
        {
            // 获取当前迭代的组件
            Component component = components[i];
            // 如果组件为空，则跳过当前循环
            if (component == null)
            {
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(component);
            replacedCount += ReplaceObjectReference(serializedObject, oldPrefab, newPrefab);
            replacedCount += ReplaceObjectReference(serializedObject, oldPrefab.transform, newPrefab.transform);
        }

        return replacedCount;
    }

    /// <summary>
    /// 替换序列化对象中指定的对象引用。
    /// </summary>
    private static int ReplaceObjectReference(SerializedObject serializedObject, UnityEngine.Object oldReference,
        UnityEngine.Object newReference)
    {
        // 初始化替换计数器，用于记录被替换的引用次数
        int replacedCount = 0;
        // 获取序列化对象的迭代器，用于遍历所有可见属性
        SerializedProperty property = serializedObject.GetIterator();
        // 设置是否进入子对象的标志，初始为true表示会遍历子对象
        bool enterChildren = true;
        // 遍历所有可见属性
        while (property.NextVisible(enterChildren))
        {
            // 遍历完子对象后，设置标志为false，避免重复遍历
            enterChildren = false;
            // 检查当前属性是否为对象引用类型，并且值是否等于旧引用
            // 如果不是对象引用类型或值不等于旧引用，则跳过当前属性
            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.objectReferenceValue != oldReference)
            {
                continue;
            }

            // 将当前属性的引用值更新为新引用
            property.objectReferenceValue = newReference;
            // 替换计数器加1
            replacedCount++;
        }

        // 如果有替换操作发生，则应用修改后的属性
        if (replacedCount > 0)
        {
            // 应用修改的属性，但不生成撤销操作
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        // 返回替换的引用次数
        return replacedCount;
    }

    /// <summary>
    /// 获取预制体资源路径。
    /// </summary>
    public static string GetPrefabAssetPath(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return null;
        }

        if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            return AssetDatabase.GetAssetPath(gameObject);
        }

        if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
        {
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
            return prefabAsset == null ? null : AssetDatabase.GetAssetPath(prefabAsset);
        }

        PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
        return prefabStage == null ? null : prefabStage.assetPath;
    }
}
