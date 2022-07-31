/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using UnityEngine;
using System.Collections;

public class RoleBase : GameActor, IAtker, IBeHit
{
    #region 变量

    #endregion

    #region 生命周期接口实现
    public override void IInit()
    {
        throw new System.NotImplementedException();
    }
    public override void IDie()
    {
        throw new System.NotImplementedException();
    }
    public override void IUpdate()
    {
        throw new System.NotImplementedException();
    }
    public override void ILateUpdate()
    {
        throw new System.NotImplementedException();
    }
    public override void IFixedUpdate()
    {
        throw new System.NotImplementedException();
    }
   
    #endregion

    #region 攻击和被击接口实现
    public virtual void Atk(int index)
    {
        
    }
    public virtual void BeHit(ref DamageInfo damageInfo)
    {
        
    }
    public virtual DamageInfo GetDamageInfo()
    {
        return null;
    }
    public virtual DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        return null;
    }
    public virtual bool IsCanAtk()
    {
        return false;
    }
    public virtual bool IsCanBeHit()
    {
        return false;
    }
    public virtual void ShowAtkEffect(EffectInfo atkEffectInfo)
    {
    }
    public virtual void ShowBeHitEffect(EffectInfo hitEffectInfo)
    {
    }

    #endregion
}