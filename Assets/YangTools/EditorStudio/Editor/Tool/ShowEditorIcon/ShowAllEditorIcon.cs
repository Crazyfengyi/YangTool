#if UNITY_EDITOR
/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-07 
*/
using Sirenix.Utilities.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    public class ShowAllEditorIcon : EditorWindow
    {
        private static string[] Text;

        [MenuItem(SettingInfo.YongToolsFunctionPath + "显示Unity编辑器资源")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ShowAllEditorIcon));
            var fullPath = Path.GetFullPath(SettingInfo.YangToolAssetPath + "ShowEditorIcon/AllIcon.txt");
            var str = File.ReadAllText(fullPath);
            Text = str.Split("\n"[0]);
        }
        public Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            //鼠标放在按钮上的样式
            foreach (MouseCursor item in Enum.GetValues(typeof(MouseCursor)))
            {
                GUILayout.Button(Enum.GetName(typeof(MouseCursor), item));
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), item);
                GUILayout.Space(10);
            }
            GUILayout.Label("点击复制查找代码，用EditorGUIUtility.FindTexture()查找");

            //内置图标
            for (int i = 0; i < Text.Length; i += 8)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 8; j++)
                {
                    int index = i + j;
                    if (index < Text.Length)
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent(Text[index].Trim()), GUILayout.Width(50), GUILayout.Height(30)))
                        {
                            string strName = Text[index].Trim();
                            Clipboard.Copy<string>($"EditorGUIUtility.FindTexture(\"{strName}\");", CopyModes.ShallowCopy);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
}
#endif
