#if UNITY_EDITOR
/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-11 
*/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    #region 多选枚举
    // /// <summary>
    // /// 多选枚举
    // /// </summary>
    // [CustomPropertyDrawer(typeof(MutilSelectEnumAttribute))]
    // public class MutilSelectEnumEditorDrawer : PropertyDrawer
    // {
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent lable)
    //     {
    //         MutilSelectEnumAttribute state = attribute as MutilSelectEnumAttribute; 
    //         property.intValue = EditorGUI.MaskField(position,state.propertyName,property.intValue,property.enumDisplayNames);
    //     }
    // }
    #endregion

    //Unity已有[InspectorName("中文")]
    
    // #region  中文枚举
    // [CustomPropertyDrawer(typeof(EnumLabelAttribute))]
    // public class EnumLabelDrawer : PropertyDrawer
    // {
    //     private readonly List<string> m_displayNames = new List<string>();
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         var att = (EnumLabelAttribute)attribute;
    //         var type = property.serializedObject.targetObject.GetType();
    //         var field = type.GetField(property.name);
    //         var enumType = field.FieldType;
    //         foreach (var enumName in property.enumNames)
    //         {
    //             var enumField = enumType.GetField(enumName);
    //             var hds = enumField.GetCustomAttributes(typeof(HeaderAttribute), false);
    //             m_displayNames.Add(hds.Length <= 0 ? enumName : ((HeaderAttribute)hds[0]).header);
    //         }
    //         EditorGUI.BeginChangeCheck();
    //         var value = EditorGUI.Popup(position, att.header, property.enumValueIndex, m_displayNames.ToArray());
    //         if (EditorGUI.EndChangeCheck())
    //         {
    //             property.enumValueIndex = value;
    //         }
    //     }
    // }
    //
    // //例:
    // // public class EnumTest : MonoBehaviour
    // // {
    // //     [EnumLabel("动画类型")]
    // //     public EmAniType AniType;
    // // }
    // // public enum EmAniType
    // // {
    // //     [Header("待机")]
    // //     Idle,
    // // }
    // #endregion
}
#endif