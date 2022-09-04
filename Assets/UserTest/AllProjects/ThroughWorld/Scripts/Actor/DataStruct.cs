/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using UnityEngine;
using System.Collections;

/// <summary>
/// 阵营
/// </summary>
public enum ActorCampType
{
    None,
    Monster,
    Player,
    Npc,
    Building
}
/// <summary>
/// 伤害信息
/// </summary>
public class DamageInfo
{

}
/// <summary>
/// 特效信息
/// </summary>
public class EffectInfo
{

}
/// <summary>
/// 交互类型
/// </summary>
public enum InterActiveType
{
    /// <summary>
    /// 默认
    /// </summary>
    None,
    /// <summary>
    /// 对话
    /// </summary>
    Dialogue,
    /// <summary>
    /// 触发
    /// </summary>
    Trigger,
}
/// <summary>
/// 角色属性
/// </summary>
public enum RoleAttribute
{
    None,
    HP,
    MP,
}
/// <summary>
/// 角色标记
/// </summary>
public enum RoleFlag
{
    None,
    标记1,
}