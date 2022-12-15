/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[Serializable]
public class RoleBase : GameActor
{
    [ShowInInspector]
    protected BuffControl roleBuffControl;
    [ShowInInspector]
    protected RoleAttributeControl roleAttributeControl;
    [ShowInInspector]
    protected HealthControl healthControl;
    /// <summary>
    /// 血量脚本
    /// </summary>
    public HealthControl HealthControl => healthControl;
    [ShowInInspector]
    protected Dictionary<RoleFlag, float> flagkeyValue = new Dictionary<RoleFlag, float>();

    #region 生命周期接口实现
    public override void IInit()
    {
        roleBuffControl = new BuffControl(this);
        roleAttributeControl = new RoleAttributeControl(this);
        healthControl = new HealthControl(this, roleAttributeControl.GetAttribute(RoleAttribute.HP), () =>
        {
            GameActorManager.Instance.RemoveActor(this);
        });

        roleAttributeControl.ChangeAttribute(RoleAttribute.HP, 100);
        roleAttributeControl.ChangeAttribute(RoleAttribute.MP, 100);
        roleAttributeControl.ChangeAttribute(RoleAttribute.Atk, 30);
        roleAttributeControl.ChangeAttribute(RoleAttribute.Def, 20);
    }
    public override void IUpdate()
    {
        roleBuffControl?.IUpdate();
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
        healthControl?.IDestroy();
        GameActorManager.Instance.RemoveActor(this, false);
    }
    #endregion

    #region  角色属性
    /// <summary>
    /// 获得角色属性
    /// </summary>
    /// <param name="roleAttribute">属性枚举</param>
    public float GetRoleAttribute(RoleAttribute roleAttribute)
    {
        return roleAttributeControl.GetAttribute(roleAttribute).Value;
    }
    /// <summary>
    /// 角色属性更改
    /// </summary>
    public void RoleAttributeChange(RoleAttribute roleAttribute, AttributeValueType attributeValueType, float value)
    {
        roleAttributeControl.ChangeAttribute(roleAttribute, value, attributeValueType);
        //可以考虑去掉限制
        if (roleAttribute == RoleAttribute.HP)
        {
            healthControl?.healthBar?.UpdateData(this);
        }
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