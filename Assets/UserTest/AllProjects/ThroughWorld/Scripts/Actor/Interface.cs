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
/// <summary>
/// 交互者
/// </summary>
public interface IInteractor
{


}

/// <summary>
/// 交互物接口
/// </summary>
public interface IInteractive
{
    /// <summary>
    /// 距离
    /// </summary>
    public float Distance(Vector3 point)
    {
        return float.MaxValue;
    }
    /// <summary>
    /// 覆盖范围(物体需要的范围)
    /// </summary>
    public float GetOverideMaxDistance()
    {
        return float.MaxValue;
    }
    /// <summary>
    /// 交互类型
    /// </summary>
    public InterActiveType GetInterActiveType()
    {
        return InterActiveType.None;
    }
    /// <summary>
    /// 可以交互
    /// </summary>
    public bool CanInter()
    {
        return true;
    }
    /// <summary>
    /// 进入范围
    /// </summary>
    public void EnterRang(RoleBase role)
    {

    }
    /// <summary>
    /// 退出范围
    /// </summary>
    public void ExitRang(RoleBase role)
    {

    }
    /// <summary>
    /// 交互 
    /// </summary>
    public void InterAction(RoleBase role)
    {

    }
}