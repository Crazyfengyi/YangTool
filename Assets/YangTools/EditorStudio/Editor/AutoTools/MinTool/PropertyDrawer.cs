#if UNITY_EDITOR
/*
 *Copyright(C) 2020 by XCHANGE
 *All rights reserved.
 *Author:       YangWork
 *UnityVersion：2020.3.7f1c1
 *创建时间:         2021-06-15
 */
using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    //颜色
    [CustomPropertyDrawer(typeof(Color))]
    public class ColorPropertyDrawer : PropertyDrawer
    {
        private const float spacing = 5f;
        private const float hexWeight = 60f;
        private const float alphaWeight = 32f;

        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(pos, label, property);
            pos = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float colorWeight = pos.width - hexWeight - spacing - alphaWeight - spacing;
            Color newColor = EditorGUI.ColorField(new Rect(pos.x, pos.y, colorWeight, pos.height), property.colorValue);

            if (!newColor.Equals(property.colorValue))
            {
                property.colorValue = newColor;
            }

            string hex = EditorGUI.TextField(new Rect(pos.x + colorWeight + spacing, pos.y, hexWeight, pos.height),
                ColorUtility.ToHtmlStringRGB(property.colorValue));

            try
            {
                newColor = FromHex(hex, property.colorValue.a);
                if (!newColor.Equals(property.colorValue)) property.colorValue = newColor;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }

            float newAlpha = EditorGUI.Slider(
                new Rect(pos.x + colorWeight + hexWeight + spacing * 2, pos.y, alphaWeight, pos.height),
                property.colorValue.a, 0f, 1f);
            if (newAlpha.Equals(property.colorValue.a))
            {
                property.colorValue = new Color(property.colorValue.r, property.colorValue.g, property.colorValue.b,
                    newAlpha);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static Color FromHex(string hexValue, float aplha)
        {
            if (string.IsNullOrEmpty(hexValue)) return Color.clear;
            if (hexValue[0] == '#') hexValue = hexValue.TrimStart('#');
            if (hexValue.Length > 6)
            {
                hexValue = hexValue.Remove(6, hexValue.Length - 6);
            }

            int value = int.Parse(hexValue, NumberStyles.HexNumber);
            return new Color((float) ((value >> 16) & 255) / 255f, (float) ((value >> 8) & 255) / 255f,
                (float) (value & 255) / 255f, aplha);
        }
    }
    //sprite
    [CustomPropertyDrawer(typeof(Sprite))]
    public class SpritePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 110f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUI.ObjectField(new Rect(position.x, position.y, 100f, 100f), property, typeof(Sprite),
                GUIContent.none);

            if (property.objectReferenceValue != null)
            {
                Rect spriteNameRect = new Rect(position.x + 105f, position.y + 35f, position.width - 105f, position.height);
                EditorGUI.LabelField(spriteNameRect, property.objectReferenceValue.name);
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif