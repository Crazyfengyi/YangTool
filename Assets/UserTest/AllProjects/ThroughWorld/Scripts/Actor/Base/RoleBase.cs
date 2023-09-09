/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using BehaviorDesigner.Runtime;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

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
    [HideInEditorMode]
    protected PlayableDirector timeLine;
    /// <summary>
    /// AI行为
    /// </summary>
    public BehaviorTree AIBehavior => aiBehavior;

    private AniControl aniControl;
    /// <summary>
    /// 动画控制器
    /// </summary>
    public AniControl AniControl => aniControl;
    /// <summary>
    /// 模型信息
    /// </summary>
    public ModelInfo modelInfo;

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
        aniControl = new AniControl(Animator);
        roleBuffControl = new BuffControl(this);
        roleAttributeControl = new RoleAttributeControl(this);
        healthControl = new HealthControl(this, roleAttributeControl.GetAttribute(RoleAttribute.HP), () =>
        {
            GameActorManager.Instance.RemoveActor(this);
        });

        aiBehavior = GetComponent<BehaviorTree>();
        timeLine = GetComponent<PlayableDirector>();
        if (aiBehavior) skillControl = new SkillControl(aiBehavior, timeLine);

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

    #region 特效
    /// <summary>
    /// 播放特效--挂载到身上
    /// </summary>
    /// <param name="effectName">特效名</param>
    /// <param name="effectPointType">挂载点</param>
    /// <param name="isLoop">是否循环</param>
    public GameObject PlayEffectAtSelf(string effectName, ModelPointType effectPointType = ModelPointType.Root, bool isLoop = false)
    {
        if (!modelInfo)
        {
            modelInfo = GetComponentInChildren<ModelInfo>(true);
        }

        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Effects/{effectName}");

        Vector3 tempPos = modelInfo.GetEffectPoint(effectPointType).position - modelInfo.GetEffectPoint(ModelPointType.Foot).position;
        GameObject effect = GameObject.Instantiate(obj, tempPos, Quaternion.identity, modelInfo.GetEffectPoint(ModelPointType.Root));

        //float angle = Vector3.Angle(modelInfo.Root.forward, Vector3.forward);
        //angle = modelInfo.Root.forward.x > 0 ? angle : -angle;
        //if (obj != null)
        //{
        //    obj.transform.Rotate(0, angle, 0, Space.World);
        //}
        //else
        //{
        //    Debug.LogError("�ҵ���Ч��" + effectName);
        //}

        if (isLoop)
        {
            //loopEffectList.Add(obj);
        }

        return obj;
    }

    /// <summary>
    /// 播放特效--不挂载
    /// </summary>
    public GameObject PlayEffect(string effectName, ModelPointType effectPointType = ModelPointType.Root, bool isLoop = false)
    {
        if (!modelInfo)
        {
            modelInfo = GetComponentInChildren<ModelInfo>(true);
        }
        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Effects/{effectName}");

        Vector3 tempPos = modelInfo.GetEffectPoint(effectPointType).position;
        GameObject effect = GameObject.Instantiate(obj, tempPos, Quaternion.identity);

        //float angle = Vector3.Angle(modelInfo.Root.forward, Vector3.forward);
        //angle = modelInfo.Root.forward.x > 0 ? angle : -angle;
        //if (obj != null)
        //{
        //    obj.transform.Rotate(0, angle, 0, Space.World);
        //}
        //else
        //{
        //    Debug.LogError("�ҵ���Ч��" + effectName);
        //}

        if (isLoop)
        {
            //loopEffectList.Add(effectObj);
        }

        return effect;
    }
    #endregion
}