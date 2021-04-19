using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//TODO ��������õ����� ͨ������ǰ��#
[ExecuteInEditMode]
[CustomEditor(typeof(RectTransform), true)]//bool �����Ƿ����
public class CreateScript : Editor
{
    public static string AddTemplateString =
        @"partial class #����#
        {
            #region �ֶ�
            #�滻�ֶ�#
            #endregion

            #region ����
            #�滻������#
            #endregion
        }";

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
    //ƴ���ַ�����stringBuilder
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
    /// ��ʼ��
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
        tagetTranform.position = EditorGUILayout.Vector3Field("�������꣺", tagetTranform.position);
        tagetTranform.localPosition = EditorGUILayout.Vector3Field("������꣺", tagetTranform.localPosition);
        m_Foldout = Foldout(m_Foldout, "��ʾ�Զ����ɴ���˵�");
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
                Debug.LogError("��ûʵ��");
            }

            if (GUILayout.Button("���ɽű�"))
            {
                CreateCsUIScript(CreateTemplateString);
            }
        }
    }

    /// <summary>
    /// �����ű�
    /// </summary>
    public static void BuildUIScript()
    {
        //��ȡѡ�е�prefab
        GameObject[] selectobjs = Selection.gameObjects;

        foreach (GameObject go in selectobjs)
        {

            string classStr = "";

            ////����Ѿ������˽ű�����ֻ�滻//auto�·����ַ���
            ////����ˢ��
            //if (File.Exists(scriptPath))
            //{
            //    FileStream classfile = new FileStream(scriptPath, FileMode.Open);
            //    StreamReader read = new StreamReader(classfile);
            //    classStr = read.ReadToEnd();
            //    read.Close();
            //    classfile.Close();
            //    File.Delete(scriptPath);

            //    //�ָ��λ��
            //    string splitStr = "//auto";
            //    //auto ����Ĳ���
            //    string unchangeStr = Regex.Split(classStr, splitStr, RegexOptions.IgnoreCase)[0];
            //    //auto ����Ĳ���
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

        #region ·��
        string resultPath = "";

        Transform tempNode = tCom.gameObject.transform;
        string nodePath = "/" + tempNode.name;

        //�������������·��
        while (tempNode != Selection.activeGameObject.transform)
        {
            //ȡ���ϼ�
            tempNode = tempNode.parent;
            //���/����
            int index = nodePath.IndexOf('/');
            //�ѵõ���·������
            nodePath = nodePath.Insert(index, "/" + tempNode.name);
        }

        //ȥ����ͷб��
        if (nodePath.StartsWith("/"))
        {
            nodePath = nodePath.Remove(0, 1);
        }

        //����·��
        resultPath = nodePath;
        #endregion

        string attributeName = $"{gamobjectName}_{componentName}";//Ҫ���ɵ�������

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
    /// ����C#UI�ű�
    /// </summary>
    private void CreateCsUIScript(string pStr)
    {
        //��ñ���·�� EditorPrefs==PlayerPreds
        string path = EditorPrefs.GetString("createScriptFolder", "");

        path = EditorUtility.SaveFilePanel("CreateScript", path, Selection.activeGameObject.name + ".cs", "cs");
        if (string.IsNullOrEmpty(path)) return;

        //�ļ���
        string fileName = Path.GetFileNameWithoutExtension(path);

        //����ģ��
        pStr = pStr.Replace("#����#", fileName);
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

            strBuidler.Append(info.codeStr);

            if (info.componentName == "Button")
            {
                string addStr = $"{info.attributeName}.onClick.AddListener(OnClick_{info.attributeName});";
                strBuidler2.Append(addStr);

                string funStr = $"public void OnClick_{info.attributeName}()\n    {{\n\n    }}";
                strBuidler3.Append(funStr);
            }
        }

        pStr = pStr.Replace("#��Ա#", $"{strBuidler.ToString()}");
        pStr = pStr.Replace("#��ʼ������#", $"{strBuidler2.ToString()}");
        pStr = pStr.Replace("#����#", $"{strBuidler3.ToString()}");

        //д��
        File.WriteAllText(path, pStr, new UTF8Encoding(false));
        //����ε�·���洢����
        EditorPrefs.SetString("createScriptFolder", path);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// ���۲˵�
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
/// ������Ϣ
/// </summary>
public struct AttributeInfo
{
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