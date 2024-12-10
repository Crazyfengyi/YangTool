#if UNITY_EDITOR
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 转换模式
/// </summary>
public enum ChangeMode
{
    Txt2Excel,
    Excel2Txt
}
public class DataChange : EditorWindow
{
    /// <summary>
    /// 当前编辑器窗口实例
    /// </summary>
    private static DataChange instance;
    /// <summary>
    /// 转换模式
    /// </summary>
    public static ChangeMode changeMode = ChangeMode.Txt2Excel;
    /// <summary>
    /// 项目根路径	
    /// </summary>
    private static string pathRoot;
    /// <summary>
    /// 滚动窗口初始位置
    /// </summary>
    private static Vector2 scrollPos;
    /// <summary>
    /// Excel文件列表
    /// </summary>
    private static List<string> excelPathList;
    /// <summary>
    /// Txt文件
    /// </summary>
    public static List<string> txtPathList;
    //选中的导出路径
    private static string _m_sExportPath = @"D:\Document\ETG_Text2Excel\Assets\DataExport\Resources\Result";

    [MenuItem("YangTools/DataTool/DataChange")]
    static void CreateBaseData()
    {
        Init();
        //加载Excel文件
        LoadExcel();
        instance.Show();
        DataChange window = (DataChange)GetWindow(typeof(DataChange));
    }
    /// <summary>
    /// 初始化
    /// </summary>
    private static void Init()
    {
        //获取当前实例
        instance = (DataChange)GetWindow(typeof(DataChange));
        InitPath();
        excelPathList = new List<string>();
        txtPathList = new List<string>();
        scrollPos = new Vector2(instance.position.x, instance.position.y + 75);
    }
    public static void InitPath()
    {
        //初始化
        pathRoot = Application.dataPath;
        //注意这里需要对路径进行处理
        //目的是去除Assets这部分字符以获取项目目录
        pathRoot = pathRoot.Substring(0, pathRoot.LastIndexOf("/"));
    }
    void OnGUI()
    {
        DrawOptions();

        switch (changeMode)
        {
            case ChangeMode.Txt2Excel:
                DrawTxt2Excel();
                break;
            case ChangeMode.Excel2Txt:
                DrawExcel2Txt();
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 绘制插件界面配置项
    /// </summary>
    private void DrawOptions()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("请选择转换类型:", GUILayout.Width(85));
        changeMode = (ChangeMode)EditorGUILayout.EnumPopup(changeMode);
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
    /// 绘制Excel2Txt
    /// </summary>
    private void DrawExcel2Txt()
    {
        if (excelPathList == null) return;
        if (excelPathList.Count < 1)
        {
            EditorGUILayout.LabelField("目前没有Excel文件被选中哦!");
        }
        else
        {
            EditorGUILayout.LabelField("下列项目将被转换:");
            GUILayout.BeginVertical();
            float hight = excelPathList.Count * 18.75f;
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(Mathf.Min(hight, 250)));
            foreach (string s in excelPathList)
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
                Excel2Txt();
            }
        }
    }
    /// <summary>
    /// 绘制Txt2Excel
    /// </summary>
    private void DrawTxt2Excel()
    {
        if (txtPathList == null) return;
        if (txtPathList.Count < 1)
        {
            EditorGUILayout.LabelField("目前没有Txt文件被选中哦!");
        }
        else
        {
            EditorGUILayout.LabelField("下列项目将被转换:");
            GUILayout.BeginVertical();
            float hight = txtPathList.Count * 18.75f;
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(Mathf.Min(hight, 250)));
            foreach (string s in txtPathList)
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
                Txt2Excel();
            }
        }
    }
    void OnSelectionChange()
    {
        if (instance == null)
        {
            Init();
        }

        InitPath();
        //当选择发生变化时重绘窗体
        Show();
        LoadExcel();
        LoadTxt();
        Repaint();
    }
    /// <summary>
    /// 加载Excel
    /// </summary>
    private static void LoadExcel()
    {
        if (excelPathList == null) excelPathList = new List<string>();
        excelPathList.Clear();
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
                excelPathList.Add(objPath);
            }
        }
    }
    /// <summary>
    /// 加载Txt
    /// </summary>
    public static void LoadTxt()
    {
        if (txtPathList == null) txtPathList = new List<string>();
        txtPathList.Clear();
        //获取选中的对象
        object[] selection = (object[])Selection.objects;
        //判断是否有对象被选中
        if (selection.Length == 0)
            return;
        //遍历每一个对象判断不是Excel文件
        foreach (Object obj in selection)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            if (objPath.EndsWith(".txt"))
            {
                txtPathList.Add(objPath);
            }
        }
    }
    /// <summary>
    /// 转换Excel2Txt
    /// </summary>
    private static void Excel2Txt()
    {
        foreach (string assetsPath in excelPathList)
        {
            StringBuilder sb = new StringBuilder();
            //获取Excel文件的绝对路径
            string excelPath = pathRoot + "/" + assetsPath;
            FileStream fs = new FileStream(excelPath, FileMode.Open, FileAccess.ReadWrite);
            using (ExcelPackage package = new ExcelPackage(fs))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets[1];
                int row = sheet.Dimension.End.Row;//行
                for (int i = 1; i <= row; i++)
                {
                    try
                    {
                        object value1 = sheet.GetValue(i, 1);
                        object value2 = sheet.GetValue(i, 2);
                        if (value1 != null)
                        {
                            sb.AppendLine("#" + value1);
                        }
                        else
                        {
                            Debug.LogError($"{i}行的key为空");
                        }
                        if (value2 != null)
                        {
                            sb.AppendLine(value2.ToString());
                        }
                        else
                        {
                            sb.AppendLine("");
                        }
                    }
                    catch (Exception)
                    {
                        Debug.LogError($"报错:{i}");
                    }
                }
            }
            fs.Close();
            fs.Dispose();

            string[] split = excelPath.Split("/");
            string name = split[split.Length - 1];
            name = name.Replace(".xlsx", "");
            //目标文件
            string targetPath = _m_sExportPath + $"\\{name}.txt";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Create(targetPath).Dispose();

            FileStream fileStream = new FileStream(targetPath, FileMode.Open, FileAccess.ReadWrite);

            string str = sb.ToString();
            str = str.Replace("\r\n", "\n");
            str = str.Replace("\n", "\r\n");
            //将字符串转换为字节数组
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            //向文件中写入字节数组
            fileStream.Write(bytes, 0, bytes.Length);
            //刷新缓冲区
            fileStream.Flush();
            //关闭流
            fileStream.Close();
        }

        //刷新本地资源
        AssetDatabase.Refresh();
        Debug.Log("导出Txt");
        //转换完后关闭插件
        //这样做是为了解决窗口
        //再次点击时路径错误的Bug
        instance.Close();
    }
    /// <summary>
    /// 转换Txt2Excel
    /// </summary>
    private static void Txt2Excel()
    {
        for (int i = 0; i < txtPathList.Count; i++)
        {
            ChangeOne(txtPathList[i]);
        }

        void ChangeOne(string path)
        {
            string[] split = path.Split("/");
            string name = split[split.Length - 1];
            name = name.Replace(".txt", "");
            //目标文件
            string targetPath = _m_sExportPath + $"\\{name}.xlsx";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            var str = new StringBuilder();
            List<string> temp = new List<string>();
            var others = new Dictionary<string, string>();

            using (var reader = File.OpenText(path))
            {
                var allText = reader.ReadToEnd();
                var splition = allText.Split('#');

                for (var i = 0; i < splition.Length;)
                {
                    if (string.IsNullOrWhiteSpace(splition[i]))
                    {
                        ++i;
                        continue;
                    }

                    var enterKey = splition[i].IndexOf('\n');

                    if (enterKey == -1) Debug.LogError(splition[i]);

                    var key = splition[i].Substring(0, enterKey - 1);
                    var value = splition[i].Substring(enterKey + 1, splition[i].Length - enterKey - 3);
                    {
                        others[key] = value;
                        ++i;
                    }
                }
            }

            FileInfo newFile = new FileInfo(targetPath);
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Test1");
                for (int k = 0; k < others.Count; k++)
                {
                    KeyValuePair<string, string> item = others.ElementAt(k);
                    worksheet.Cells[k + 1, 1].Value = item.Key;
                    worksheet.Cells[k + 1, 2].Value = item.Value;
                }

                //设置基本样式
                //worksheet.Cells.Style.Font.Name = "宋体";
                //worksheet.Cells.Style.Font.Size = 11F;

                //设置第一行列标题样式
                using (ExcelRange r = worksheet.Cells[1, 1, others.Count, 2])
                {
                    //r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    //r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);
                }

                worksheet.Column(1).Width = 40;
                worksheet.Column(2).Width = 60;

                package.Save();
                Debug.Log("导出Excel");
            }
            AssetDatabase.Refresh();
        }
    }
}
#endif