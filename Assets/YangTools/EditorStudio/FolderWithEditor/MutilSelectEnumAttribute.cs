using System;
using UnityEngine;
namespace YangTools
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, Inherited = true)]
    public class MutilSelectEnumAttribute : PropertyAttribute
    {
        public string propertyName;
        public MutilSelectEnumAttribute(string name)
        {
            propertyName = name;
        }
    }
}