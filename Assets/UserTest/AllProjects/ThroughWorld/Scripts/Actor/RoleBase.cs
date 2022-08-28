/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[Serializable]
public class RoleBase : GameActor, IAtker, IBeHit
{
    [ShowInInspector]
    public BuffControl roleBuffControl;
    [ShowInInspector]
    public RoleAttributeControl roleAttributeControl;
    [ShowInInspector]
    public Dictionary<RoleFlag, float> flagkeyValue = new Dictionary<RoleFlag, float>();

    #region 生命周期接口实现
    public override void IInit()
    {
        roleBuffControl = new BuffControl();
        roleBuffControl.Init(this);

        roleAttributeControl = new RoleAttributeControl();
        roleAttributeControl.Init(this);
        roleAttributeControl.SetAttribute(RoleAttribute.HP, 100);
        roleAttributeControl.SetAttribute(RoleAttribute.MP, 100);
    }
    public override void IDie()
    {

    }
    public override void IUpdate()
    {
        if (roleBuffControl != null)
        {
            roleBuffControl.IUpdate();
        }
    }
    public override void ILateUpdate()
    {

    }
    public override void IFixedUpdate()
    {

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

    #region 标记值操作
    public float GetFlag(RoleFlag roleFlag, float defaultValue = 0)
    {
        if (flagkeyValue.TryGetValue(roleFlag, out var flagvalue))
        {
            return flagvalue;
        }
        else
        {
            flagkeyValue.Add(roleFlag, defaultValue);
            return defaultValue;
        }
    }
    public void SetFlag(RoleFlag roleFlag, float value)
    {
        if (flagkeyValue.ContainsKey(roleFlag))
        {
            flagkeyValue[roleFlag] = value;
        }
        else
        {
            flagkeyValue.Add(roleFlag, value);
        }
    }
    #endregion
}