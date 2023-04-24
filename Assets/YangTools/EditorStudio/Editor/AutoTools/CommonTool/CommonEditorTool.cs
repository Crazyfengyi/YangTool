/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-16 
*/
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
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
            style.fixedHeight = 30;
            style.contentOffset = new Vector2(20f, -3f);
            style.fontSize = 12;
            style.richText = true;
            style.fontStyle = FontStyle.Normal;

            Rect rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, $"<color=#00F5FF>{title}</color>", style);

            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                //箭头
                Rect toggleRect = new Rect(rect.x + 4f, rect.y + 6f, 13f, 13f);
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            GUILayout.Space(10);

            //unity默认内容折叠
            //display = EditorGUILayout.Foldout(display, title);

            return display;
        }
        /// <summary>
        /// 通用弹窗提示
        /// </summary>
        public static void CommonTipsPanel(string tips, Action overCallBack = null)
        {
            if (EditorUtility.DisplayDialog("提示", tips, "OK"))
            {
                overCallBack?.Invoke();
            }
        }
        /// <summary>
        /// 清空Log输出
        /// </summary>
        public static void ClearConsole()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            Type type = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
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
        /// <summary>
        /// 保存字符串到Txt
        /// </summary>
        public static void SaveStringToTxt(string fileName, string text)
        {
            string dicPath = Application.dataPath + $"/YangTools/Editor/SaveData";
            string path = dicPath + $"/{fileName}.txt";

            if (!Directory.Exists(dicPath))
            {
                Directory.CreateDirectory(dicPath);
            }

            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            //定义存放文件信息的字节数组
            byte[] tempBytes = new byte[fileStream.Length];
            //读取文件信息
            fileStream.Read(tempBytes, 0, tempBytes.Length);
            //将得到的字节型数组重写编码为字符型数组
            char[] chars = Encoding.UTF8.GetChars(tempBytes);
            //转换成字符串
            string tempStr = new string(chars);
            //=========更改=========

            tempStr += text + "|";

            //======================
            //将字符串转换为字节数组
            byte[] bytes = Encoding.UTF8.GetBytes(tempStr);
            //向文件中写入字节数组
            fileStream.Write(bytes, 0, bytes.Length);
            //刷新缓冲区
            fileStream.Flush();
            //关闭流
            fileStream.Close();

            AssetDatabase.Refresh();
        }
    }

    #region 提示弹窗
    /// <summary>
    /// 提示弹窗
    /// </summary>
    public class TipsShowWindow : EditorWindow
    {
        private Vector2 scroll;
        ///搜索文字
        private static string search;
        //提示文字
        private static string ShowTips;
        //输出文字
        private static string ShowStr;
        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="tips">提示</param>
        public static void OpenWindow(string title, string tips)
        {
            search = "";
            ShowTips = tips;
            ShowStr = EditorAutoTools.consleString.GetString();
            GetWindowWithRect<TipsShowWindow>(new Rect(500, 500, 300, 300), true, title, true);
        }
        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("搜索框");

            //检查代码块中是否有任何控件被更改
            //EditorGUI.BeginChangeCheck();
            search = GUILayout.TextField(search);
            //EditorGUI.EndChangeCheck();

            GUILayout.Space(20);

            var temp = new GUIStyle();
            temp.richText = true;

            GUILayout.Label(ShowTips, temp);
            GUILayout.Space(20);

            scroll = GUILayout.BeginScrollView(scroll);
            string[] charArray = ShowStr.Split('\n', '\r');
            for (int i = 0; i < charArray.Length; i++)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    if (!charArray[i].Contains(search)) continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(charArray[i]);
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (GUILayout.Button("关闭"))
            {
                Close();
            }
        }
    }
    #endregion

    #region 字符串记录
    /// <summary>
    /// 面板输出字符串记录
    /// </summary>
    public class ConsleString
    {
        StringBuilder value = new StringBuilder();
        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            value.Clear();
        }
        /// <summary>
        /// 添加-自动换行
        /// </summary>
        public void Add(string str)
        {
            value.Append(str + "\n");
        }
        /// <summary>
        /// 检测是否有值
        /// </summary>
        /// <returns></returns>
        public bool CheackHaveValue()
        {
            return value.Length > 0;
        }
        /// <summary>
        /// 获得字符串
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return value.ToString();
        }
        /// <summary>
        /// 输出到DeBug面板
        /// </summary>
        public void OutPutToDeBug()
        {
            Debug.LogError(value.ToString());
        }
    }
    #endregion
}