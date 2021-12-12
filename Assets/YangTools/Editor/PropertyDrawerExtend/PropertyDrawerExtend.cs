/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-11 
*/  
using System;
using UnityEngine;  
using UnityEditor;
using System.Collections.Generic;

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
//         Rect btnRect2 = new Rect(position);
//         position.width -= 60;
//         btnRect2.width = 60;
//         btnRect2.x = 20;

//         EditorGUI.BeginProperty(position, label, property);
//         EditorGUI.LabelField(btnRect2, property.displayName);
//         EditorGUI.PropertyField(position, property, true);
//         // if (GUI.Button(btnRect, "select"))
//         // {
//         //     string path = property.stringValue;
//         //     string selectStr = EditorUtility.OpenFilePanel("Select Files", path, "");
//         //     if (!string.IsNullOrEmpty(selectStr))
//         //     {
//         //         property.stringValue = selectStr;
//         //     }
//         // }
//         EditorGUI.EndProperty();
//     }
// }
#endregion

#region  中文枚举
[CustomPropertyDrawer(typeof(EnumLabelAttribute))]
public class EnumLabelDrawer : PropertyDrawer
{
    private readonly List<string> m_displayNames = new List<string>();
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var att = (EnumLabelAttribute)attribute;
        var type = property.serializedObject.targetObject.GetType();
        var field = type.GetField(property.name);
        var enumtype = field.FieldType;
        foreach (var enumName in property.enumNames)
        {
            var enumfield = enumtype.GetField(enumName);
            var hds = enumfield.GetCustomAttributes(typeof(HeaderAttribute), false);
            m_displayNames.Add(hds.Length <= 0 ? enumName : ((HeaderAttribute)hds[0]).header);
        }
        EditorGUI.BeginChangeCheck();
        var value = EditorGUI.Popup(position, att.header, property.enumValueIndex, m_displayNames.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            property.enumValueIndex = value;
        }
    }
}

//例:
// public class EnumTest : MonoBehaviour
// {
//     [EnumLabel("动画类型")]
//     public EmAniType AniType;
// }
// public enum EmAniType
// {
//     [Header("待机")]
//     Idle,
// }
#endregion

#region  SCriptObject扩展
// [CustomPropertyDrawer(typeof(ExpandableAttribute), true)]
// public class ExpandableAttributeDrawer : PropertyDrawer
// {
//     // Use the following area to change the style of the expandable ScriptableObject drawers;
//     #region Style Setup
//     private enum BackgroundStyles
//     {
//         None,
//         HelpBox,
//         Darken,
//         Lighten
//     }

//     /// <summary>
//     /// Whether the default editor Script field should be shown.
//     /// </summary>
//     private static bool SHOW_SCRIPT_FIELD = false;

//     /// <summary>
//     /// The spacing on the inside of the background rect.
//     /// </summary>
//     private static float INNER_SPACING = 6.0f;

//     /// <summary>
//     /// The spacing on the outside of the background rect.
//     /// </summary>
//     private static float OUTER_SPACING = 4.0f;

//     /// <summary>
//     /// The style the background uses.
//     /// </summary>
//     private static BackgroundStyles BACKGROUND_STYLE = BackgroundStyles.HelpBox;

//     /// <summary>
//     /// The colour that is used to darken the background.
//     /// </summary>
//     private static Color DARKEN_COLOUR = new Color(0.0f, 0.0f, 0.0f, 0.2f);

//     /// <summary>
//     /// The colour that is used to lighten the background.
//     /// </summary>
//     private static Color LIGHTEN_COLOUR = new Color(1.0f, 1.0f, 1.0f, 0.2f);
//     #endregion

//     /// <summary>
//     /// Cached editor reference.
//     /// </summary>
//     private Editor editor = null;

//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         float totalHeight = 0.0f;

//         totalHeight += EditorGUIUtility.singleLineHeight;

//         if (property.objectReferenceValue == null)
//             return totalHeight;

//         if (!property.isExpanded)
//             return totalHeight;

//         if (editor == null)
//             Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

//         if (editor == null)
//             return totalHeight;

//         SerializedProperty field = editor.serializedObject.GetIterator();

//         field.NextVisible(true);

//         if (SHOW_SCRIPT_FIELD)
//         {
//             totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//         }

//         while (field.NextVisible(false))
//         {
//             totalHeight += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
//         }

//         totalHeight += INNER_SPACING * 2;
//         totalHeight += OUTER_SPACING * 2;

//         return totalHeight;
//     }
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         Rect fieldRect = new Rect(position);
//         fieldRect.height = EditorGUIUtility.singleLineHeight;

//         EditorGUI.PropertyField(fieldRect, property, label, true);

//         if (property.objectReferenceValue == null)
//             return;

//         property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, GUIContent.none, true);

//         if (!property.isExpanded)
//             return;

//         if (editor == null)
//             Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

//         if (editor == null)
//             return;


//         #region Format Field Rects
//         List<Rect> propertyRects = new List<Rect>();
//         Rect marchingRect = new Rect(fieldRect);

//         Rect bodyRect = new Rect(fieldRect);
//         bodyRect.xMin += EditorGUI.indentLevel * 14;
//         bodyRect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
//             + OUTER_SPACING;

//         SerializedProperty field = editor.serializedObject.GetIterator();
//         field.NextVisible(true);

//         marchingRect.y += INNER_SPACING + OUTER_SPACING;

//         if (SHOW_SCRIPT_FIELD)
//         {
//             propertyRects.Add(marchingRect);
//             marchingRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//         }

//         while (field.NextVisible(false))
//         {
//             marchingRect.y += marchingRect.height + EditorGUIUtility.standardVerticalSpacing;
//             marchingRect.height = EditorGUI.GetPropertyHeight(field, true);
//             propertyRects.Add(marchingRect);
//         }

//         marchingRect.y += INNER_SPACING;

//         bodyRect.yMax = marchingRect.yMax;
//         #endregion

//         DrawBackground(bodyRect);

//         #region Draw Fields
//         EditorGUI.indentLevel++;

//         int index = 0;
//         field = editor.serializedObject.GetIterator();
//         field.NextVisible(true);

//         if (SHOW_SCRIPT_FIELD)
//         {
//             //Show the disabled script field
//             EditorGUI.BeginDisabledGroup(true);
//             EditorGUI.PropertyField(propertyRects[index], field, true);
//             EditorGUI.EndDisabledGroup();
//             index++;
//         }

//         //Replacement for "editor.OnInspectorGUI ();" so we have more control on how we draw the editor
//         while (field.NextVisible(false))
//         {
//             try
//             {
//                 EditorGUI.PropertyField(propertyRects[index], field, true);
//             }
//             catch (StackOverflowException)
//             {
//                 field.objectReferenceValue = null;
//                 Debug.LogError("Detected self-nesting cauisng a StackOverflowException, avoid using the same " +
//                     "object iside a nested structure.");
//             }

//             index++;
//         }

//         EditorGUI.indentLevel--;
//         #endregion
//     }

//     /// <summary>
//     /// Draws the Background
//     /// </summary>
//     /// <param name="rect">The Rect where the background is drawn.</param>
//     private void DrawBackground(Rect rect)
//     {
//         switch (BACKGROUND_STYLE)
//         {

//             case BackgroundStyles.HelpBox:
//                 EditorGUI.HelpBox(rect, "", MessageType.None);
//                 break;
//             case BackgroundStyles.Darken:
//                 EditorGUI.DrawRect(rect, DARKEN_COLOUR);
//                 break;
//             case BackgroundStyles.Lighten:
//                 EditorGUI.DrawRect(rect, LIGHTEN_COLOUR);
//                 break;
//         }
//     }
// }
#endregion

