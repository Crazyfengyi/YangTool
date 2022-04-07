/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-20 
*/
using UnityEngine;
using System.Collections;

namespace YangTools
{
    /// <summary>
    /// 工具设置
    /// </summary>
    public sealed class SettingInfo
    {
        /// <summary>
        /// 菜单名称
        /// </summary>
        public const string menuName = "YangTools";
        /// <summary>
        /// 辅助功能
        /// </summary>
        public const string functionName = "辅助功能";
        /// <summary>
        /// 菜单主路径
        /// </summary>
        public const string MenuPath = menuName + "/";
        /// <summary>
        /// 辅助功能路径
        /// </summary>
        public const string YongToolsFunctionPath = menuName + "/" + functionName + "/";
        /// <summary>
        /// 获得ScriptableObject
        /// </summary>
        public static T GetSO<T>() where T : ScriptableObject
        {
            string typeName = typeof(T).Name;
            Object res = Resources.Load<Object>($"SO/{typeName}");
            return (T)res;
        }
    }
}