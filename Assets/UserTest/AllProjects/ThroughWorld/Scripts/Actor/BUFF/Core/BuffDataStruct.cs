/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using Sirenix.OdinInspector;
using System;
using DataStruct;

/// <summary>
/// buff状态
/// </summary>
public enum BuffState
{
    None,
    启用,
    禁用
}
/// <summary>
/// buff叠加类型
/// </summary>
public enum BuffAppendType
{
    None,
    /// <summary>
    /// 刷新--刷新数据
    /// </summary>
    Refresh,
    /// <summary>
    /// 替换--移除旧的,添加新的
    /// </summary>
    Replace,
    /// <summary>
    /// 叠加--叠多成依次触发
    /// </summary>
    Overlay,
    /// <summary>
    /// 并存--同时触发
    /// </summary>
    Coexist,
    /// <summary>
    /// 舍弃 
    /// </summary>
    Discard,
    /// <summary>
    /// 累加--多个提升等级
    /// </summary>
    Add
}
/// <summary>
/// 角色状态
/// </summary>
[Flags]
public enum RoleState
{
    None = 0,
    攻击免疫 = 1 << 1,
    致盲状态 = 1 << 2,
    不能闪避 = 1 << 3,
    禁魔状态 = 1 << 4,
    缴械状态 = 1 << 5,
    冰冻状态 = 1 << 6,
    隐身状态 = 1 << 7,
    无敌状态 = 1 << 8,
    催眠状态 = 1 << 9,
    禁用被动 = 1 << 10,
    缠绕状态 = 1 << 11,
    沉默状态 = 1 << 12,
    眩晕状态 = 1 << 13,
    无法选中 = 1 << 14,
    无碰撞 = 1 << 15,
    魔法免疫 = 1 << 16,
    飞行状态 = 1 << 17,
    禁用物品 = 1 << 18,
}
/// <summary>
/// 条件左值枚举
/// </summary>
public enum ConditionFunType
{
    None,
    当前血量
}
/// <summary>
/// 运算符号
/// </summary>
public enum OperatorSymbol
{
    None,
    小于,
    大于,
    等于
}
/// <summary>
/// 条件信息
/// </summary>
public class Condition
{
    public ConditionFunType funType;
    public OperatorSymbol @operator;
    public float vaule;

    /// <summary>
    /// 获得判断是否成功
    /// </summary>
    /// <returns></returns>
    public bool GetIsSuccee()
    {
        return false;
    }
}
/// <summary>
/// 生效事件触发委托
/// </summary>
public delegate void ActiveInvokeDelegate(RoleBase tirgger, RoleBase owner);
/// <summary>
/// 失效事件触发委托
/// </summary>
public delegate void DeActiveInvokeDelegate(RoleBase tirgger, RoleBase owner);

public enum BuffType
{
    None,
    PropertyChange,
}
public enum BuffEndType
{
    None,
    TimeOver,
    CountOver,
    TimeOrCountOver
}
public enum BuffFlagType
{
    None,
    Common,
}
public enum BuffID
{
    None,
    buff_10001,
}
/// <summary>
/// buff属性改变信息
/// </summary>
[Serializable]
public struct BuffAttributeChangeInfo
{
    [LabelText("角色属性")]
    public RoleAttribute roleAttribute;
    [LabelText("属性值类型")]
    public AttributeValueType attributeValueType;
    [LabelText("属性值")]
    public float value;
}