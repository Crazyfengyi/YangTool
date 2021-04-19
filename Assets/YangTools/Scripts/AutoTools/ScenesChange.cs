/** 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *Author:       陈春洋 
 *UnityVersion：2019.4.17f1c1 
 *创建时间:         2021-03-31 
*/
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace YangTools
{
    public static class ScenesChange
    {
        /// <summary>
        /// 自动生成代码路径
        /// </summary>
        static readonly string ScenesMenuPath = "YangTools/Scripts/AutoTools/AllScenes.cs";
        /// <summary>
        /// 场景计数(暂时未操作计数)
        /// </summary>
        static int secensCount = 0;//TODO 设置成可以自己控制开关

        [MenuItem("YangTools/自动写入所有场景到快捷切换列表")]
        public static void UpdateList()
        {
            string scenesMenuPath = Path.Combine(Application.dataPath, ScenesMenuPath);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("// Generated file");
            stringBuilder.AppendLine("using UnityEditor;");
            stringBuilder.AppendLine("using UnityEditor.SceneManagement;");
            stringBuilder.AppendLine("using YangTools;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("public static class ScenesMenu");
            stringBuilder.AppendLine("{");

            foreach (string sceneGuid in AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" }))
            {
                string sceneFilename = AssetDatabase.GUIDToAssetPath(sceneGuid);
                string sceneName = Path.GetFileNameWithoutExtension(sceneFilename);
                string methodName = sceneFilename.Replace('/', '_').Replace('\\', '_').Replace('.', '_').Replace('-', '_').Replace(' ', '_').Replace('(', '_').Replace(')', '_');
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(string.Format("    [MenuItem(\"YangTools/AllSecnes/{0}\", priority = 10)]", sceneName));
                stringBuilder.AppendLine(string.Format("    public static void {0}() {{ ScenesChange.OpenScene(\"{1}\"); }}", methodName, sceneFilename));
            }
            stringBuilder.AppendLine("}");
            Debug.LogError("所有场景自动生成快速切换代码到：" + Path.GetDirectoryName(scenesMenuPath) + "完成");
            Directory.CreateDirectory(Path.GetDirectoryName(scenesMenuPath));
            File.WriteAllText(scenesMenuPath, stringBuilder.ToString());
            AssetDatabase.Refresh();
        }

        [MenuItem("YangTools/移除显示场景外的其他场景")]
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
        /// <param name="filename"></param>
        public static void OpenScene(string filename)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var oldScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.OpenScene(filename, OpenSceneMode.Additive);
                EditorSceneManager.CloseScene(oldScene, false);

                secensCount++;
                if (secensCount >= 5)
                {
                    secensCount = 0;
                    ReomveAllNotActiveScene();
                }
            }
        }
    }
}