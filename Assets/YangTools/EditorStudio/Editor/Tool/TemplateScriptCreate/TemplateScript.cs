#if UNITY_EDITOR
/*
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION# 
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-07-27 
*/
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace YangTools
{
    public static class TemplateScript
    {
        private const string MyScriptDefault = SettingInfo.YangToolAssetPath + "TemplateScriptCreate/81-C# Script-NewBehaviourScript.cs.txt";

        [MenuItem("Assets/Create/C# TemplateScript", false, 80)]
        public static void CreateMyScript()
        {
            var locationPath = GetSelectPathOrFallback();
            var icon = EditorGUIUtility.FindTexture("cs Script Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreatScriptableObject>(), locationPath + "/NewBehaviourScript.cs", icon, MyScriptDefault);
        }
        
        private static string GetSelectPathOrFallback()
        {
            string path = "Assets";
            foreach (Object item in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// 创建代码完成后的ScriptableObject回调
        /// </summary>
        private class CreatScriptableObject : EndNameEditAction
        {
            //当接受用户一个编辑的名字时，Unity会调用这个函数
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                //pathName:Assets/YangTools/Editor/AutoScriptCreate/we.cs
                //resourceFile:Assets/YangTools/Editor/AutoScriptCreate/81-C# Script-NewBehaviourScript.cs.txt
                var obj = CreateScriptAssetFormTemplate(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }

            //更改模板内容
            private static Object CreateScriptAssetFormTemplate(string pathName, string resourceFile)
            {
                string newFullPath = Path.GetFullPath(pathName);
                string oldFullPath = Path.GetFullPath(resourceFile);

                string text = File.ReadAllText(oldFullPath);
                //获取没有扩展名的文件名
                string fileName = Path.GetFileNameWithoutExtension(pathName);
                text = Regex.Replace(text, "#SCRIPTNAME#", fileName);
                text = text.Replace("#COMPANY#", PlayerSettings.companyName);//unity设置里的公司名
                var computerName = Dns.GetHostName();
                text = text.Replace("#AUTHOR#", $"{computerName}");//电脑主机名称
                text = text.Replace("#UNITYVERSION#", Application.unityVersion); //unity版本
                text = text.Replace("#DATE#", System.DateTime.Now.ToString("yyyy-MM-dd"));//创建时间
                File.WriteAllText(newFullPath, text);

                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }
        }
    }
}
#endif