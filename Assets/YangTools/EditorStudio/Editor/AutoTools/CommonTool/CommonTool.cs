#if UNITY_EDITOR
/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       WIN-VJ19D9AB7HB
 *UnityVersion：2022.3.0f1c1
 *创建时间:         2023-09-12
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.SceneManagement;

public class CommonTool : MonoBehaviour
{
    [MenuItem(SettingInfo.YongToolsGameToolPath + "打开存档目录")]
    public static void OpenGameSavePath()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
        Debug.Log("打开存档目录！");
    }

    [MenuItem(SettingInfo.YongToolsGameToolPath + "删除游戏存档")]
    public static void DeleteGameSave()
    {
        //string path = Path.Combine(Application.persistentDataPath, "saveData.uch");
        //if (File.Exists("path"))
        //{
        //    File.Delete(path);
        //    Debug.Log($"删除存档完成！{path}");
        //}
    }

    [MenuItem(SettingInfo.YongToolsGameToolPath + "启动游戏")]
    private static void StartGame()
    {
        string entryScene = "Assets/Scenes/GameEntry.unity";
        EditorSceneManager.OpenScene(entryScene, OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    [MenuItem(SettingInfo.YongToolsGameToolPath + "添加宏[LOCAL_MODE]")]
    public static void AddDefine_LOCAL_MODE()
    {
        AddDefines(";LOCAL_MODE");
        Debug.Log("[宏定义]=>添加 [LOCAL_MODE]");
    }

    [MenuItem(SettingInfo.YongToolsGameToolPath + "移除宏[LOCAL_MODE]")]
    public static void RemoveDefine_LOCAL_MODE()
    {
        RemoveDefines(";LOCAL_MODE");
        Debug.Log("[宏定义]=>移除 [LOCAL_MODE]");
    }

    /// <summary>
    /// 添加宏定义
    /// </summary>
    private static void AddDefines(params string[] defines)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        string s = PlayerSettings.GetScriptingDefineSymbols(target);
        foreach (string define in defines)
        {
            if (!s.Contains(define)) s += define;
        }
        PlayerSettings.SetScriptingDefineSymbols(target, s);
    }

    /// <summary>
    /// 移除宏定义
    /// </summary>
    private static void RemoveDefines(params string[] defines)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        var s = PlayerSettings.GetScriptingDefineSymbols(target);
        foreach (string define in defines)
        {
            s = s.Replace(define, "");
        }
        PlayerSettings.SetScriptingDefineSymbols(target, s);
    }
}
#endif