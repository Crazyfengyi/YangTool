/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-09-12 
*/
using System;
using System.Collections;
using UnityEditor.Build;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class BuildPostProcess : IPostprocessBuildWithReport
{
    public int callbackOrder => 100;
    public void OnPostprocessBuild(BuildReport report)
    {
        return;
        //判断打包平台
        if (report.summary.platform == BuildTarget.StandaloneLinux64)
        {
            PostProcessLinux(report.summary.outputPath);
        }
    }

    private void PostProcessLinux(string outputPath)
    {
        UnityEngine.Debug.Log("执行Linux包后处理");
        UnityEngine.Debug.Log($"Linux包输出目录：{outputPath}");
        string dirPath = Directory.GetParent(outputPath).FullName;
        string dirName = Path.GetDirectoryName(outputPath);

        //删除无用文件
        DeleteNotNecessaryFiles(dirPath);

        //注意！ 电脑上需要安装7z压缩软件!
        //使用7z命令，压缩安装包并命名
        string param = $"a -tzip {dirName}.zip {dirPath}/*";
        //是否执行成功都退出！
        param = param.Trim().TrimEnd('&') + " &exit";
        ExecuteZipCommand(param);
    }

    private void DeleteNotNecessaryFiles(string dir)
    {
        List<string> willDeleteFiles = new List<string>()
        {
            "UnityPlayer_s.debug",
            "LinuxPlayer_s.debug"
        };

        foreach (var file in willDeleteFiles)
        {
            string path = Path.Combine(dir, file);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static void ExecuteZipCommand(string cmd)
    {
        Process.Start(@"D:\Program Files\7-Zip\7z.exe", cmd);
    }
}