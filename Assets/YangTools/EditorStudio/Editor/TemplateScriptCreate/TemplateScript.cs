/** 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION# 
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-07-27 
*/
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;

namespace YangTools
{
    public class TemplateScript
    {
        public static readonly string MY_SCRIPT_DEFAULT = SettingInfo.YangToolAssetPath + "TemplateScriptCreate/81-C# Script-NewBehaviourScript.cs.txt";

        [MenuItem("Assets/Create/C# TemplateScript", false, 80)]
        public static void CreateMyScript()
        {
            string locationPath = GetSelectPathOrFallback();
            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreatScriptableObject>(), locationPath + "/NewBehaviourScript.cs", icon, MY_SCRIPT_DEFAULT);
        }

        public static string GetSelectPathOrFallback()
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
                UnityEngine.Object obj = CreateScriptAssetFormTemplats(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }

            //内部方法--更改模板类容
            internal static Object CreateScriptAssetFormTemplats(string pathName, string resourceFile)
            {
                string newfullPath = Path.GetFullPath(pathName);
                string oldFullPath = Path.GetFullPath(resourceFile);

                string text = File.ReadAllText(oldFullPath);
                //获取没有扩展名的文件名
                string fileName = Path.GetFileNameWithoutExtension(pathName);
                text = Regex.Replace(text, "#SCRIPTNAME#", fileName);
                //unity设置里的公司名
                text = text.Replace("#COMPANY#", PlayerSettings.companyName);
                //电脑主机名称
                string computerName = Dns.GetHostName();
                text = text.Replace("#AUTHOR#", $"{computerName}");
                //unity版本
                text = text.Replace("#UNITYVERSION#", Application.unityVersion);
                //创建时间
                text = text.Replace("#DATE#", System.DateTime.Now.ToString("yyyy-MM-dd"));

                File.WriteAllText(newfullPath, text);

                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }
        }
    }
}