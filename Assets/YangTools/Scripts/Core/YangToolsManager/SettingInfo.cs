/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       YangWork
 *UnityVersion：2020.3.1f1c1
 *创建时间:         2021-04-20
*/

using UnityEngine;

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
        private const string menuName = "YangTools";

        /// <summary>
        /// 辅助功能
        /// </summary>
        private const string functionName = "辅助工具";

        /// <summary>
        /// 游戏工具
        /// </summary>
        private const string gameToolName = "游戏工具";

        /// <summary>
        /// 打包工具
        /// </summary>
        private const string buildToolName = "打包工具";

        /// <summary>
        /// 菜单主路径
        /// </summary>
        public const string MenuPath = menuName + "/";

        /// <summary>
        /// 辅助功能路径
        /// </summary>
        public const string YongToolsFunctionPath = menuName + "/" + functionName + "/";

        /// <summary>
        /// 游戏工具路径
        /// </summary>
        public const string YongToolsGameToolPath = menuName + "/" + gameToolName + "/";

        /// <summary>
        /// 打包工具路径
        /// </summary>
        public const string YongToolsBuildToolPath = menuName + "/" + buildToolName + "/";

        /// <summary>
        /// UI创建扩展路径
        /// </summary>
        public const string YangToolUIPath = "GameObject/UI/YangTool/";

        /// <summary>
        /// Asset编辑器文件路径
        /// </summary>
        public const string YangToolAssetPath = "Assets/YangTools/EditorStudio/Editor/";

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