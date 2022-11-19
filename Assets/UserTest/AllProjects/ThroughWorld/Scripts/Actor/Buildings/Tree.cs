/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-09-04 
*/
using UnityEngine;
using System.Collections;
using YangTools;
using YangTools.Extend;

/// <summary>
/// 树
/// </summary>
public class Tree : GameActor, IAtker, IBeHit
{
    public override void IInit()
    {
    }
    public override void IUpdate()
    {
    }
    public override void ILateUpdate()
    {
    }
    public override void IFixedUpdate()
    {
    }
    public override void IDie()
    {
    }
    public override void IDestroy()
    {
    }
    public override bool IsCanAtk()
    {
        return false;
    }
    public override bool IsCanBeHit()
    {
        return true;
    }
    public override void Atk(AtkInfo atkInfo)
    {
        throw new System.NotImplementedException();
    }
    public override void BeHit(ref DamageInfo damageInfo)
    {
        throw new System.NotImplementedException();
    }
    public override DamageInfo GetDamageInfo()
    {
        throw new System.NotImplementedException();
    }
    public override DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        throw new System.NotImplementedException();
    }
    public override void ShowAtkEffect(EffectInfo atkEffectInfo)
    {
    }
    public override void ShowBeHitEffect(EffectInfo hitEffectInfo)
    {
    }

    #region 交互物
    /// <summary>
    /// 距离
    /// </summary>
    public override float Distance(Vector3 point)
    {
        return base.Distance(point);
    }
    /// <summary>
    /// 覆盖范围(物体需要的范围)
    /// </summary>
    public override float GetOverideMaxDistance()
    {
        return float.MaxValue;
    }
    /// <summary>
    /// 交互类型
    /// </summary>
    public InterActiveType GetInterActiveType()
    {
        return InterActiveType.Trigger;
    }
    /// <summary>
    /// 可以交互
    /// </summary>
    public override bool CanInter()
    {
        return true;
    }
    /// <summary>
    /// 进入范围
    /// </summary>
    public override void EnterRang(RoleBase role)
    {

    }
    /// <summary>
    /// 退出范围
    /// </summary>
    public override void ExitRang(RoleBase role)
    {

    }
    /// <summary>
    /// 交互 
    /// </summary>
    public override void InterAction(RoleBase role)
    {
        float size = GetSize(RoleSizeType.ColliderSize);
        Vector3 temp = Random.onUnitSphere;
        temp = temp.SetYValue();
        temp = temp.normalized;
        GameObject obj = GameActorManager.Instance.CreateItem("Apple", transform.position + temp * size);
    }
    #endregion
}