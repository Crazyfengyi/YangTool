using System;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[AttributeUsage(AttributeTargets.Field|AttributeTargets.Enum,Inherited = true)]
public class MutilSelectEnumAttribute : PropertyAttribute
{
    public string propertyName;
    public MutilSelectEnumAttribute(string name)
    {
        propertyName = name;
    }
}