using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// GameWindow 图片 Physics Shape 批量生成工具----unity批量生产2D图片的不规则物理形状(碰撞体)
/// </summary>
public sealed class GameWindowPhysicsShapeTool : EditorWindow
{
    private const string DefaultFolder = "Assets/AssetBundle/Art/UI/GameWindow/GameIcon";
    private const string PhysicsShapeProperty = "m_SpriteGenerateFallbackPhysicsShape";
    private const float DefaultOutlineDetail = 0.5f;
    private const byte AlphaTolerance = 0;

    private string folderPath = DefaultFolder;
    private bool onlyMissingShape = true;
    private float outlineDetail = DefaultOutlineDetail;

    /// <summary>打开 Physics Shape 批量生成窗口。</summary>
    [MenuItem("Tools/GameWindow/Physics Shape 批量生成")]
    private static void OpenWindow()
    {
        GetWindow<GameWindowPhysicsShapeTool>("Physics Shape 工具");
    }

    /// <summary>从当前 Project 窗口选中的文件夹打开工具。</summary>
    [MenuItem("Assets/GameWindow/生成 Physics Shape", false, 2000)]
    private static void OpenFromSelection()
    {
        GameWindowPhysicsShapeTool window = GetWindow<GameWindowPhysicsShapeTool>("Physics Shape 工具");
        string selectedFolder = GetSelectedFolder();
        if (!string.IsNullOrEmpty(selectedFolder))
        {
            window.folderPath = selectedFolder;
        }

        window.Show();
    }

    /// <summary>验证当前菜单是否选中了项目文件夹。</summary>
    [MenuItem("Assets/GameWindow/生成 Physics Shape", true)]
    private static bool ValidateOpenFromSelection()
    {
        return !string.IsNullOrEmpty(GetSelectedFolder());
    }

    /// <summary>绘制批量生成工具界面。</summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("批量生成图片 Physics Shape", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("根据图片透明区域生成 Sprite Editor 中的 Physics Shape。生成结果会写入图片导入资源。", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("图片目录", folderPath);
        if (GUILayout.Button("选择", GUILayout.Width(60f)))
        {
            string selectedFolder =
                EditorUtility.OpenFolderPanel("选择图片目录", GetAbsoluteFolderPath(folderPath), string.Empty);
            string assetFolder = ConvertToAssetPath(selectedFolder);
            if (!string.IsNullOrEmpty(assetFolder))
            {
                folderPath = assetFolder;
            }
        }

        EditorGUILayout.EndHorizontal();
        onlyMissingShape = EditorGUILayout.ToggleLeft("只处理没有 Physics Shape 的图片", onlyMissingShape);
        outlineDetail = EditorGUILayout.Slider("轮廓细节", outlineDetail, 0f, 1f);

        if (GUILayout.Button("开始生成", GUILayout.Height(30f)))
        {
            GeneratePhysicsShapes(folderPath, onlyMissingShape, outlineDetail);
        }
    }

    /// <summary>批量查找目录下的图片并生成 Physics Shape。</summary>
    private static void GeneratePhysicsShapes(string folder, bool processOnlyMissing, float detail)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            EditorUtility.DisplayDialog("Physics Shape", "目录不存在：" + folder, "确定");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] {folder});
        List<string> allPaths = new List<string>();
        List<string> missingPaths = new List<string>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!IsSpriteTexture(path))
            {
                continue;
            }

            allPaths.Add(path);
            if (HasMissingPhysicsShape(path))
            {
                missingPaths.Add(path);
            }
        }

        List<string> paths = processOnlyMissing ? missingPaths : allPaths;
        if (paths.Count == 0)
        {
            if (allPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Physics Shape", "当前目录及子目录没有找到 Sprite 图片。", "确定");
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex("Physics Shape",
                "找到 " + allPaths.Count + " 张图片，但它们都已有 Physics Shape。是否全部重新生成？",
                "全部生成", "取消", "返回");
            if (choice != 0)
            {
                return;
            }

            paths = allPaths;
        }

        try
        {
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < paths.Count; i++)
            {
                GeneratePhysicsShape(paths[i], detail);
                EditorUtility.DisplayProgressBar("生成 Physics Shape", paths[i], (i + 1f) / paths.Count);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Physics Shape", "处理完成，共处理 " + paths.Count + " 张图片。", "确定");
    }

    /// <summary>为单张图片生成并写入 Sprite Editor 的自定义 Physics Outline。</summary>
    private static void GeneratePhysicsShape(string path, float detail)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (dataProvider == null)
        {
            EnableFallbackPhysicsShape(importer, path);
            return;
        }

        dataProvider.InitSpriteEditorDataProvider();
        ISpritePhysicsOutlineDataProvider physicsProvider =
            dataProvider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
        ITextureDataProvider textureProvider = dataProvider.GetDataProvider<ITextureDataProvider>();
        Texture2D texture = textureProvider?.GetReadableTexture2D();
        if (physicsProvider == null || texture == null)
        {
            EnableFallbackPhysicsShape(importer, path);
            return;
        }

        textureProvider.GetTextureActualWidthAndHeight(out int actualWidth, out int actualHeight);
        Vector2 scale = new Vector2(texture.width / (float) actualWidth, texture.height / (float) actualHeight);
        SpriteRect[] spriteRects = dataProvider.GetSpriteRects();
        for (int i = 0; i < spriteRects.Length; i++)
        {
            SpriteRect spriteRect = spriteRects[i];
            Rect scaledRect = spriteRect.rect;
            scaledRect.xMin *= scale.x;
            scaledRect.xMax *= scale.x;
            scaledRect.yMin *= scale.y;
            scaledRect.yMax *= scale.y;

            if (!TryGenerateOutline(texture, scaledRect, detail, out Vector2[][] generatedPaths))
            {
                EnableFallbackPhysicsShape(importer, path);
                return;
            }

            List<Vector2[]> paths = new List<Vector2[]>(generatedPaths.Length);
            Rect localRect = new Rect(Vector2.zero, spriteRect.rect.size);
            localRect.center = Vector2.zero;
            for (int pathIndex = 0; pathIndex < generatedPaths.Length; pathIndex++)
            {
                Vector2[] generatedPath = generatedPaths[pathIndex];
                Vector2[] localPath = new Vector2[generatedPath.Length];
                for (int pointIndex = 0; pointIndex < generatedPath.Length; pointIndex++)
                {
                    Vector2 point = new Vector2(generatedPath[pointIndex].x / scale.x,
                        generatedPath[pointIndex].y / scale.y);
                    localPath[pointIndex] = CapPointToRect(point, localRect);
                }

                paths.Add(localPath);
            }

            physicsProvider.SetOutlines(spriteRect.spriteID, paths);
            physicsProvider.SetTessellationDetail(spriteRect.spriteID, detail);
        }

        dataProvider.Apply();
        importer.SaveAndReimport();
    }

    /// <summary>通过 Unity 编辑器内部 API 调用 Sprite Editor 的轮廓生成算法。</summary>
    private static bool TryGenerateOutline(Texture2D texture, Rect rect, float detail, out Vector2[][] paths)
    {
        paths = null;
        Type utilityType = Type.GetType("UnityEditor.Sprites.SpriteUtility, UnityEditor.CoreModule");
        MethodInfo method = utilityType?.GetMethod("GenerateOutline", BindingFlags.Public | BindingFlags.NonPublic |
                                                                      BindingFlags.Static);
        if (method == null)
        {
            return false;
        }

        object[] arguments = {texture, rect, detail, AlphaTolerance, true, null};
        method.Invoke(null, arguments);
        paths = arguments[5] as Vector2[][];
        return paths != null;
    }

    /// <summary>当当前导入器不支持自定义数据接口时启用 Unity 的回退生成。</summary>
    private static void EnableFallbackPhysicsShape(TextureImporter importer, string path)
    {
        SerializedObject serializedImporter = new SerializedObject(importer);
        SerializedProperty property = serializedImporter.FindProperty(PhysicsShapeProperty);
        if (property == null)
        {
            Debug.LogWarning("图片导入器不支持 Physics Shape 属性：" + path);
            return;
        }

        property.boolValue = true;
        serializedImporter.ApplyModifiedPropertiesWithoutUndo();
        importer.SaveAndReimport();
    }

    /// <summary>将生成的轮廓点限制在当前 Sprite 的矩形范围内。</summary>
    private static Vector2 CapPointToRect(Vector2 point, Rect rect)
    {
        point.x = Mathf.Clamp(point.x, rect.xMin, rect.xMax);
        point.y = Mathf.Clamp(point.y, rect.yMin, rect.yMax);
        return point;
    }

    /// <summary>判断图片是否为 Sprite 类型。</summary>
    private static bool IsSpriteTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        return importer != null && importer.textureType == TextureImporterType.Sprite;
    }

    /// <summary>判断图片的任意 Sprite 是否没有 Physics Shape。</summary>
    private static bool HasMissingPhysicsShape(string path)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null && sprite.GetPhysicsShapeCount() == 0)
            {
                return true;
            }
        }

        Sprite mainSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return mainSprite != null && mainSprite.GetPhysicsShapeCount() == 0;
    }

    /// <summary>获取当前 Project 窗口选中的项目文件夹。</summary>
    private static string GetSelectedFolder()
    {
        UnityEngine.Object selectedObject = Selection.activeObject;
        string path = selectedObject == null ? string.Empty : AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            int separatorIndex = path.LastIndexOf('/');
            path = separatorIndex > 0 ? path.Substring(0, separatorIndex) : string.Empty;
        }

        return AssetDatabase.IsValidFolder(path) ? path : string.Empty;
    }

    /// <summary>将 Unity 的项目相对目录转换为系统文件夹选择框使用的绝对路径。</summary>
    private static string GetAbsoluteFolderPath(string assetPath)
    {
        string relativePath = assetPath.StartsWith("Assets/") ? assetPath.Substring("Assets/".Length) : string.Empty;
        return string.IsNullOrEmpty(relativePath)
            ? Application.dataPath
            : System.IO.Path.Combine(Application.dataPath, relativePath);
    }

    /// <summary>将系统文件夹选择框返回的绝对路径转换为 Assets 相对路径。</summary>
    private static string ConvertToAssetPath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
        {
            return string.Empty;
        }

        string normalizedAssetsPath = Application.dataPath.Replace('\\', '/');
        string normalizedSelectedPath = absolutePath.Replace('\\', '/').TrimEnd('/');
        if (string.Equals(normalizedSelectedPath, normalizedAssetsPath, System.StringComparison.OrdinalIgnoreCase))
        {
            return "Assets";
        }

        string assetsPrefix = normalizedAssetsPath + "/";
        if (!normalizedSelectedPath.StartsWith(assetsPrefix, System.StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Physics Shape", "请选择当前 Unity 项目 Assets 目录下的文件夹。", "确定");
            return string.Empty;
        }

        return "Assets/" + normalizedSelectedPath.Substring(assetsPrefix.Length);
    }
}