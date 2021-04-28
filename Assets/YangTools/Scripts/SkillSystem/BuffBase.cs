/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public abstract class BuffBase : ScriptableObject
{
    #region 属性
    /// <summary>
    /// buff类型
    /// </summary>
    public BuffType buffTypeId;
    /// <summary>
    /// 技能描述
    /// </summary>
    [LabelText("Buff描述")]
    [MultiLineProperty]
    public string skillDescribe;

    public Texture2D icon;
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
    /// 层数
    /// </summary>
    [HideInInspector] public int buffLayer;
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
    /// 初始化
    /// </summary>
    public abstract void Init(object data);

    #region 生命周期
    /// <summary>
    /// buff添加到玩家buff列表前
    /// </summary>
    /// <param name="isHaveSameBuff">玩家是否有同类型buff</param>
    /// <returns>是否添加buff</returns>
    public virtual bool OnBuffAwake(BuffManager buffManager, bool isHaveSameBuff)
    {
        return false;
    }
    /// <summary>
    /// buff添加到玩家buff列表后
    /// </summary>
    public virtual void OnBuffStart()
    {

    }

    /// <summary>
    /// 存在相同类型且为相同来源(caster:施加者)
    /// </summary>
    public virtual void OnBuffRefresh()
    {

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

    //==================以下是监听===============
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