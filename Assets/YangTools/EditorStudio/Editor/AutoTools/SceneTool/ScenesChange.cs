#if UNITY_EDITOR
/* 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *Author:       陈春洋 
 *UnityVersion：2019.4.17f1c1 
 *创建时间:         2021-03-31 
*/
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YangTools.Scripts.Core;

namespace YangTools
{
    public static class ScenesChange
    {
        /// <summary>
        /// 自动生成代码路径
        /// </summary>
        static readonly string ScenesMenuPath = "YangTools/EditorStudio/Editor/AutoTools/SceneTool/AllScenes.cs";
        /// <summary>
        /// 场景计数(暂时未操作计数)
        /// </summary>
        private static int SceneCount = 0;//TODO 设置成可以自己控制开关

        [MenuItem(SettingInfo.MenuPath + "自动写入所有场景到快捷切换列表")]
        public static void UpdateList()
        {
            string scenesMenuPath = Path.Combine(Application.dataPath, ScenesMenuPath);
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("#if UNITY_EDITOR");
            stringBuilder.AppendLine("// Generated file");
            stringBuilder.AppendLine("using UnityEditor;");
            stringBuilder.AppendLine("using UnityEditor.SceneManagement;");
            stringBuilder.AppendLine("using YangTools;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("public static class ScenesMenu");
            stringBuilder.AppendLine("{");

            foreach (var sceneGuid in AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" }))
            {
                var sceneFilename = AssetDatabase.GUIDToAssetPath(sceneGuid);
                var sceneName = Path.GetFileNameWithoutExtension(sceneFilename);
                var methodName = sceneFilename.Replace('/', '_').Replace('\\', '_').Replace('.', '_').Replace('-', '_').Replace(' ', '_').Replace('(', '_').Replace(')', '_');
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"    [MenuItem(\"YangTools/AllScenes/{sceneName}\", priority = 10)]");
                stringBuilder.AppendLine($"    public static void {methodName}() {{ ScenesChange.OpenScene(\"{sceneFilename}\"); }}");
            }
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("#endif");
            Debug.LogError("所有场景自动生成快速切换代码到:" + Path.GetDirectoryName(scenesMenuPath) + "完成");
            Directory.CreateDirectory(Path.GetDirectoryName(scenesMenuPath) ?? throw new InvalidOperationException());
            File.WriteAllText(scenesMenuPath, stringBuilder.ToString());
            AssetDatabase.Refresh();
        }

        [MenuItem(SettingInfo.MenuPath + "移除显示场景外的其他场景")]
        public static void ReomveAllNotActiveScene()
        {
            //当前场景
            UnityEngine.SceneManagement.Scene current = EditorSceneManager.GetActiveScene();

            //场景数量
            int allCount = EditorSceneManager.sceneCount;
            for (int i = 0; i < allCount; i++)
            {
                if (EditorSceneManager.GetSceneAt(i) != current)
                {
                    EditorSceneManager.UnloadSceneAsync(EditorSceneManager.GetSceneAt(i));
                }
            }
        }
        /// <summary>
        /// 编辑器打开场景
        /// </summary>
        public static void OpenScene(string filename)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var oldScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.OpenScene(filename, OpenSceneMode.Additive);
                EditorSceneManager.CloseScene(oldScene, false);

                SceneCount++;
                if (SceneCount >= 5)
                {
                    SceneCount = 0;
                    ReomveAllNotActiveScene();
                }
            }
        }
    }
}
#endif