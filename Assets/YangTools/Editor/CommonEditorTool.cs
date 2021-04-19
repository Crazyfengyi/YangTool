/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-16 
*/
using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// 编辑器通用方法
/// </summary>
public static class CommonEditorTool
{
    /// <summary>
    /// 收折菜单
    /// </summary>
    public static bool Foldout(bool display, string title)
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

    /// <summary>
    /// 获得相对路径--会去掉名字开头#
    /// </summary>
    public static string GetPath(GameObject gameObject)
    {
        string resultPath = "";

        Transform tempNode = gameObject.transform;
        //去掉开头#
        var tempName = tempNode.name.StartsWith("#") ? tempNode.name.Substring(1, tempNode.name.Length - 1) : tempNode.name;
        string nodePath = "/" + tempName;

        //遍历到顶，求得路径
        while (tempNode != Selection.activeGameObject.transform)
        {
            //取得上级
            tempNode = tempNode.parent;
            //求出/在哪
            int index = nodePath.IndexOf('/');
            //去掉开头#
            var tempName2 = RemoveMark(tempNode.name);

            //把得到的路径插入
            nodePath = nodePath.Insert(index, "/" + tempName2);
        }

        //去掉开头斜杠
        if (nodePath.StartsWith("/"))
        {
            nodePath = nodePath.Remove(0, 1);
        }

        //最终路径
        resultPath = nodePath;

        return resultPath;
    }

    /// <summary>
    /// 去掉开头#
    /// </summary>
    /// <returns>如果有的话去掉，没有就返回原来</returns>
    public static string RemoveMark(string str)
    {
        return str.StartsWith("#") ? str.Substring(1, str.Length - 1) : str;
    }

    /// <summary>
    /// 首字母小写
    /// </summary>
    public static string Lowercase(string str)
    {
        str = str.Replace(str[0], str[0].ToString().ToLower().ToCharArray()[0]);

        return str;
    }
}