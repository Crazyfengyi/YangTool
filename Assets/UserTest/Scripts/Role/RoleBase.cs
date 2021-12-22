/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using UnityEngine;
using System.Collections;

public class RoleBase : MonoBehaviour, IAtker, IBeHit, ICustomLife
{
    #region 变量

    #endregion

    #region 生命周期接口实现
    public void IInit()
    {
        throw new System.NotImplementedException();
    }
    public void IDie()
    {
        throw new System.NotImplementedException();
    }

    public void IUpdate()
    {
        throw new System.NotImplementedException();
    }
    public void ILateUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void IFixedUpdate()
    {
        throw new System.NotImplementedException();
    }
   
    #endregion

    #region 攻击和被击接口实现
    public void Atk(int index)
    {
        throw new System.NotImplementedException();
    }

    public void BeHit(ref DamageInfo damageInfo)
    {
        throw new System.NotImplementedException();
    }

    public DamageInfo GetDamageInfo()
    {
        throw new System.NotImplementedException();
    }

    public DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        throw new System.NotImplementedException();
    }


    public bool IsCanAtk()
    {
        throw new System.NotImplementedException();
    }

    public bool IsCanBeHit()
    {
        throw new System.NotImplementedException();
    }

    public void ShowAtkEffect(EffectInfo atkEffectInfo)
    {
        throw new System.NotImplementedException();
    }

    public void ShowBeHitEffect(EffectInfo hitEffectInfo)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}