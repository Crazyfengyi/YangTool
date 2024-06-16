/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff基类
/// </summary>
[Serializable]
public abstract class BuffBase
{
    /// <summary>
    /// 技能描述
    /// </summary>
    [LabelText("Buff描述")]
    [MultiLineProperty]
    public string skillDescribe;
    /// <summary>
    /// 创建者
    /// </summary>
    public GameActor creator;
    /// <summary>
    /// 挂载的目标
    /// </summary>
    public GameActor target;
    /// <summary>
    /// buffID
    /// </summary>
    [BoxGroup("基础数据")]
    public BuffID id;
    /// <summary>
    /// 图标
    /// </summary>
    [BoxGroup("基础数据")]
    public Sprite icon;
    /// <summary>
    /// BUFF类型
    /// </summary>
    [BoxGroup("基础数据")]
    public BuffFlagType buffFlagType;
    /// <summary>
    /// 分组设置
    /// </summary>
    [BoxGroup("基础数据")]
    public BuffGroupSetting groupSetting;
    /// <summary>
    /// 生效事件
    /// </summary>
    public event ActiveInvokeDelegate activeEvent;
    /// <summary>
    /// 失效事件
    /// </summary>
    public event DeActiveInvokeDelegate deActiveEvent;
    /// <summary>
    /// 是否可以生效回调
    /// </summary>
    public Func<bool> IsCanActive;
    /// <summary>
    /// 结束检查
    /// </summary>
    [SerializeField]
    public BuffEndChecker buffEndChecker;

    #region 运行时用
    /// <summary>
    /// Buff由哪个技能创建
    /// </summary>
    [HideInInspector] public int ability;
    /// <summary>
    /// buff状态
    /// </summary>
    [HideInInspector] public BuffState buffState;
    /// <summary>
    /// 层数
    /// </summary>
    [HideInInspector] public int buffLayer;
    #endregion

    #region 表格相关数据
    /// <summary>
    /// 等级
    /// </summary>
    [BoxGroup("数据")]
    [LabelText("Buff等级")]
    public int buffLevel;
    /// <summary>
    /// 时长
    /// </summary>
    [BoxGroup("数据")]
    [LabelText("持续时间")]
    public int buffDuration;
    /// <summary>
    /// 检测间隔
    /// </summary>
    [BoxGroup("数据")]
    [LabelText("检测间隔")]
    public float interval;
    /// <summary>
    /// 是否为增益buff
    /// </summary>
    [BoxGroup("数据")]
    [LabelText("是增益buff")]
    public bool IsBuff = true;
    /// <summary>
    /// 特效路径
    /// </summary>
    [BoxGroup("数据")]
    [LabelText("特效路径")]
    public string effectsPath;
    /// <summary>
    /// 标签--属性？
    /// </summary>
    [BoxGroup("数据")]
    [LabelText("标签")]
    public string buffTag;
    /// <summary>
    /// 免疫BuffTag
    /// </summary>
    [BoxGroup("数据")]
    [LabelText("免疫标签")]
    public string buffImmuneTag;
    #endregion

    /// <summary>
    /// 所有事件
    /// </summary>
    public List<BuffEventListenerBase> allListeners = new List<BuffEventListenerBase>();
    public BuffBase(GameActor _creator, GameActor _target, int configId)
    {
        creator = _creator;
        target = _target;
        BuffConfig data = GameResourceManager.Instance.GetBuffConfig(configId);
        Init(data);
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(BuffConfig buffConfig)
    {
        icon = buffConfig.icon;
        buffEndChecker = BuffEndChecker.Create(buffConfig);
        groupSetting = buffConfig.buffGroupSetting;
        skillDescribe = buffConfig.des; 
    }

    #region 生命周期
    /// <summary>
    /// 更新
    /// </summary>
    public virtual void Update()
    {
        buffEndChecker.Update(Time.deltaTime);
    }
    /// <summary>
    /// 是否结束
    /// </summary>
    public virtual bool IsEnd()
    {
        return buffEndChecker.IsEnd();
    }

    /// <summary>
    /// buff添加到玩家buff列表前
    /// </summary>
    public virtual void OnBuffAwake(BuffControl buffControl)
    {
        OnAwake();
    }
    protected abstract void OnAwake();
    /// <summary>
    /// buff添加到玩家buff列表后
    /// </summary>
    public virtual void OnBuffStart(BuffControl buffControl)
    {
        OnStart();
    }
    protected abstract void OnStart();
    /// <summary>
    /// 存在相同类型--刷新时
    /// </summary>
    public virtual void OnBuffRefresh(BuffControl buffControl)
    {
        OnRefresh();
    }
    protected abstract void OnRefresh();
    /// <summary>
    /// 存在相同类型--叠加时
    /// </summary>
    public virtual void OnBuffOverlay(BuffControl buffControl)
    {
        OnOverlay();
    }
    protected abstract void OnOverlay();
    /// <summary>
    /// 存在相同类型--替换时
    /// </summary>
    public virtual void OnBuffReplace(BuffControl buffControl)
    {
        OnReplace();
    }
    protected abstract void OnReplace();
    /// <summary>
    /// buff从玩家buff列表移除前
    /// </summary>
    public virtual void OnBuffRemove(BuffControl buffControl)
    {
        OnRemove();
    }
    protected abstract void OnRemove();
    /// <summary>
    /// buff从玩家buff列表移除后
    /// </summary>
    public virtual void OnBuffDestroy(BuffControl buffControl)
    {
        OnDestroy();
    }
    protected abstract void OnDestroy();
    #endregion

    #region 生效失效固定函数
    /// <summary>
    /// 生效
    /// </summary>
    public abstract void TakeEffect();
    /// <summary>
    /// 失效
    /// </summary>
    public abstract void LoseEffect();
    #endregion

    #region 事件触发
    /// <summary>
    /// buff生效事件
    /// </summary>
    public virtual void ActiveInvoke(RoleBase trigger, RoleBase owner)
    {
        //if (!isEffective || isCanAgainEffective)
        //{
        //    buffEndCheck.OnEffective();

        //    ChangeBuffCanUseTimesEffect();
        //    EffectiveEventHandler?.Invoke(trigger, owner, buffLayer);
        //    isEffective = true;
        //}
    }
    /// <summary>
    /// buff失效事件
    /// </summary>
    public virtual void DeActiveInvoke(RoleBase trigger, RoleBase owner)
    {
        //if (isEffective)
        //{
        //    if (buffConfig.endIsRemove)
        //        LoseEffectiveEventHandler?.Invoke();
        //    isEffective = false;
        //}
    }
    /// <summary>
    /// 技能释放
    /// </summary>
    public virtual void OnSkillExecuted()
    {

    }
    /// <summary>
    /// 攻击前(TODO：子弹创建时?,或者计算伤害前)
    /// </summary>
    public virtual void OnBeforeAttack(string damageData)
    {

    }
    /// <summary>
    /// 攻击后(子弹造成伤害后)
    /// </summary>
    public virtual void OnAfterAttack(string damageData)
    {

    }
    /// <summary>
    /// 受伤前
    /// </summary>
    public virtual void OnBeforeBeHit(string damageData)
    {

    }
    /// <summary>
    /// 受伤后
    /// </summary>
    public virtual void OnAfterBeHit(string damageData)
    {

    }
    /// <summary>
    /// 死亡前
    /// </summary>
    public virtual void OnBeforeDead()
    {

    }
    /// <summary>
    /// 死亡后
    /// </summary>
    public virtual void OnAfterDead()
    {

    }
    /// <summary>
    /// 击杀目标后
    /// </summary>
    public virtual void OnKillTarget(string damageData)
    {

    }
    #endregion
}