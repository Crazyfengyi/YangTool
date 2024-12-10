#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DataExportTool : EditorWindow
{
    /// <summary>
    /// 当前编辑器窗口实例
    /// </summary>
    private static DataExportTool instance;
    /// <summary>
    /// Excel文件列表
    /// </summary>
    private static List<string> excelList;
    /// <summary>
    /// 项目根路径	
    /// </summary>
    private static string pathRoot;
    /// <summary>
    /// 滚动窗口初始位置
    /// </summary>
    private static Vector2 scrollPos;
    /// <summary>
    /// 输出格式索引
    /// </summary>
    private static int indexOfFormat = 0;
    /// <summary>
    /// 输出格式
    /// </summary>
    private static string[] formatOption = new string[] { "JSON", "CSV", "XML" };
    /// <summary>
    /// 编码索引
    /// </summary>
    private static int indexOfEncoding = 0;
    /// <summary>
    /// 编码选项
    /// </summary>
    private static string[] encodingOption = new string[] { "UTF-8", "GB2312" };
    //选中的导出路径
    private static string _m_sExportPath;

    [MenuItem("YangTools/DataTool/CreateBaseData")]
    static void CreateBaseData()
    {
        Init();
        //加载Excel文件
        LoadExcel();
        instance.Show();

        //Rect rect = new Rect(0,0,550,900);
        //LangeuageDBTool window = (LangeuageDBTool)GetWindowWithRect(typeof(LangeuageDBTool), rect);
        DataExportTool window = (DataExportTool)GetWindow(typeof(DataExportTool));

    }

    void OnGUI()
    {
        DrawOptions();
        DrawExport();
    }
    /// <summary>
    /// 绘制插件界面输出项
    /// </summary>
    private void DrawExport()
    {
        if (excelList == null) return;
        if (excelList.Count < 1)
        {
            EditorGUILayout.LabelField("目前没有Excel文件被选中哦!");
        }
        else
        {
            EditorGUILayout.LabelField("下列项目将被转换为" + formatOption[indexOfFormat] + ":");
            GUILayout.BeginVertical();
            float hight = excelList.Count * 18.75f;
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(Mathf.Min(hight, 250)));
            foreach (string s in excelList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Toggle(true, s);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            //输出
            if (GUILayout.Button("转换"))
            {
                Convert();
            }
        }
    }

    /// <summary>
    /// 转换Excel文件
    /// </summary>
    private static void Convert()
    {
        foreach (string assetsPath in excelList)
        {
            //获取Excel文件的绝对路径
            string excelPath = pathRoot + "/" + assetsPath;
            //构造Excel工具类
            ExcelUtility excel = new ExcelUtility(excelPath);

            //判断编码类型
            Encoding encoding = null;
            if (indexOfEncoding == 0)
            {
                encoding = Encoding.GetEncoding("utf-8");
            }
            else if (indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("gb2312");
            }

            //判断输出类型
            if (indexOfFormat == 0)
            {
                excel.ConvertToJson(_m_sExportPath, encoding);
            }
            else if (indexOfFormat == 1)
            {
                excel.ConvertToCSV(_m_sExportPath, encoding);
            }
            else if (indexOfFormat == 2)
            {
                excel.ConvertToXml(_m_sExportPath);
            }

            //刷新本地资源
            AssetDatabase.Refresh();
        }

        //转换完后关闭插件
        //这样做是为了解决窗口
        //再次点击时路径错误的Bug
        instance.Close();

    }

    /// <summary>
    /// 绘制插件界面配置项
    /// </summary>
    private void DrawOptions()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("请选择格式类型:", GUILayout.Width(85));
        indexOfFormat = EditorGUILayout.Popup(indexOfFormat, formatOption, GUILayout.Width(125));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("请选择编码类型:", GUILayout.Width(85));
        indexOfEncoding = EditorGUILayout.Popup(indexOfEncoding, encodingOption, GUILayout.Width(125));
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("导出地址 ： ");

        EditorGUILayout.TextArea(_m_sExportPath);

        if (GUILayout.Button("Select Folder", GUILayout.Width(Screen.width)))
        {
            string currentDirectory;

            if (string.IsNullOrEmpty(_m_sExportPath))
            {
                currentDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                currentDirectory = _m_sExportPath;
            }

            _m_sExportPath = EditorUtility.OpenFolderPanel("Select Folder", currentDirectory, "*");
        }

        EditorGUILayout.EndVertical();

    }

    /// <summary>
    /// 加载Excel
    /// </summary>
    private static void LoadExcel()
    {
        if (excelList == null) excelList = new List<string>();
        excelList.Clear();
        //获取选中的对象
        object[] selection = (object[])Selection.objects;
        //判断是否有对象被选中
        if (selection.Length == 0)
            return;
        //遍历每一个对象判断不是Excel文件
        foreach (Object obj in selection)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            if (objPath.EndsWith(".xlsx"))
            {
                excelList.Add(objPath);
            }
        }
    }

    void OnSelectionChange()
    {
        //当选择发生变化时重绘窗体
        Show();
        LoadExcel();
        Repaint();
    }

    private static void Init()
    {
        //获取当前实例
        instance = (DataExportTool)GetWindow(typeof(DataExportTool));
        //初始化
        pathRoot = Application.dataPath;
        //注意这里需要对路径进行处理
        //目的是去除Assets这部分字符以获取项目目录
        //我表示Windows的/符号一直没有搞懂
        pathRoot = pathRoot.Substring(0, pathRoot.LastIndexOf("/"));
        excelList = new List<string>();
        scrollPos = new Vector2(instance.position.x, instance.position.y + 75);
    }

}
#endif