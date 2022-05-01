using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using YangTools.Log;

namespace YangTools
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(RectTransform), true)]//bool 子类是否可用
    public class CreateScript : Editor
    {
        public static string AddTemplateString =
            @"
#region 自动生成代码
partial class #类名#
{
    #region 字段
    #替换字段#
    #endregion

    #region 生命周期
    //void Start()
    //{
    //    InitBtnLister();
    //}    
    #endregion

    #region 方法
    public void InitBtnLister()
    {
        #初始化方法#
    }
    #替换方法#
    #endregion
}
#endregion";

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
        /// <summary>
        /// 拼接字段用
        /// </summary>
        StringBuilder strBuidlerForFeild = new StringBuilder();
        /// <summary>
        /// 拼接按钮绑定方法用
        /// </summary>
        StringBuilder strBuidlerForBtnLister = new StringBuilder();
        /// <summary>
        /// 拼接按钮方法
        /// </summary>
        StringBuilder strBuidlerForBtnFun = new StringBuilder();

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
            strBuidlerForFeild.Clear();
            strBuidlerForBtnLister.Clear();
            strBuidlerForBtnFun.Clear();
        }

        public override void OnInspectorGUI()
        {
            editor.OnInspectorGUI();
            tagetTranform.position = EditorGUILayout.Vector3Field("世界坐标：", tagetTranform.position);
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("相对坐标：", tagetTranform.localPosition);
            GUI.enabled = true;

            m_Foldout = CommonEditorTool.Foldout(m_Foldout, "显示自动生成代码菜单");
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
                    AddUIScript();
                }

                if (GUILayout.Button("生成脚本"))
                {
                    CreateCsUIScript(CreateTemplateString);
                }
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
            //路径
            string resultPath = CommonEditorTool.GetPath(tCom.gameObject);

            //去掉开头#
            var tempGamobjectName = CommonEditorTool.RemoveMark(gamobjectName);
            var tempComponetName = CommonEditorTool.RemoveMark(componentName);

            string attributeName = $"{CommonEditorTool.Lowercase(tempGamobjectName)}_{CommonEditorTool.Lowercase(tempComponetName)}";//要生成的属性名

            string codeStr = "";
            codeStr += $"[Autohook(AutohookAttribute.HookType.Component, \"{resultPath}\", useDefault = false)]\r\n\t";
            //codeStr += "[]\r\n\t";
            codeStr += "public " + componentName + " " + attributeName + " = default;\r\n\t";

            AttributeInfo tempInfo = new AttributeInfo(tCom.gameObject, resultPath, componentName, attributeName, codeStr);

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
        /// 往现有脚本添加代码
        /// </summary>
        public void AddUIScript()
        {
            string path = EditorPrefs.GetString("selectScriptFolder", "");

            path = EditorUtility.OpenFilePanel("SelectScript", path, "cs");

            if (String.IsNullOrEmpty(path)) return;

            if (File.Exists(path))
            {
                string classStr = File.ReadAllText(path);

                //类名
                int index1 = classStr.IndexOf("class");
                int index2 = classStr.IndexOf(": MonoBehaviour");
                string className = classStr.Substring(index1, index2 - index1);
                className = className.Replace("class", "");
                className = className.Trim();

                //添加过自动生成代码
                if (classStr.Contains("#region 自动生成代码"))
                {
                    classStr = classStr.Split(new string[] { "#region 自动生成代码" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    classStr = classStr.TrimEnd();

                    //模板
                    string templateString = AddTemplateString;
                    //看原类是否包含start
                    if (classStr.Contains($"void Start()"))
                    {
                        classStr = classStr.Replace("void Start()\r\n    {", "void Start()\r\n    {\r\n        InitBtnLister();\r\n");
                    }
                    else
                    {
                        templateString = templateString.Replace("//", "");
                    }

                    classStr += "\n";
                    classStr += templateString;

                    (string field, string initStr, string funStr) result = CreateCodeString();

                    classStr = classStr.Replace("#类名#", $"{className}");
                    classStr = classStr.Replace("#替换字段#", $"{result.field}");
                    classStr = classStr.Replace("#初始化方法#", $"{result.initStr}");
                    classStr = classStr.Replace("#替换方法#", $"{result.funStr}");

                    File.WriteAllText(path, classStr);
                }
                else
                {
                    classStr = classStr.TrimEnd();

                    //看原类是否包含partial
                    if (!classStr.Substring(0, index1).Contains("partial"))
                    {
                        classStr = classStr.Replace("class", "partial class");
                    }

                    if (!classStr.Contains("using UnityEngine.UI;"))
                    {
                        classStr = classStr.Insert(0, "using UnityEngine.UI;\r\n");
                    }

                    (string field, string initStr, string funStr) result = CreateCodeString();

                    //模板
                    string templateString = AddTemplateString;
                    //看原类是否包含start
                    if (classStr.Contains($"void Start()"))
                    {
                        classStr = classStr.Replace("void Start()\r\n    {", "void Start()\r\n    {\r\n        InitBtnLister();\r\n");
                    }
                    else
                    {
                        templateString = templateString.Replace("//", "");
                    }

                    classStr += "\n";
                    classStr += templateString;

                    classStr = classStr.Replace("#类名#", $"{className}");
                    classStr = classStr.Replace("#替换字段#", $"{result.field}");
                    classStr = classStr.Replace("#初始化方法#", $"{result.initStr}");
                    classStr = classStr.Replace("#替换方法#", $"{result.funStr}");

                    File.WriteAllText(path, classStr);
                }
            }

            EditorPrefs.SetString("selectScriptFolder", path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成C#UI脚本
        /// </summary>
        private void CreateCsUIScript(string pStr)
        {
            //获得保存路径 EditorPrefs==PlayerPreds
            string path = EditorPrefs.GetString("createScriptFolder", "");

            path = EditorUtility.SaveFilePanel("CreateScript", path, CommonEditorTool.RemoveMark(Selection.activeGameObject.name) + ".cs", "cs");
            if (string.IsNullOrEmpty(path)) return;

            //文件名
            string fileName = Path.GetFileNameWithoutExtension(path);

            //处理模板
            pStr = pStr.Replace("#类名#", fileName);

            (string field, string initStr, string funStr) result = CreateCodeString();

            pStr = pStr.Replace("#成员#", $"{result.field}");
            pStr = pStr.Replace("#初始化方法#", $"{result.initStr}");
            pStr = pStr.Replace("#方法#", $"{result.funStr}");

            //写入
            File.WriteAllText(path, pStr, new UTF8Encoding(false));
            //将这次的路径存储下来
            EditorPrefs.SetString("createScriptFolder", path);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成代码string
        /// </summary>
        private (string field, string initStr, string funStr) CreateCodeString()
        {
            strBuidlerForFeild.Clear();
            strBuidlerForBtnLister.Clear();
            strBuidlerForBtnFun.Clear();

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

                strBuidlerForFeild.Append(info.codeStr);

                if (info.componentName == "Button")
                {
                    string addStr = $"{info.attributeName}.onClick.AddListener(OnClick_{info.attributeName});";
                    strBuidlerForBtnLister.Append(addStr);

                    string funStr = $"\r\n    public void OnClick_{info.attributeName}()\n    {{\n\n    }}";
                    strBuidlerForBtnFun.Append(funStr);
                }
            }

            SetHierarchyNameAdd();

            return (strBuidlerForFeild.ToString().TrimEnd(), strBuidlerForBtnLister.ToString(), strBuidlerForBtnFun.ToString());
        }

        /// <summary>
        /// 给物体名字加#
        /// </summary>
        public void SetHierarchyNameAdd()
        {
            GameObject gameObject = Selection.activeGameObject;
            if (gameObject == null)
            {
                Debuger.ToError("未选中物体");
                return;
            }

            RectTransform[] allRect = gameObject.GetComponentsInChildren<RectTransform>();

            //去掉所有开头#
            for (int i = 0; i < allRect.Length; i++)
            {
                if (allRect[i].name.StartsWith("#"))
                {
                    allRect[i].name = CommonEditorTool.RemoveMark(allRect[i].name);
                }
            }

            //根据要生成的字段添加物体#开头
            for (int i = 0; i < attributeList.Count; i++)
            {
                AttributeInfo item = attributeList[i];
                if (!item.gameObject.name.StartsWith("#"))
                {
                    item.gameObject.name = "#" + item.gameObject.name;
                }
            }

            //TODO 有缺点--多个脚本使用自动生成时，#开头以最后一次生成为准
        }

        [MenuItem("YangTools/辅助功能/移除选中物体下所有的#开头")]
        public static void RemoveAllMark()
        {
            GameObject gameObject = Selection.activeGameObject;
            if (gameObject == null)
            {
                Debuger.ToError("未选中物体");
                return;
            }

            RectTransform[] allRect = gameObject.GetComponentsInChildren<RectTransform>(true);

            //去掉所有开头#
            for (int i = 0; i < allRect.Length; i++)
            {
                if (allRect[i].name.StartsWith("#"))
                {
                    allRect[i].name = CommonEditorTool.RemoveMark(allRect[i].name);
                }
            }
        }
    }

    /// <summary>
    /// 属性信息
    /// </summary>
    public struct AttributeInfo
    {
        /// <summary>
        /// 引用
        /// </summary>
        public GameObject gameObject;
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

        public AttributeInfo(GameObject _gameObject, string _path, string _componentName, string _attributeName, string _codeStr)
        {
            gameObject = _gameObject;
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
}