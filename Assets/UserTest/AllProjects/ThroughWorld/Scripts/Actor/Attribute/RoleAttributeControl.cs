/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-21 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

[Serializable]
/// <summary>
/// 角色属性控制器
/// </summary>
public class RoleAttributeControl
{
    private RoleBase handle;
    //血量
    public ValueTotal Attack => GetAttribute(RoleAttribute.HP);
    [ShowInInspector]
    //[SerializeField]
    /// <summary>
    /// 所有属性
    /// </summary>
    public Dictionary<RoleAttribute, ValueTotal> attributesDic = new Dictionary<RoleAttribute, ValueTotal>();
    public RoleAttributeControl()
    {

    }
    public void Init(RoleBase _handle)
    {
        handle = _handle;
    }
    /// <summary>
    /// 获取属性
    /// </summary>
    public ValueTotal GetAttribute(RoleAttribute roleAttribute)
    {
        if (!attributesDic.TryGetValue(roleAttribute, out ValueTotal attribute))
        {
            attribute = new ValueTotal(0);
            attributesDic.Add(roleAttribute, attribute);
        }
        return attribute;
    }
    /// <summary>
    /// 更改属性值
    /// </summary>
    public void ChangeAttribute(RoleAttribute roleAttribute, float value, AttributeValueType valueType = AttributeValueType.BaseValue)
    {
        if (attributesDic.TryGetValue(roleAttribute, out ValueTotal attribute))
        {
            attribute.AddValue(valueType, value);
        }
        else
        {
            switch (valueType)
            {
                case AttributeValueType.BaseValue:
                    attribute = new ValueTotal(value);
                    break;
                case AttributeValueType.AddValue:
                    attribute = new ValueTotal(0, value);
                    break;
                case AttributeValueType.AddPercent:
                    attribute = new ValueTotal(0, 0, value);
                    break;
                case AttributeValueType.FinalAdd:
                    attribute = new ValueTotal(0, 0, 0, value);
                    break;
                case AttributeValueType.FinalAddPercent:
                    attribute = new ValueTotal(0, 0, 0, 0, value);
                    break;
                default:
                    break;
            }
            attributesDic.Add(roleAttribute, attribute);
        }
    }
}

