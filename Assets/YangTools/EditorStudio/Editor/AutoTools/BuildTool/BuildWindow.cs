#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using YangTools;

public class BuildWindow : EditorWindow
{
    private static BuildWindow Instance;

    public List<BuildData> buildDatas = new List<BuildData>();
    public BuildData select;
    private static string dataPath = "Assets\\YangTools\\EditorStudio\\Editor\\SaveData";

    [MenuItem(SettingInfo.YongToolsBuildToolPath + "打包配置窗口", priority = 100000)]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        BuildWindow window = (BuildWindow) EditorWindow.GetWindow(typeof(BuildWindow));
        Instance = window;
        ReloadUpdateShow();
        Instance?.Show();
    }

    /// <summary>
    /// 获得本地选择的配置
    /// </summary>
    public static BuildData GetLocalSelect()
    {
        List<BuildData> all = LoadAllAssetsAtPath<BuildData>(dataPath);
        //选择上次选中的
        if (EditorPrefs.HasKey(EditorBuildWindowSelectKey))
        {
            string old = EditorPrefs.GetString(EditorBuildWindowSelectKey);
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].showName == old)
                {
                    return all[i];
                }
            }
        }

        return null;
    }

    #region 加载本地配置

    /// <summary>
    /// 重新加载更新显示
    /// </summary>
    private static void ReloadUpdateShow()
    {
        Instance?.LoadAllBuildData(dataPath + "\\");
    }

    private void LoadAllBuildData(string path)
    {
        buildDatas.Clear();
        List<BuildData> all = LoadAllAssetsAtPath<BuildData>(path);
        buildDatas.AddRange(all);

        //选择上次选中的
        if (EditorPrefs.HasKey(EditorBuildWindowSelectKey))
        {
            string old = EditorPrefs.GetString(EditorBuildWindowSelectKey);
            for (int i = 0; i < buildDatas.Count; i++)
            {
                if (buildDatas[i].showName == old)
                {
                    SetSelect(buildDatas[i]);
                    break;
                }
            }
        }
    }

    public static List<T> LoadAllAssetsAtPath<T>(string folderPath) where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] {folderPath});
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }

        return assets;
    }

    #endregion

    #region 生命周期

    // /// <summary>
    // /// 启用
    // /// </summary>
    // void OnEnable()
    // {
    //     Debug.LogError("启用");
    // }
    //
    // /// <summary>
    // /// 聚焦
    // /// </summary>
    // private void OnFocus()
    // {
    //     Debug.LogError("聚焦");
    // }
    //
    // /// <summary>
    // /// 失去聚焦
    // /// </summary>
    // void OnLostFocus()
    // {
    //     Debug.LogError("失去聚焦");
    // }
    //
    // /// <summary>
    // /// 删除
    // /// </summary>
    // void OnDestroy()
    // {
    //     Debug.LogError("删除");
    // }

    /// <summary>
    /// 界面改变
    /// </summary>
    private void OnHierarchyChange()
    {
        Debug.LogError("界面改变");
    }

    #endregion

    #region OnGUI

    private string createName;

    void OnGUI()
    {
        //头部显示设置列表
        var style = EditorStyle.Get;

        #region 设置列表

        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < buildDatas.Count; i++)
        {
            if (GUILayout.Button(buildDatas[i].showName,
                    select == buildDatas[i] ? style.menuButtonSelected : style.menuButton))
            {
                SetSelect(buildDatas[i]);
            }
        }

        EditorGUILayout.EndHorizontal();

        #endregion

        EditorGUILayout.Space(20);

        #region 管理UI

        //管理UI--创建删除按钮
        EditorGUILayout.BeginHorizontal();
        createName = GUILayout.TextField(string.IsNullOrEmpty(createName)
            ? $"BuildData_{DateTime.Now:yyyy_MM_dd_HHHH_mm_ss}"
            : createName);
        if (GUILayout.Button("创建", style.menuButton))
        {
            Create();
        }

        if (GUILayout.Button("删除", style.menuButton))
        {
            Delete(select);
        }

        EditorGUILayout.EndHorizontal();

        #endregion

        #region

        if (select != null)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("配置名:");
            select.showName = GUILayout.TextField(select.showName);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.Label("宏定义:");
            DrawDefineList(select.defineList, ref objectScrollPosition);

            GUILayout.Space(20);
            GUILayout.Label("构建配置:");

            select.buildTarget = (BuildTargetGroup) EditorGUILayout.EnumPopup("构建平台", select.buildTarget);
            select.buildPath = EditorGUILayout.TextField("构建路径", select.buildPath);
            select.companyName = EditorGUILayout.TextField("公司名", select.companyName);
            select.buildName = EditorGUILayout.TextField("构建名", select.buildName);
            select.buildVersion = EditorGUILayout.TextField("构建版本", select.buildVersion);
            select.buildNumberCode = EditorGUILayout.IntField("内部版本号", select.buildNumberCode);

            select.bundleVersion = EditorGUILayout.TextField("AB包名", select.bundleVersion);
            select.adsId = EditorGUILayout.TextField("广告ID", select.adsId);
            select.tips = EditorGUILayout.TextField("提示", select.tips);

            GUILayout.Space(60);
            if (GUILayout.Button("应用配置到项目设置"))
            {
                Apply(select);
            }
        }

        #endregion
    }

    #endregion

    #region 宏定义

    private Vector2 objectScrollPosition;

    private void DrawDefineList(List<DefineSymbol> defineList, ref Vector2 scrollPos)
    {
        EditorGUILayout.BeginVertical();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(false));
        var allGroup = defineList.GroupBy(x => x.Group);
        foreach (var oneGroup in allGroup)
        {
            EditorGUILayout.LabelField(oneGroup.Key);
            foreach (var item in oneGroup)
            {
                DrawItem(item);
            }
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("添加宏"))
        {
            DefineSymbol newSymbol = new DefineSymbol();
            newSymbol.IsOpen = true;
            newSymbol.Name = "Default";
            newSymbol.Desc = "描述";
            EditorDefineSymbolWindow.DisplayWizard("添加宏定义", "确认", newSymbol,
                () => { select.defineList.Add(newSymbol); });
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawItem(DefineSymbol item)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(10);
        item.IsOpen = EditorGUILayout.Toggle(item.IsOpen, GUILayout.Width(20));
        GUI.enabled = item.IsOpen;
        EditorGUILayout.LabelField(item.Name);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"/*{item.Desc ?? ""}*/");
        GUI.enabled = true;

        if (GUILayout.Button("编辑"))
        {
            EditorDefineSymbolWindow.DisplayWizard("修改宏定义", "确认", item, () => { });
        }

        if (GUILayout.Button("删除"))
        {
            select.defineList.Remove(item);
        }

        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region 数据选择

    private const string EditorBuildWindowSelectKey = "Editor_BuildWindow_Select";

    public void SetSelect(BuildData data)
    {
        EditorPrefs.SetString(EditorBuildWindowSelectKey, data.showName);
        select = data;
    }

    public void Create()
    {
        string showName = (string.IsNullOrEmpty(createName)
            ? $"buildData_{DateTime.Now:yyyy_MM_dd_HHHH_mm_ss}.asset"
            : $"{createName}.asset");
        Debug.Log($"创建：{showName}");
        //创建数据资源文件
        BuildData asset = ScriptableObject.CreateInstance<BuildData>();
        asset.showName = showName.Replace(".asset", "");
        //保存到本地
        //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
        string path = Path.Combine(dataPath, showName);
        Debug.Log($"路径：{path}");
        AssetDatabase.CreateAsset(asset, path);
        //保存创建的资源
        AssetDatabase.SaveAssets();
        //刷新界面
        AssetDatabase.Refresh();
        ReloadUpdateShow();
    }

    public void Delete(BuildData data)
    {
        string path = AssetDatabase.GetAssetPath(data);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ReloadUpdateShow();
    }

    #endregion

    #region 配置应用

    public static void Apply(BuildData data)
    {
        UpdateBase(data);
        ApplyScriptingDefineSymbols(data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void UpdateBase(BuildData data)
    {
        BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

        PlayerSettings.companyName = data.companyName;
        PlayerSettings.productName = data.buildName;
        NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(data.buildTarget);
        //包名
        PlayerSettings.SetApplicationIdentifier(buildTarget, $"com.{data.companyName}.{data.buildName}");

        // Force il2cpp on ios
        var isIL2CPP = currentBuildTarget == BuildTarget.iOS;

        PlayerSettings.SetScriptingBackend(buildTarget, isIL2CPP
            ? ScriptingImplementation.IL2CPP
            : ScriptingImplementation.Mono2x);

        // Android fix architecture
        if (currentBuildTarget == BuildTarget.Android)
        {
            var targetArchitectures = AndroidArchitecture.ARMv7;
            if (isIL2CPP)
            {
                targetArchitectures |= AndroidArchitecture.ARM64;
            }

            PlayerSettings.Android.targetArchitectures = targetArchitectures;
        }

        PlayerSettings.bundleVersion = data.buildVersion;

        if (currentBuildTarget == BuildTarget.Android)
        {
            PlayerSettings.Android.bundleVersionCode = data.buildNumberCode;
        }
        else if (currentBuildTarget == BuildTarget.iOS)
        {
            PlayerSettings.iOS.buildNumber = data.buildNumberCode.ToString();
        }
        else if (currentBuildTarget == BuildTarget.WebGL)
        {
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public static bool ApplyScriptingDefineSymbols(BuildData data)
    {
        if (data.buildTarget == BuildTargetGroup.Unknown) return false;
        data.buildTarget = BuildTargetGroup.Android;
        string defineStr = data.DefineStr;
        NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(data.buildTarget);
        string old = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
        if (old != defineStr)
        {
            for (int i = 0; i < data.defineList.Count; i++)
            {
                if (data.defineList[i].IsOpen && !old.Contains(data.defineList[i].Name))
                {
                    old += $";{data.defineList[i].Name}";
                }
                else if (!data.defineList[i].IsOpen && old.Contains(data.defineList[i].Name))
                {
                    old = old.Replace($";{data.defineList[i].Name}", "");
                    //防止是第一个
                    if (old.StartsWith($"{data.defineList[i].Name}"))
                        old = old.Replace($"{data.defineList[i].Name}", "");
                }
            }

            PlayerSettings.SetScriptingDefineSymbols(buildTarget, old);
            return true;
        }

        return false;
    }

    #endregion
}

public class EditorStyle
{
    private static EditorStyle style = null;

    public GUIStyle area;
    public GUIStyle areaPadded;

    public GUIStyle menuButton;
    public GUIStyle menuButtonSelected;
    public GUIStyle smallSquareButton;

    public GUIStyle heading;
    public GUIStyle subheading;
    public GUIStyle subheading2;

    public GUIStyle boldLabelNoStretch;

    public GUIStyle link;

    public GUIStyle toggle;

    public Texture2D saveIconSelected;
    public Texture2D saveIconUnselected;

    public static EditorStyle Get
    {
        get
        {
            if (style == null) style = new EditorStyle();
            return style;
        }
    }

    public EditorStyle()
    {
        // An area with padding.
        area = new GUIStyle();
        area.padding = new RectOffset(10, 10, 10, 10);
        area.wordWrap = true;

        // An area with more padding.
        areaPadded = new GUIStyle();
        areaPadded.padding = new RectOffset(20, 20, 20, 20);
        areaPadded.wordWrap = true;

        // Unselected menu button.
        menuButton = new GUIStyle(EditorStyles.toolbarButton);
        menuButton.fontStyle = FontStyle.Normal;
        menuButton.fontSize = 14;
        menuButton.fixedHeight = 24;

        // Selected menu button.
        menuButtonSelected = new GUIStyle(menuButton);
        menuButtonSelected.fontStyle = FontStyle.Bold;

        // Main Headings
        heading = new GUIStyle(EditorStyles.label);
        heading.fontStyle = FontStyle.Bold;
        heading.fontSize = 24;

        subheading = new GUIStyle(heading);
        subheading.fontSize = 18;

        subheading2 = new GUIStyle(heading);
        subheading2.fontSize = 14;

        boldLabelNoStretch = new GUIStyle(EditorStyles.label);
        boldLabelNoStretch.stretchWidth = false;
        boldLabelNoStretch.fontStyle = FontStyle.Bold;

        link = new GUIStyle();
        link.fontSize = 16;
        if (EditorGUIUtility.isProSkin)
            link.normal.textColor = new Color(0.262f, 0.670f, 0.788f);
        else
            link.normal.textColor = new Color(0.129f, 0.129f, 0.8f);

        toggle = new GUIStyle(EditorStyles.toggle);
        toggle.stretchWidth = false;

        // saveIconSelected = AssetDatabase.LoadAssetAtPath<Texture2D>(ES3Settings.PathToEasySaveFolder() + "Editor/es3Logo16x16.png");
        // saveIconUnselected = AssetDatabase.LoadAssetAtPath<Texture2D>(ES3Settings.PathToEasySaveFolder() + "Editor/es3Logo16x16-bw.png");
    }
}
#endif