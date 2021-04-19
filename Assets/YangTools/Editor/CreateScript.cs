using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//TODO 标记有引用的物体 通过名字前加#
[ExecuteInEditMode]
[CustomEditor(typeof(RectTransform), true)]//bool 子类是否可用
public class CreateScript : Editor
{
    public static string AddTemplateString =
        @"partial class #类名#
        {
            #region 字段
            #替换字段#
            #endregion

            #region 方法
            #替换发方法#
            #endregion
        }";

    public static string CreateTemplateString =
        @"
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class #类名# : MonoBehaviour
{
    #成员#

    public void Start()
    {
        #初始化方法#
    }

    #方法#
}";

    private Editor editor;//RectTransformEditor
    Transform tagetTranform;//自身
    static bool m_Foldout;//收折菜单
    static List<AttributeInfo> attributeList = new List<AttributeInfo>();//要创建的属性
    //拼接字符串的stringBuilder
    StringBuilder strBuidler = new StringBuilder();
    StringBuilder strBuidler2 = new StringBuilder();
    StringBuilder strBuidler3 = new StringBuilder();

    private void OnEnable()
    {
        editor = Editor.CreateEditor(target, Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.RectTransformEditor", true));
        tagetTranform = target as RectTransform;
        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        attributeList.Clear();
        strBuidler.Clear();
        strBuidler2.Clear();
        strBuidler3.Clear();
    }

    public override void OnInspectorGUI()
    {
        editor.OnInspectorGUI();
        tagetTranform.position = EditorGUILayout.Vector3Field("世界坐标：", tagetTranform.position);
        tagetTranform.localPosition = EditorGUILayout.Vector3Field("相对坐标：", tagetTranform.localPosition);
        m_Foldout = Foldout(m_Foldout, "显示自动生成代码菜单");
        if (m_Foldout)
        {
            Component[] allRectransform = Selection.activeGameObject.GetComponentsInChildren<RectTransform>();

            EditorGUILayout.LabelField("全部组件", GUILayout.Width(110));
            for (int j = 0; j < allRectransform.Length; j++)
            {
                Component[] mComponents = allRectransform[j].GetComponents<Component>();

                EditorGUILayout.LabelField($"{allRectransform[j].name}的组件:", GUILayout.Width(100));
                using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
                {
                    GUI.backgroundColor = Color.white;
                    Rect rect = hScope.rect;
                    for (int i = 0; i < mComponents.Length; i++)
                    {
                        Component tCom = mComponents[i];
                        GenerateCode(tCom);
                    }
                }
            }

            GUILayout.Space(30);

            if (GUILayout.Button("将UI代码添加到代码中"))
            {
                Debug.LogError("还没实现");
            }

            if (GUILayout.Button("生成脚本"))
            {
                CreateCsUIScript(CreateTemplateString);
            }
        }
    }

    /// <summary>
    /// 创建脚本
    /// </summary>
    public static void BuildUIScript()
    {
        //获取选中的prefab
        GameObject[] selectobjs = Selection.gameObjects;

        foreach (GameObject go in selectobjs)
        {

            string classStr = "";

            ////如果已经存在了脚本，则只替换//auto下方的字符串
            ////方便刷新
            //if (File.Exists(scriptPath))
            //{
            //    FileStream classfile = new FileStream(scriptPath, FileMode.Open);
            //    StreamReader read = new StreamReader(classfile);
            //    classStr = read.ReadToEnd();
            //    read.Close();
            //    classfile.Close();
            //    File.Delete(scriptPath);

            //    //分割的位置
            //    string splitStr = "//auto";
            //    //auto 上面的部分
            //    string unchangeStr = Regex.Split(classStr, splitStr, RegexOptions.IgnoreCase)[0];
            //    //auto 下面的部分
            //    string changeStr = Regex.Split(TemplateString, splitStr, RegexOptions.IgnoreCase)[1];

            //    StringBuilder build = new StringBuilder();
            //    build.Append(unchangeStr);
            //    build.Append(splitStr);
            //    build.Append(changeStr);
            //    classStr = build.ToString();
            //}
            //else
            //{
            //    classStr = TemplateString;
            //}

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }

    /// <summary>
    /// 根据类型创建按钮
    /// </summary>
    /// <param name="tCom"></param>
    void GenerateCode(Component tCom)
    {
        if (tCom == null) return;
        string gamobjectName = tCom.gameObject.name;//挂组件的物体名
        string componentName = tCom.GetType().Name;//组件名
        if (componentName.Equals("CanvasRenderer")) return;//排除
        //选中颜色样式
        GUIStyle styleHas = new GUIStyle(GUI.skin.button);
        styleHas.normal.textColor = Color.red;
        styleHas.hover.textColor = Color.red;

        #region 路径
        string resultPath = "";

        Transform tempNode = tCom.gameObject.transform;
        string nodePath = "/" + tempNode.name;

        //遍历到顶，求得路径
        while (tempNode != Selection.activeGameObject.transform)
        {
            //取得上级
            tempNode = tempNode.parent;
            //求出/在哪
            int index = nodePath.IndexOf('/');
            //把得到的路径插入
            nodePath = nodePath.Insert(index, "/" + tempNode.name);
        }

        //去掉开头斜杠
        if (nodePath.StartsWith("/"))
        {
            nodePath = nodePath.Remove(0, 1);
        }

        //最终路径
        resultPath = nodePath;
        #endregion

        string attributeName = $"{gamobjectName}_{componentName}";//要生成的属性名

        string codeStr = "";
        codeStr += $"[Autohook(AutohookAttribute.HookType.Component, \"{resultPath}\", useDefault = false)]\r\n\t";
        //codeStr += "[]\r\n\t";
        codeStr += "public " + componentName + " " + attributeName + " = default;\r\n\t";

        AttributeInfo tempInfo = new AttributeInfo(resultPath, componentName, attributeName, codeStr);

        if (attributeList.Contains(tempInfo) == false)
        {
            if (GUILayout.Button(componentName))
            {
                attributeList.Add(tempInfo);
            }
        }
        else if (GUILayout.Button(componentName, styleHas))
        {
            attributeList.Remove(tempInfo);
        }
    }

    /// <summary>
    /// 生成C#UI脚本
    /// </summary>
    private void CreateCsUIScript(string pStr)
    {
        //获得保存路径 EditorPrefs==PlayerPreds
        string path = EditorPrefs.GetString("createScriptFolder", "");

        path = EditorUtility.SaveFilePanel("CreateScript", path, Selection.activeGameObject.name + ".cs", "cs");
        if (string.IsNullOrEmpty(path)) return;

        //文件名
        string fileName = Path.GetFileNameWithoutExtension(path);

        //处理模板
        pStr = pStr.Replace("#类名#", fileName);
        //检查有无重名的
        Dictionary<string, List<string>> tempattributeNameDic = new Dictionary<string, List<string>>();

        for (int i = 0; i < attributeList.Count; i++)
        {
            AttributeInfo info = attributeList[i];

            //如果有同名的
            if (tempattributeNameDic.ContainsKey(info.attributeName))
            {
                int length = tempattributeNameDic[info.attributeName].Count;
                tempattributeNameDic[info.attributeName].Add($"{info.attributeName}_{length}");
                //替换名称
                info.codeStr = info.codeStr.Replace(info.attributeName, $"{info.attributeName}_{length}");
            }
            else
            {
                tempattributeNameDic.Add(info.attributeName, new List<string>() { info.attributeName });
            }

            strBuidler.Append(info.codeStr);

            if (info.componentName == "Button")
            {
                string addStr = $"{info.attributeName}.onClick.AddListener(OnClick_{info.attributeName});";
                strBuidler2.Append(addStr);

                string funStr = $"public void OnClick_{info.attributeName}()\n    {{\n\n    }}";
                strBuidler3.Append(funStr);
            }
        }

        pStr = pStr.Replace("#成员#", $"{strBuidler.ToString()}");
        pStr = pStr.Replace("#初始化方法#", $"{strBuidler2.ToString()}");
        pStr = pStr.Replace("#方法#", $"{strBuidler3.ToString()}");

        //写入
        File.WriteAllText(path, pStr, new UTF8Encoding(false));
        //将这次的路径存储下来
        EditorPrefs.SetString("createScriptFolder", path);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 收折菜单
    /// </summary>
    static bool Foldout(bool display, string title)
    {
        GUIStyle style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.boldLabel).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = 23;
        style.contentOffset = new Vector2(20f, -3f);

        Rect rect = GUILayoutUtility.GetRect(16f, 22f, style);
        GUI.Box(rect, title, style);

        Event e = Event.current;
        Rect toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }
}

/// <summary>
/// 属性信息
/// </summary>
public struct AttributeInfo
{
    /// <summary>
    /// 路径
    /// </summary>
    public string path;
    /// <summary>
    /// 组件名
    /// </summary>
    public string componentName;
    /// <summary>
    /// 属性名称
    /// </summary>
    public string attributeName;
    /// <summary>
    /// 属性代码字符串
    /// </summary>
    public string codeStr;

    public AttributeInfo(string _path, string _componentName, string _attributeName, string _codeStr)
    {
        path = _path;
        componentName = _componentName;
        attributeName = _attributeName;
        codeStr = _codeStr;
    }

    public override bool Equals(object obj)
    {
        if (obj is AttributeInfo)
        {
            return GetHashCode() == ((AttributeInfo)obj).GetHashCode();
        }

        return false;
    }

    public override int GetHashCode()
    {
        return path.GetHashCode() + componentName.GetHashCode() + attributeName.GetHashCode() + codeStr.GetHashCode();
    }

}