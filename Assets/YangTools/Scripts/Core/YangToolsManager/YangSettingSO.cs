/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0b14 
 *创建时间:         2022-04-07 
*/
using Sirenix.OdinInspector;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 工具设置开关
    /// </summary>
    [CreateAssetMenu(fileName = "YangSettingSO", menuName = "YangTool/YangSettingSO")]
    public class YangSettingSO : ScriptableObject
    {
        [LabelText("开关Hierarchy面板设置")]
        public bool isOpenHierarchyShowSetting = false;
    }
}