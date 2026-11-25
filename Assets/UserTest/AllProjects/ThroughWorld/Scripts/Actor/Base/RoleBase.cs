/*
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using BehaviorDesigner.Runtime;
using DataStruct;
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
    public BuffControl roleBuffControl;
    [HideInEditorMode]
    [ShowInInspector]
    public RoleAttributeControl roleAttributeControl;
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
    [SerializeField, LabelText("AI调度行为树")]
    protected BehaviorTree aiBehavior;
    // 技能执行树与 AI 调度树分别配置
    [SerializeField, LabelText("技能执行行为树")]
    private BehaviorTree skillBehavior;
    // 等待 AI 开始执行的最长秒数
    [SerializeField, Min(0.1f), LabelText("技能调度等待时限（秒）")]
    private float skillRequestTimeout = 5f;
    // 实际执行结束后才开始计算的间隔
    [SerializeField, Min(0), LabelText("技能公共间隔（秒）")]
    private float skillCommonInterval = 2f;
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
    public ModelInfo ModelInfo { get; set; }

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
        healthControl = new HealthControl(this, roleAttributeControl.GetAttribute(RoleAttribute.Hp), () =>
        {
            GameActorManager.Instance.RemoveActor(this);
        });

        if (!ModelInfo)
        {
            ModelInfo = GetComponentInChildren<ModelInfo>(true);
        }

        InitializeSkillControl();

        InitRecord();
    }

    /// <summary>
    /// 初始化技能执行引用 并兼容旧预制体中未显式绑定的双行为树
    /// </summary>
    protected void InitializeSkillControl()
    {
        skillControl?.BreakSkill();
        if (aiBehavior == null) aiBehavior = GetComponent<BehaviorTree>();
        if (skillBehavior == null)
        {
            foreach (BehaviorTree tree in GetComponents<BehaviorTree>())
            {
                if (tree != aiBehavior)
                {
                    skillBehavior = tree;
                    break;
                }
            }
        }
        timeLine = GetComponent<PlayableDirector>();
        skillControl = new SkillControl(this, skillBehavior, timeLine);
        skillControl.Configure(skillRequestTimeout, skillCommonInterval);
    }

    /// <summary>
    /// 冷却与调度等待不依赖正式管理器的寻路或选敌分支
    /// </summary>
    protected virtual void Update()
    {
        skillControl?.Tick(Time.deltaTime);
    }

    /// <summary>
    /// 停用角色时停止技能并中止其 AI 调度
    /// </summary>
    protected virtual void OnDisable()
    {
        StopRoleSkills();
    }

    /// <summary>
    /// 直接销毁物体时也保证技能退出
    /// </summary>
    protected virtual void OnDestroy()
    {
        StopRoleSkills();
    }

    /// <summary>
    /// 幂等清理技能与 AI 调度树
    /// </summary>
    private void StopRoleSkills()
    {
        skillControl?.BreakSkill();
        if (aiBehavior != null) aiBehavior.DisableBehavior(false);
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
        StopRoleSkills();
        OnDieAction?.Invoke();
    }
    public override void IDestroy()
    {
        StopRoleSkills();
        healthControl?.IDestroy();
        GameActorManager.Instance.RemoveActor(this, false);
    }
    #endregion

    #region  角色属性
    /// <summary>
    /// 获得角色属性
    /// </summary>
    /// <param name="roleAttribute">属性枚举</param>
    public ValueTotal GetRoleAttribute(RoleAttribute roleAttribute)
    {
        return roleAttributeControl.GetAttribute(roleAttribute);
    }
    /// <summary>
    /// 获得角色属性值
    /// </summary>
    /// <param name="roleAttribute">属性枚举</param>
    public float GetRoleAttributeValue(RoleAttribute roleAttribute)
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
        if (roleAttribute == RoleAttribute.Hp)
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
        Collider[] temp = Physics.OverlapSphere(transform.position, roleAttributeControl.GetAttribute(RoleAttribute.GuardRang).Value);
        if (temp.Length > 0)
        {
            bool haveTarget = false;
            for (int i = 0; i < temp.Length; i++)
            {
                GameActor tempTarget = temp[i].gameObject.GetComponentInParent<GameActor>();
                if (tempTarget && canAtkCamp.HasFlag(tempTarget.campType))
                {
                    haveTarget = true;
                    target = tempTarget.gameObject;
                    targetPos = tempTarget.transform.position;
                }
            }
            if (!haveTarget)
            {
                target = null;
                targetPos = null;
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
        if (!ModelInfo)
        {
            ModelInfo = GetComponentInChildren<ModelInfo>(true);
        }

        GameObject obj = GameResourceManager.Instance.ResourceLoad($"Effects/{effectName}");

        Vector3 tempPos = ModelInfo.GetEffectPoint(effectPointType).position - ModelInfo.GetEffectPoint(ModelPointType.Foot).position;
        GameObject effect = GameObject.Instantiate(obj, tempPos, Quaternion.identity, ModelInfo.GetEffectPoint(ModelPointType.Root));
        effect.transform.forward = ModelInfo.Root.forward;

        //float angle = Vector3.Angle(modelInfo.Root.forward, Vector3.forward);
        //angle = modelInfo.Root.forward.x > 0 ? angle : -angle;
        //if (effect != null)
        //{
        //    effect.transform.Rotate(0, angle, 0, Space.World);
        //}
        //else
        //{
        //    Debug.LogError("生成特效失败:" + effectName);
        //}
        //Mathf.MoveTowards();
        //Vector3.MoveTowards();
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
        if (!ModelInfo)
        {
            ModelInfo = GetComponentInChildren<ModelInfo>(true);
        }
        GameObject obj = GameResourceManager.Instance.ResourceLoad($"Effects/{effectName}");

        Vector3 tempPos = ModelInfo.GetEffectPoint(effectPointType).position;
        GameObject effect = GameObject.Instantiate(obj, tempPos, Quaternion.identity);
        effect.transform.forward = ModelInfo.Root.forward;
        //float angle = Vector3.Angle(modelInfo.Root.forward, Vector3.forward);
        //angle = modelInfo.Root.forward.x > 0 ? angle : -angle;
        //if (effect != null)
        //{
        //    effect.transform.Rotate(0, angle, 0, Space.World);
        //}
        //else
        //{
        //    Debug.LogError("生成特效失败:" + effectName);
        //}

        if (isLoop)
        {
            //loopEffectList.Add(effectObj);
        }

        return effect;
    }
    
    #endregion

    #region 攻击和被击接口实现

    public override void Atk(AtkInfo atkInfo)
    {
        if (!IsCanAtk()) return;

        if (atkInfo.targetActor.IsCanBeHit())
        {
            GameBattleManager.Instance.HitProcess(atkInfo.damageInfo, atkInfo.targetActor);
            ShowAtkEffect(atkInfo.atkEffectInfo);
        }
    }

    public override void BeHit(ref DamageInfo damageInfo)
    {
        healthControl.MinusHp(damageInfo);
    }

    public override DamageInfo GetDamageInfo()
    {
        //伤害信息
        var result = new DamageInfo();
        if (SkillControl != null && SkillControl.CurrentRunTimeSkillData != null)
        {
            result.damage = SkillControl.CurrentRunTimeSkillData.skill.Atk.Percent * GetRoleAttributeValue(RoleAttribute.Atk);
            result.damage += SkillControl.CurrentRunTimeSkillData.skill.Atk.Num;
        }
        else
        {
            Debug.LogError($"技能数据为null:{SkillControl == null}:{SkillControl?.CurrentRunTimeSkillData == null}");
        }

        return result; 
    }

    public override DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        damageInfo.damage = damageInfo.damage - roleAttributeControl.GetAttribute(RoleAttribute.Def).Value;
        damageInfo.damage = Mathf.Max(damageInfo.damage, 0);
        return damageInfo;
    }

    public override bool IsCanAtk()
    {
        return isActiveAndEnabled && (healthControl == null || healthControl.GetCurrentHp() > 0);
    }

    public override bool IsCanBeHit()
    {
        return isActiveAndEnabled && (healthControl == null || healthControl.GetCurrentHp() > 0);
    }

    public override void ShowAtkEffect(EffectInfo atkEffectInfo)
    {
    }

    public override void ShowBeHitEffect(EffectInfo hitEffectInfo)
    {
    }

    #endregion 攻击和被击接口实现

    #region 伤害检测
    /// <summary>
    /// 伤害检测
    /// </summary>
    /// <param name="checkConfig"></param>
    public bool DamageCheck(CheckConfig checkConfig)
    {
        if (skillControl == null || !skillControl.IsExecuting || !IsCanAtk()) return false;
        PhysicsCheckManager.Instance.PhysicCheck(checkConfig, this, out Collider[] array);

        for (int i = 0; i < array.Length; i++)
        {
            GameActor script = array[i].GetComponentInChildren<GameActor>(true);
            if (script)
            {
                DamageInfo damageInfo = GetDamageInfo();
                damageInfo.atkPos = transform.position;

                AtkInfo atkInfo = new AtkInfo();
                atkInfo.sourceActor = this;
                atkInfo.targetActor = script;
                atkInfo.damageInfo = damageInfo;
                atkInfo.atkEffectInfo = null;
                Atk(atkInfo);
            }
        }
        //Debug.LogError($"伤害检测:{array.Length}");
        return array.Length > 0;
    }

    #endregion
}
