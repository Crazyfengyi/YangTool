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
public class RoleBase : GameActor
{
    [ShowInInspector]
    public BuffControl roleBuffControl;
    [ShowInInspector]
    public RoleAttributeControl roleAttributeControl;
    [ShowInInspector]
    public HealthControl healthControl;
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
        healthControl = new HealthControl(this, roleAttributeControl.GetAttribute(RoleAttribute.HP), () =>
        {
            GameActorManager.Instance.ActorDie(this);
        });
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