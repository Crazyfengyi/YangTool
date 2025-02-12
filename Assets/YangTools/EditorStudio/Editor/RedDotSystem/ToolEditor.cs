#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using YangTools;

namespace GameMain.Editor
{
    public static class ToolEditor
    {
        // [MenuItem("工具/生成配表-微信(bytes)")]
        // public static void ExcelToUnit_Wx()
        // {
        //     ExecuteBat("gen_byte_wx.bat", $"{Application.dataPath}/../../../excels/DataTables/");
        // }
        //
        // [MenuItem("工具/生成配表-抖音(bytes)")]
        // public static void ExcelToUnity_Dy()
        // {
        //     ExecuteBat("gen_byte_dy.bat", $"{Application.dataPath}/../../../excels/DataTables/");
        // }

        // private static void ExecuteBat(string batFile, string workDir)
        // {
        //     string path = FormatPath(workDir + batFile);
        //
        //     if (System.IO.File.Exists(path) == false)
        //     {
        //         Debug.LogError($"不存在的bat文件:{path}");
        //         return;
        //     }
        //
        //     try
        //     {
        //         System.Diagnostics.Process process = null;
        //         
        //         process = new System.Diagnostics.Process();
        //
        //         process.StartInfo.WorkingDirectory = workDir;
        //
        //         process.StartInfo.FileName = batFile;
        //
        //         process.Start();
        //
        //         process.WaitForExit();
        //
        //         process.Close();
        //
        //     }
        //     catch (System.Exception ex)
        //     {
        //         Debug.LogError($"ex:{ex.ToString()}");
        //     }
        // }

        // private static string FormatPath(string path)
        // {
        //     path = path.Replace("//", "\\");
        //
        //     if (Application.platform == RuntimePlatform.OSXEditor)
        //     {
        //         path = path.Replace('\\', '/');
        //     }
        //
        //     return path;
        // }

        [MenuItem(SettingInfo.YongToolsGameToolPath + "打开DataPath")]
        public static void OpenDataPath()
        {
            Application.OpenURL(Application.dataPath);
        }

        [MenuItem(SettingInfo.YongToolsGameToolPath + "打开PersistentDataPath")]
        public static void OpenPersistentDataPath()
        {
            Application.OpenURL(Application.persistentDataPath);
        }
        
        // [MenuItem("工具/打开Excel目录")]
        // public static void OpenExcelFolder()
        // {
        //     Application.OpenURL($"{Application.dataPath}/../../../excels/DataTables/Datas");
        // }
        //
        // [MenuItem("工具/打开美术目录")]
        // public static void OpenArtsFolder()
        // {
        //     Application.OpenURL($"{Application.dataPath}/../../../美术");
        // }

        // [MenuItem("工具/打开策划目录")]
        // public static void OpenPlanFolder()
        // {
        //     Application.OpenURL($"{Application.dataPath}/../../../策划文档");
        // }
        
        // [MenuItem("工具/设置微信权限信息", priority = 100001)]
        // public static void SetWeiXin()
        // {
        //     string path = Path.GetFullPath("..\\", Application.dataPath);
        //     string targetPath = Path.Combine(path, "WeiXinBuild\\minigame\\game.json");
        //
        //     StreamReader streamReader = new StreamReader(targetPath);
        //     string jsonStr = streamReader.ReadToEnd();
        //         streamReader.Close();
        //     if (!jsonStr.Contains("获取昵称和头像用于展示"))
        //     {
        //         jsonStr = jsonStr.Replace("\"desktop\"",
        //             "\"desktop\",\n  \"permission\":{\n\"scope.userFuzzyLocation\":{\"desc\":\"你的位置信息将用于小游戏位置接口的效果展示\"},\n\"scope.userInfo\":{\"desc\":\"获取昵称和头像用于展示\"}\n}");
        //     }
        //     else
        //     {
        //         Debug.LogError($"已经设置过了微信权限");
        //     }
        //
        //     StreamWriter streamWriter = new StreamWriter(targetPath);
        //     streamWriter.Write(jsonStr);
        //     streamWriter.Close();
        //
        //     Debug.LogError($"设置微信权限信息完成:{targetPath}");
        // }

        // [MenuItem("工具/移除缺失脚本", priority = 100002)]
        // public static void RemoveMissScript()
        // {
        //     if (Selection.gameObjects == null)
        //     {
        //         Debug.LogError("请选择对象");
        //         return;
        //     }
        //     
        //     m_goCount = 0;
        //     m_missingCount = 0;
        //     foreach (var go in Selection.gameObjects)
        //     {
        //         search(go);
        //     }
        //     Debug.LogError($"移除缺失脚本完成:{m_goCount}个对象，移除{m_missingCount}个缺失脚本");
        // }
        
        // static int m_goCount;
        // static int m_missingCount;
        // static void search(GameObject go)
        // {
        //     m_goCount++;
        //     m_missingCount += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        //     foreach (Transform child in go.transform)
        //         search(child.gameObject);
        // }
        
//         public class PostBuild : AssetPostprocessor
//         {
//             [PostProcessBuild(100)] //确保这个方法在打包后执行
//             public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
//             {
// #if UNITY_WECHAT_GAME
//                 //Debug.LogError($"设置微信权限信息");
//                 //SetWeiXin();
// #endif
//                 Debug.LogError($"打包结束");
//             }
//         }
    }
}
#endif