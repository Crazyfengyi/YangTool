/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[Serializable]
/// <summary>
/// buff基类
/// </summary>
public class BuffBase
{
    /// <summary>
    /// 技能描述
    /// </summary>
    [LabelText("Buff描述")]
    [MultiLineProperty]
    public string skillDescribe;
    /// <summary>
    /// buff创建者
    /// </summary>
    public RoleBase creator;

    [BoxGroup("基础数据")]
    /// <summary>
    /// buffID
    /// </summary>
    public int id;
    [BoxGroup("基础数据")]
    /// <summary>
    /// 图标
    /// </summary>
    public Texture2D icon;
    [BoxGroup("基础数据")]
    /// <summary>
    /// BUFF类型
    /// </summary>
    public BuffType type;
    [BoxGroup("基础数据")]
    /// <summary>
    /// 分组设置
    /// </summary>
    public BuffGroupSetting groupSetting;

    //BuffTypeId(Buff类型Id)， Caster（Buff施加者），Parent（Buff当前挂载的目标）,
    //Ability(Buff由哪个技能创建)，BuffLayer（层数）, BuffLevel（等级）BuffDuration（时长）
    //，BuffTag，BuffImmuneTag（免疫BuffTag）以及Context(Buff创建时的一些相关上下文数据)
    //等等。

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
    [SerializeField]
    /// <summary>
    /// 结束检查
    /// </summary>
    public BuffEndChecker buffEndChecker;

    #region 运行时用
    /// <summary>
    /// Buff施加者
    /// </summary>
    [HideInInspector] public string caster;
    /// <summary>
    /// Buff当前挂载的目标
    /// </summary>
    [HideInInspector] public string parent;
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
    public BuffBase(RoleBase _creator, int configId)
    {
        creator = _creator;
        BuffConfig data = GameResourceManager.Instance.GetBuffConfig(configId);
        Init(data);
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(BuffConfig buffConfig)
    {
        buffEndChecker = BuffEndChecker.Create(buffConfig);
        groupSetting = buffConfig.buffGroupSetting;
    }
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
    /// <param name="isHaveSameBuff">玩家是否有同类型buff</param>
    /// <returns>是否添加buff</returns>
    public virtual bool OnBuffAwake(BuffControl buffControl, bool isHaveSameBuff)
    {
        return false;
    }

    /// <summary>
    /// buff添加到玩家buff列表后
    /// </summary>
    public virtual void OnBuffStart()
    {
        //遍历eventDic字典下对应点的方法执行
    }

    /// <summary>
    /// 存在相同类型
    /// </summary>
    /// <returns>是否继续添加(手动处理返回false)</returns>
    public virtual bool OnBuffRefresh(BuffControl buffControl, BuffBase buff)
    {

        return false;
    }

    /// <summary>
    /// buff从玩家buff列表移除前
    /// </summary>
    public virtual void OnBuffRemove()
    {

    }

    /// <summary>
    /// buff从玩家buff列表移除后
    /// </summary>
    public virtual void OnBuffDestroy()
    {

    }

    #region 事件触发
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

    #endregion
}