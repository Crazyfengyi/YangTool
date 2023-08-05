/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 阵营
/// </summary>
[Flags]
public enum ActorCampType
{
    [LabelText("未知的")]
    None = 0,
    [LabelText("玩家")]
    Player = 1 << 1,
    [LabelText("怪物")]
    Monster = 1 << 2,
    [LabelText("NPC")]
    Npc = 1 << 3,
    [LabelText("建筑")]
    Building = 1 << 4,
    [LabelText("怪物+建筑")]
    MonsterAndBuilding = Monster + Building,
    [LabelText("玩家+建筑")]
    PlayerAndBuilding = Player + Building,
}
/// <summary>
/// 攻击信息
/// </summary>
public class AtkInfo
{
    public GameActor sourceActor;//攻击来源
    public GameActor targetActor;//攻击目标
    public EffectInfo atkEffectInfo;
}
/// <summary>
/// 击中伤害信息
/// </summary>
public class DamageInfo
{
    public float damage;
    public Vector3 atkPos;//攻击起点
    public Vector3 behitPos;//受击点
    public EffectInfo beHitEffectInfo;//击中特效
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
    Circle,
    Throw,
    Parabola,
    Physics,
    Bomb
}

/// <summary>
/// 子弹类型
/// </summary>
public enum BulletType
{
    None,
    Circle,
    Throw,
    Parabola,
    Physics,
    Bomb
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
    /// <summary>
    /// 生存时间结束
    /// </summary>
    TimeEnd,
    /// <summary>
    /// 场景切换
    /// </summary>
    SceneChange,
    /// <summary>
    /// 检测到攻击目标
    /// </summary>
    HaveAtk,
    /// <summary>
    /// 碰撞到建筑物和地面
    /// </summary>
    Collision,
    /// <summary>
    /// 到达目标点
    /// </summary>
    EndPoint,
}