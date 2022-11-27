/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 阵营
/// </summary>
[Flags]
public enum ActorCampType
{
    None = 0,
    Player = 1 << 1,
    Monster = 1 << 2,
    Npc = 1 << 3,
    Building = 1 << 4,

    MonsterAndBuilding = Monster + Building,
    PlayerAndBuilding = Player + Building,
}
/// <summary>
/// 攻击信息
/// </summary>
public class AtkInfo
{
    public GameActor targetActor;
    public EffectInfo atkEffectInfo;
}
/// <summary>
/// 伤害信息
/// </summary>
public class DamageInfo
{
    public float damage;
    public Vector3 atkPos;
}
/// <summary>
/// 特效信息
/// </summary>
public class EffectInfo
{

}
/// <summary>
/// 子弹发射类型
/// </summary>
public enum BulletShootType
{
    None,
    Circle
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
/// 角色大小类型
/// </summary>
public enum RoleSizeType
{
    None,
    ColliderSize,
    MeshSize,
}
/// <summary>
/// 角色属性
/// </summary>
public enum RoleAttribute
{
    None,
    HP,
    MP,
    Atk,
    Def,
    AtkRang,

}
/// <summary>
/// 角色标记
/// </summary>
public enum RoleFlag
{
    None,
    标记1,
}
/// <summary>
/// 子弹死亡类型
/// </summary>
public enum BulletDieType
{
    None,
    TimeOut,
    SceneChange,
    Atk,
}