/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-11-16 
*/
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// 打包工具
/// </summary>
public static class BuildTool
{
    /// <summary>
    /// 打包机上的APK根目录
    /// </summary>
    private static string APK_ROOT_PATH = @"D:\SpaceWork\Out_APKs";
    /// <summary>
    /// 计数
    /// </summary>
    private static int APK_FLAG = 0;

    [MenuItem("YangTools/BuildTool/一键输出APK(Android打包)", priority = 9999)]
    public static void BuildAPK()
    {
        //打包设置
        BuildSetting();
        //打包APK
        BuildOutputAPK();
    }
    /// <summary>
    /// 打包设置
    /// </summary>
    private static void BuildSetting()
    {
        //string keyStorePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "key/x.keystore");
        //PlayerSettings.Android.keystoreName = keyStorePath;
        //PlayerSettings.Android.keystorePass = "x";
        //PlayerSettings.Android.keyaliasName = "x";
        //PlayerSettings.Android.keyaliasPass = "x";
    }
    /// <summary>
    /// 开始打包
    /// </summary>
    private static void BuildOutputAPK()
    {
        //简单的判断是否同一天
        string time = PlayerPrefs.GetString("BuildTime_Flag", "0");
        if (int.Parse(time) != DateTime.Now.Day)
        {
            PlayerPrefs.SetString("BuildTime_Flag", DateTime.Now.Day.ToString());
            PlayerPrefs.SetInt("APK_FLAG", 0);
        }
        APK_FLAG = PlayerPrefs.GetInt("APK_FLAG");
        APK_FLAG++;
        PlayerPrefs.SetInt("APK_FLAG", APK_FLAG);

        if (!Directory.Exists(APK_ROOT_PATH))
        {
            Directory.CreateDirectory(APK_ROOT_PATH);
        }

        string apkRootPath = APK_ROOT_PATH;
        string timeStr = DateTime.Now.ToString("yyyyMMdd");

        //Test_v1.0.0_20220512_1
        string apkName = "Test" + "_v" + Application.version + "_" + timeStr + "_" + APK_FLAG + ".apk";
        string apkFullName = Path.Combine(apkRootPath, apkName);
        string[] buildSceneNames = GetBuildScenes().ToArray();
        BuildPipeline.BuildPlayer(buildSceneNames, apkFullName, BuildTarget.Android, BuildOptions.None);
        //打开APK目录
        Process.Start("Explorer.exe", apkRootPath);
        UnityEngine.Debug.Log("打包完成，输出目录:" + apkFullName);
    }
    /// <summary>
    /// 拷贝A到B目录
    /// </summary>
    private static void CopyAssetBundlesToStreamingAssets()
    {
        string srcPath = Path.Combine(Application.dataPath, "AssetBundles/Android");
        string desPath = Path.Combine(Application.dataPath, "StreamingAssets/Assets");
        if (!Directory.Exists(srcPath)) return;
        if (!Directory.Exists(desPath)) Directory.CreateDirectory(desPath);
        CopyDirectory(srcPath, desPath);
    }
    /// <summary>
    /// 删除目录
    /// </summary>
    private static void DeleteAssetBundlesInStreamingAssets()
    {
        string deletePath = Path.Combine(Application.dataPath, "StreamingAssets/Assets");
        DeleteDirectory(deletePath);
    }
    /// <summary>
    /// 删除文件夹
    /// </summary>
    private static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        Directory.Delete(path, true);
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 复制文件夹
    /// </summary>
    private static void CopyDirectory(string sourceFolder, string targetFolder)
    {
        try
        {
            //如果目标路径不存在,则创建目标路径
            if (!System.IO.Directory.Exists(targetFolder))
            {
                System.IO.Directory.CreateDirectory(targetFolder);
            }
            //得到原文件根目录下的所有文件
            string[] files = System.IO.Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                string dest = System.IO.Path.Combine(targetFolder, name);
                System.IO.File.Copy(file, dest);//复制文件
            }
            //得到原文件根目录下的所有文件夹
            string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = System.IO.Path.GetFileName(folder);
                string dest = System.IO.Path.Combine(targetFolder, name);
                CopyDirectory(folder, dest);//构建目标路径,递归复制文件
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("复制文件夹出错:" + e.Message);
        }

    }
    /// <summary>
    /// 获取所有需要打包场景名
    /// </summary>
    private static List<string> GetBuildScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        List<string> sceneNames = new List<string>();
        foreach (var scene in scenes)
        {
            if (scene.enabled) sceneNames.Add(scene.path);
        }
        return sceneNames;
    }
}