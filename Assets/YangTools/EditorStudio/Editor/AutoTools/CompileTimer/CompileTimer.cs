/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-11-12 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using UnityEditor;
using UnityEditor.Compilation;

/// <summary>
/// 编译时间统计
/// </summary>
[InitializeOnLoad]
public static class CompileTimer
{
    static double startTime;
    static CompileTimer()
    {
        CompilationPipeline.compilationStarted += CompilationStarted;
        CompilationPipeline.compilationFinished += CompilationFinished;
    }

    static void CompilationStarted(object obj)
    {
        startTime = EditorApplication.timeSinceStartup;
    }

    static void CompilationFinished(object obj)
    {
        double endTime = EditorApplication.timeSinceStartup;
        double timer = endTime - startTime;
        Debug.Log($"编译时间:{timer:F2} 秒");
    }
}