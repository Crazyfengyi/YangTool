
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
        /// <summary>
        /// 是:手机,否:平板
        /// </summary>
        public static bool IsPhone()
        {
            bool isPhone = true;

            //iPhone，可以根据设备名字判断是否包含iphone
            //android，可以比较设备分辨率的比例 是否大于4:3
#if UNITY_IPHONE && !UNITY_EDITOR
            string deviceInfo = SystemInfo.deviceModel.ToString();
            isPhone = deviceInfo.Contains("iPhone");
#else
            float physicscreen = 1.0f * Screen.width / Screen.height;
            isPhone = physicscreen > 1.7f; //(the ratio 4:3 = 1.33; 16:9 = 1.777;)
#endif
            return isPhone;

        }
        /// <summary>
        /// copy到粘贴板
        /// </summary>
        public static void CopyStringToClipboard(string str)
        {
            var te = new TextEditor();
            te.text = str; 
            te.SelectAll();
            te.Copy();
        }
    }
}