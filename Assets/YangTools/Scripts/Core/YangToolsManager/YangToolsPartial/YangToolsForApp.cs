
using System;
using System.Reflection;
using UnityEngine;

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
        /// <summary>
        /// 开关鼠标指针
        /// </summary>
        public static void SetCursorLock(bool lockCursor)
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
