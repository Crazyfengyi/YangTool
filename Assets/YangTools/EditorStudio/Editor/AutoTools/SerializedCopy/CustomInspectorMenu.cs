using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameMain;
using UnityEngine;
using UnityEditor;

public class YourCustomScriptEditor
{
    public static object SerializedObject;

    [MenuItem("CONTEXT/MonoBehaviour/SerializeCopy")]
    private static void SerializeCopy(MenuCommand command)
    {
        Type type = command.context.GetType();
        SerializedObject = command.context;
        Debug.LogError($"复制{type}");
    }

    [MenuItem("CONTEXT/MonoBehaviour/SerializePaste")]
    private static void SerializePaste(MenuCommand command)
    {
        Type temp = SerializedObject.GetType();
        Debug.LogError($"来源目标{temp}");
        Type type = command.context.GetType();

        // 获取所有公共字段
        FieldInfo[] publicFields = temp.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in publicFields)
        {
            //使用反射获取字段值
            FieldInfo fieldInfo = temp.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
            object fieldValue = fieldInfo.GetValue(SerializedObject);

            //使用反射设置字段值
            FieldInfo fieldInfoB = type.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfoB != null)
            {
                fieldInfoB.SetValue(command.context, fieldValue);
            }
        }

        Debug.LogError($"粘贴目标{type}");
    }
}