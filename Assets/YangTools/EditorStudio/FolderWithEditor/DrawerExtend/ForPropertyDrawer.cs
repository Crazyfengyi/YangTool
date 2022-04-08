/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-11 
*/  
using UnityEngine;
namespace YangTools
{
    #region  中文枚举
    public class EnumLabelAttribute : HeaderAttribute
    {
        public EnumLabelAttribute(string header) : base(header)
        {
        }
    }
    #endregion

    #region  ScriptObject显示
    public class ExpandableAttribute : PropertyAttribute
    {
        public ExpandableAttribute()
        {

        }
    }
    #endregion
}