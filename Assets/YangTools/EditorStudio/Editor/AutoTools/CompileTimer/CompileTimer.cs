#if UNITY_EDITOR
/*
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:2023-11-12 
*/
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

/// <summary>
/// 编译时间统计
/// </summary>
[InitializeOnLoad]
public static class CompileTimer
{
    private static double StartTime;
    static CompileTimer()
    {
        CompilationPipeline.compilationStarted += CompilationStarted;
        CompilationPipeline.compilationFinished += CompilationFinished;
    }
    /// <summary>
    /// 编译开始
    /// </summary>
    private static void CompilationStarted(object obj)
    {
        StartTime = EditorApplication.timeSinceStartup;
    }
    /// <summary>
    /// 编译结束
    /// </summary>
    private static void CompilationFinished(object obj)
    {
        var endTime = EditorApplication.timeSinceStartup;
        var timer = endTime - StartTime;
        Debug.Log($"编译时间:{timer:F2}秒");
    }
}
#endif