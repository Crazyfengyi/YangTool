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
    [CustomEditor(typeof(RectTransform), true)]//bool �����Ƿ����
    public class CreateScript : Editor
    {
        public static string AddTemplateString =
            @"
#region �Զ����ɴ���
partial class #����#
{
    #region �ֶ�
    #�滻�ֶ�#
    #endregion

    #region ��������
    //void Start()
    //{
    //    InitBtnLister();
    //}    
    #endregion

    #region ����
    public void InitBtnLister()
    {
        #��ʼ������#
    }
    #�滻����#
    #endregion
}
#endregion";

        public static string CreateTemplateString =
            @"
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class #����# : MonoBehaviour
{
    #��Ա#

    public void Start()
    {
        #��ʼ������#
    }
    #����#
}";

        private Editor editor;//RectTransformEditor
        Transform tagetTranform;//����
        static bool m_Foldout;//���۲˵�
        static List<AttributeInfo> attributeList = new List<AttributeInfo>();//Ҫ����������
        /// <summary>
        /// ƴ���ֶ���
        /// </summary>
        StringBuilder strBuidlerForFeild = new StringBuilder();
        /// <summary>
        /// ƴ�Ӱ�ť�󶨷�����
        /// </summary>
        StringBuilder strBuidlerForBtnLister = new StringBuilder();
        /// <summary>
        /// ƴ�Ӱ�ť����
        /// </summary>
        StringBuilder strBuidlerForBtnFun = new StringBuilder();

        private void OnEnable()
        {
            editor = Editor.CreateEditor(target, Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.RectTransformEditor", true));
            tagetTranform = target as RectTransform;
            Init();
        }

        /// <summary>
        /// ��ʼ��
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
            tagetTranform.position = EditorGUILayout.Vector3Field("�������꣺", tagetTranform.position);
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("������꣺", tagetTranform.localPosition);
            GUI.enabled = true;

            m_Foldout = CommonEditorTool.Foldout(m_Foldout, "��ʾ�Զ����ɴ���˵�");
            if (m_Foldout)
            {
                Component[] allRectransform = Selection.activeGameObject.GetComponentsInChildren<RectTransform>();

                EditorGUILayout.LabelField("ȫ�����", GUILayout.Width(110));
                for (int j = 0; j < allRectransform.Length; j++)
                {
                    Component[] mComponents = allRectransform[j].GetComponents<Component>();

                    EditorGUILayout.LabelField($"{allRectransform[j].name}�����:", GUILayout.Width(100));
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

                if (GUILayout.Button("��UI������ӵ�������"))
                {
                    AddUIScript();
                }

                if (GUILayout.Button("���ɽű�"))
                {
                    CreateCsUIScript(CreateTemplateString);
                }
            }
        }

        /// <summary>
        /// �������ʹ�����ť
        /// </summary>
        /// <param name="tCom"></param>
        void GenerateCode(Component tCom)
        {
            if (tCom == null) return;
            string gamobjectName = tCom.gameObject.name;//�������������
            string componentName = tCom.GetType().Name;//�����
            if (componentName.Equals("CanvasRenderer")) return;//�ų�
                                                               //ѡ����ɫ��ʽ
            GUIStyle styleHas = new GUIStyle(GUI.skin.button);
            styleHas.normal.textColor = Color.red;
            styleHas.hover.textColor = Color.red;
            //·��
            string resultPath = CommonEditorTool.GetPath(tCom.gameObject);

            //ȥ����ͷ#
            var tempGamobjectName = CommonEditorTool.RemoveMark(gamobjectName);
            var tempComponetName = CommonEditorTool.RemoveMark(componentName);

            string attributeName = $"{CommonEditorTool.Lowercase(tempGamobjectName)}_{CommonEditorTool.Lowercase(tempComponetName)}";//Ҫ���ɵ�������

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
        /// �����нű���Ӵ���
        /// </summary>
        public void AddUIScript()
        {
            string path = EditorPrefs.GetString("selectScriptFolder", "");

            path = EditorUtility.OpenFilePanel("SelectScript", path, "cs");

            if (String.IsNullOrEmpty(path)) return;

            if (File.Exists(path))
            {
                string classStr = File.ReadAllText(path);

                //����
                int index1 = classStr.IndexOf("class");
                int index2 = classStr.IndexOf(": MonoBehaviour");
                string className = classStr.Substring(index1, index2 - index1);
                className = className.Replace("class", "");
                className = className.Trim();

                //��ӹ��Զ����ɴ���
                if (classStr.Contains("#region �Զ����ɴ���"))
                {
                    classStr = classStr.Split(new string[] { "#region �Զ����ɴ���" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    classStr = classStr.TrimEnd();

                    //ģ��
                    string templateString = AddTemplateString;
                    //��ԭ���Ƿ����start
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

                    classStr = classStr.Replace("#����#", $"{className}");
                    classStr = classStr.Replace("#�滻�ֶ�#", $"{result.field}");
                    classStr = classStr.Replace("#��ʼ������#", $"{result.initStr}");
                    classStr = classStr.Replace("#�滻����#", $"{result.funStr}");

                    File.WriteAllText(path, classStr);
                }
                else
                {
                    classStr = classStr.TrimEnd();

                    //��ԭ���Ƿ����partial
                    if (!classStr.Substring(0, index1).Contains("partial"))
                    {
                        classStr = classStr.Replace("class", "partial class");
                    }

                    if (!classStr.Contains("using UnityEngine.UI;"))
                    {
                        classStr = classStr.Insert(0, "using UnityEngine.UI;\r\n");
                    }

                    (string field, string initStr, string funStr) result = CreateCodeString();

                    //ģ��
                    string templateString = AddTemplateString;
                    //��ԭ���Ƿ����start
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

                    classStr = classStr.Replace("#����#", $"{className}");
                    classStr = classStr.Replace("#�滻�ֶ�#", $"{result.field}");
                    classStr = classStr.Replace("#��ʼ������#", $"{result.initStr}");
                    classStr = classStr.Replace("#�滻����#", $"{result.funStr}");

                    File.WriteAllText(path, classStr);
                }
            }

            EditorPrefs.SetString("selectScriptFolder", path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// ����C#UI�ű�
        /// </summary>
        private void CreateCsUIScript(string pStr)
        {
            //��ñ���·�� EditorPrefs==PlayerPreds
            string path = EditorPrefs.GetString("createScriptFolder", "");

            path = EditorUtility.SaveFilePanel("CreateScript", path, CommonEditorTool.RemoveMark(Selection.activeGameObject.name) + ".cs", "cs");
            if (string.IsNullOrEmpty(path)) return;

            //�ļ���
            string fileName = Path.GetFileNameWithoutExtension(path);

            //����ģ��
            pStr = pStr.Replace("#����#", fileName);

            (string field, string initStr, string funStr) result = CreateCodeString();

            pStr = pStr.Replace("#��Ա#", $"{result.field}");
            pStr = pStr.Replace("#��ʼ������#", $"{result.initStr}");
            pStr = pStr.Replace("#����#", $"{result.funStr}");

            //д��
            File.WriteAllText(path, pStr, new UTF8Encoding(false));
            //����ε�·���洢����
            EditorPrefs.SetString("createScriptFolder", path);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// ���ɴ���string
        /// </summary>
        private (string field, string initStr, string funStr) CreateCodeString()
        {
            strBuidlerForFeild.Clear();
            strBuidlerForBtnLister.Clear();
            strBuidlerForBtnFun.Clear();

            //�������������
            Dictionary<string, List<string>> tempattributeNameDic = new Dictionary<string, List<string>>();

            for (int i = 0; i < attributeList.Count; i++)
            {
                AttributeInfo info = attributeList[i];

                //�����ͬ����
                if (tempattributeNameDic.ContainsKey(info.attributeName))
                {
                    int length = tempattributeNameDic[info.attributeName].Count;
                    tempattributeNameDic[info.attributeName].Add($"{info.attributeName}_{length}");
                    //�滻����
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
        /// ���������ּ�#
        /// </summary>
        public void SetHierarchyNameAdd()
        {
            GameObject gameObject = Selection.activeGameObject;
            if (gameObject == null)
            {
                Debuger.ToError("δѡ������");
                return;
            }

            RectTransform[] allRect = gameObject.GetComponentsInChildren<RectTransform>();

            //ȥ�����п�ͷ#
            for (int i = 0; i < allRect.Length; i++)
            {
                if (allRect[i].name.StartsWith("#"))
                {
                    allRect[i].name = CommonEditorTool.RemoveMark(allRect[i].name);
                }
            }

            //����Ҫ���ɵ��ֶ��������#��ͷ
            for (int i = 0; i < attributeList.Count; i++)
            {
                AttributeInfo item = attributeList[i];
                if (!item.gameObject.name.StartsWith("#"))
                {
                    item.gameObject.name = "#" + item.gameObject.name;
                }
            }

            //TODO ��ȱ��--����ű�ʹ���Զ�����ʱ��#��ͷ�����һ������Ϊ׼
        }

        [MenuItem("YangTools/��������/�Ƴ�ѡ�����������е�#��ͷ")]
        public static void RemoveAllMark()
        {
            GameObject gameObject = Selection.activeGameObject;
            if (gameObject == null)
            {
                Debuger.ToError("δѡ������");
                return;
            }

            RectTransform[] allRect = gameObject.GetComponentsInChildren<RectTransform>(true);

            //ȥ�����п�ͷ#
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
    /// ������Ϣ
    /// </summary>
    public struct AttributeInfo
    {
        /// <summary>
        /// ����
        /// </summary>
        public GameObject gameObject;
        /// <summary>
        /// ·��
        /// </summary>
        public string path;
        /// <summary>
        /// �����
        /// </summary>
        public string componentName;
        /// <summary>
        /// ��������
        /// </summary>
        public string attributeName;
        /// <summary>
        /// ���Դ����ַ���
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