/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-16 
*/
using UnityEngine;

namespace YangTools
{
    /// <summary>
    ///  use [Autohook] to modify Component
    ///  use [Autohook(AutohookAttribute.HookType.Prefab)] to modify Prefab
    /// </summary>
    public class AutohookAttribute : PropertyAttribute
    {
        /// <summary>
        /// 用默认模式(根据名字查找)
        /// </summary>
        public bool useDefault = true;

        public readonly HookType hookType;
        public readonly string prefabPath;//预制体路径
        public readonly string prefabName;//预制体名称
        public readonly string relativePath; //子物体相对路径

        public enum HookType
        {
            Component,
            Prefab
        }

        public AutohookAttribute()
        {
            hookType = HookType.Component;
            useDefault = true;
        }

        /// <summary>
        /// 自动挂载引用
        /// </summary>
        /// <param name="_hookType">自动寻找类型</param>
        /// <param name="_path">子物体:相对路径 预制体:文件路径</param>
        /// <param name="_prefabName">预制体名称(子物体填null)</param>
        /// <param name="_usdefault">是否用默认寻找方式(找字段名称对应的物体)</param>
        public AutohookAttribute(HookType _hookType = HookType.Component, string _path = "Assets", string _prefabName = "", bool _usdefault = true)
        {
            hookType = _hookType;
            useDefault = _usdefault;

            switch (hookType)
            {
                case HookType.Component:
                    relativePath = _path;
                    break;
                case HookType.Prefab:
                    prefabPath = _path;
                    prefabName = _prefabName;
                    if (System.String.IsNullOrEmpty(prefabName))
                    {
                        Debug.LogError($"路径:{_path}==>{TypeId}:预制体名是空的,自动切换到默认寻找模式");
                        useDefault = true;
                    }
                    break;
            }
        }
    }
}