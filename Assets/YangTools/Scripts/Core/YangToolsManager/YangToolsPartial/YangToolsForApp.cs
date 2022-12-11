
using System;
using System.Reflection;

namespace YangTools
{
    public static partial class YangToolsManager
    {
        /// <summary>
        /// 清空UnityLog
        /// </summary>
        private static void ClearConsole()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            Type type = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}
