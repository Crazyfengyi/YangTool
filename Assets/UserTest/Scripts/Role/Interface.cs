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
/// 攻击接口
/// </summary>
public interface IAtker
{
    /// <summary>
    /// 攻击
    /// </summary>
    void Atk(int index);
    /// <summary>
    /// 是否可以攻击
    /// </summary>
    bool IsCanAtk();
    /// <summary>
    /// 获得伤害信息
    /// </summary>
    DamageInfo GetDamageInfo();
    /// <summary>
    /// 显示攻击特效
    /// </summary>
    void ShowAtkEffect(EffectInfo atkEffectInfo);
}
/// <summary>
/// 被击接口
/// </summary>
public interface IBeHit
{
    /// <summary>
    /// 被攻击
    /// </summary>
    void BeHit(ref DamageInfo damageInfo);
    /// <summary>
    /// 是否可以被击
    /// </summary>
    bool IsCanBeHit();
    /// <summary>
    /// 受击计算
    /// </summary>
    DamageInfo GetHitCompute(DamageInfo damageInfo);
    /// <summary>
    /// 显示受击特效
    /// </summary>
    void ShowBeHitEffect(EffectInfo hitEffectInfo);
}
/// <summary>
/// 自定义生命周期
/// </summary>
public interface ICustomLife
{
    /// <summary>
    /// 初始化
    /// </summary>
    void IInit();
    void IUpdate();
    void ILateUpdate();
    void IFixedUpdate();
    /// <summary>
    /// 死亡
    /// </summary>
    void IDie();
}