using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


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

    #region  字符串扩展
    // [CustomPropertyDrawer(typeof(string))]
    // public class StringPropertyDrawer : PropertyDrawer
    // {
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         Rect btnRect = new Rect(position);
    //         position.width -= 60;
    //         btnRect.x += btnRect.width - 60;
    //         btnRect.width = 60;
    //         EditorGUI.BeginProperty(position, label, property);
    //         EditorGUI.PropertyField(position, property, true);
    //         if (GUI.Button(btnRect, "select"))
    //         {
    //             string path = property.stringValue;
    //             string selectStr = EditorUtility.OpenFilePanel("Select Files", path, "");
    //             if (!string.IsNullOrEmpty(selectStr))
    //             {
    //                 property.stringValue = selectStr;
    //             }
    //         }

    //         EditorGUI.EndProperty();
    //     }
    // }
    #endregion