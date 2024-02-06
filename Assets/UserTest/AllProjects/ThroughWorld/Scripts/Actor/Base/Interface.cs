/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/

using DataStruct;
using UnityEngine;

/// <summary>
/// 攻击接口
/// </summary>
public interface IAtker
{
    /// <summary>
    /// 攻击
    /// </summary>
    void Atk(AtkInfo atkInfo);
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
    void IInit();
    void IUpdate();
    void ILateUpdate();
    void IFixedUpdate();
    void IDie();
    void IDestroy();
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
    /// 获得大小
    /// </summary>
    public float GetSize(RoleSizeType roleSizeType)
    {
        return 0;
    }
    /// <summary>
    /// 是有效的
    /// </summary>
    public bool IsValid()
    {
        return true;
    }
    /// <summary>
    /// 获得位置
    /// </summary>
    public Vector3 GetPos()
    {
        return Vector3.zero;
    }
    /// <summary>
    /// 距离
    /// </summary>
    public float DistanceToPoint(Vector3 point)
    {
        return float.MaxValue;
    }
    /// <summary>
    /// 交互范围
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
    /// 进入触发范围
    /// </summary>
    public void OnEnterRang(RoleBase role)
    {

    }
    /// <summary>
    /// 退出触发范围
    /// </summary>
    public void OnExitRang(RoleBase role)
    {

    }
    /// <summary>
    /// 处理交互 
    /// </summary>
    public void Interact(RoleBase role)
    {

    }
    //string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped);
}
