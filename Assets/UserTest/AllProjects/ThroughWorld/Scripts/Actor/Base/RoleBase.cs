/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using BehaviorDesigner.Runtime;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class RoleBase : GameActor
{
    [HideInEditorMode]
    [ShowInInspector]
    protected BuffControl roleBuffControl;
    [HideInEditorMode]
    [ShowInInspector]
    protected RoleAttributeControl roleAttributeControl;
    [HideInEditorMode]
    [ShowInInspector]
    protected HealthControl healthControl;
    /// <summary>
    /// 血量脚本
    /// </summary>
    public HealthControl HealthControl => healthControl;
    [ShowInInspector]
    protected Dictionary<RoleFlag, float> flagkeyValue = new Dictionary<RoleFlag, float>();
    [HideInEditorMode]
    protected BehaviorTree aiBehavior;
    /// <summary>
    /// AI行为
    /// </summary>
    public BehaviorTree AIBehavior => aiBehavior;
    [HideInEditorMode]
    protected SkillControl skillControl;
    /// <summary>
    /// 技能控制器
    /// </summary>
    public SkillControl SkillControl => skillControl;

    #region 攻击目标
    protected GameObject target;
    /// <summary>
    /// 目标物体
    /// </summary>
    public override GameObject Target
    {
        get { return target; }
    }
    protected Vector3? targetPos;
    /// <summary>
    /// 目标点
    /// </summary>
    public override Vector3? TargetPos
    {
        get { return targetPos; }
    }
    #endregion

    #region 事件回调
    public Action OnDieAction;
    #endregion

    #region 初始化记录
    private Vector3 startPos;
    /// <summary>
    /// 初始点
    /// </summary>
    public Vector3 StartPos => startPos;

    public void InitRecord()
    {
        startPos = transform.position;
    }

    #endregion

    #region 生命周期接口实现
    public override void IInit()
    {
        roleBuffControl = new BuffControl(this);
        roleAttributeControl = new RoleAttributeControl(this);
        healthControl = new HealthControl(this, roleAttributeControl.GetAttribute(RoleAttribute.HP), () =>
        {
            GameActorManager.Instance.RemoveActor(this);
        });

        aiBehavior = GetComponent<BehaviorTree>();
        if (aiBehavior) skillControl = new SkillControl(aiBehavior);

        roleAttributeControl.ChangeAttribute(RoleAttribute.HP, 1000);
        roleAttributeControl.ChangeAttribute(RoleAttribute.MP, 1000);
        roleAttributeControl.ChangeAttribute(RoleAttribute.Atk, 30);
        roleAttributeControl.ChangeAttribute(RoleAttribute.Def, 20);
        InitRecord();
    }

    public override void IUpdate()
    {
        roleBuffControl?.IUpdate();
        SearchAtkTarget();
    }
    public override void ILateUpdate()
    {

    }
    public override void IFixedUpdate()
    {

    }
    public override void IDie()
    {
        OnDieAction?.Invoke();
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

    #region 搜索攻击目标
    /// <summary>
    /// 搜索攻击目标
    /// </summary>
    public virtual void SearchAtkTarget()
    {
        Collider[] temp = Physics.OverlapSphere(transform.position, roleAttributeControl.GetAttribute(RoleAttribute.AtkRang).Value);
        if (temp.Length > 0)
        {
            for (int i = 0; i < temp.Length; i++)
            {
                GameActor tempTarget = temp[i].gameObject.GetComponentInParent<GameActor>();
                if (tempTarget && canAtkCamp.HasFlag(tempTarget.campType))
                {
                    target = tempTarget.gameObject;
                    targetPos = tempTarget.transform.position;
                }
            }
        }
        else
        {
            target = null;
            targetPos = null;
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