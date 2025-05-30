/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-13 
*/
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using DataStruct;
using UnityEngine;
using UnityEngine.Serialization;
using YangTools.Log;

/// <summary>
/// BUFF控制器
/// </summary>
[Serializable]
public class BuffControl : ICustomLife
{
    /// <summary>
    /// 持有者
    /// </summary>
    [SerializeField]
    protected RoleBase handle;
    /// <summary>
    /// 事件关联器
    /// </summary>
    protected BuffEventRelation buffEventRelation = new BuffEventRelation();
    /// <summary>
    /// buff列表
    /// </summary>
    [ShowInInspector]
    private readonly List<BuffBase> buffList = new List<BuffBase>();

    public Action<BuffBase> OnBuffAddCallBack;
    public Action<BuffBase> OnBuffRemoveCallBack;
    /// <summary>
    /// 初始化
    /// </summary>
    public BuffControl(RoleBase _handle)
    {
        handle = _handle;
    }

    #region 生命周期
    public void IInit()
    {
        throw new NotImplementedException();
    }
    public void IUpdate()
    {
        for (var i = 0; i < buffList.Count; i++)
        {
            BuffBase buffBase = buffList[i];
            if (buffBase != null)
            {
                buffBase.Update();
                if (buffBase.IsEnd())
                {
                    buffBase.OnBuffRemove(this);
                    buffList.RemoveAt(i);
                    OnBuffRemove(buffBase);
                    buffBase.OnBuffDestroy(this);
                    i--;
                }
            }
        }
    }
    public void ILateUpdate()
    {
        throw new NotImplementedException();
    }
    public void IFixedUpdate()
    {
        throw new NotImplementedException();
    }
    public void IDie()
    {
    }
    public void IDestroy()
    {
    }
    #endregion

    #region 基础操作
    /// <summary>
    /// 添加buff
    /// </summary>
    public BuffBase Add(BuffID buffId, GameActor creator = null)
    {
        //Debuger.ToError($"获得Buff:{buffId}");
        //创建BUFF
        BuffBase buffBase = CreateBuff(creator == null ? handle : creator, handle, buffId);
        buffBase = AddBuff(buffBase);
        OnBuffAddCallBack?.Invoke(buffBase);
        return buffBase;
    }
    /// <summary>
    /// Buff添加时
    /// </summary>
    private BuffBase AddBuff(BuffBase newBuff)
    {
        newBuff.OnBuffAwake(this);
        for (var i = 0; i < buffList.Count; i++)
        {
            BuffBase skillBase = buffList[i];
            //根据已有的BUFF觉得最终添加类型
            BuffAppendType AppendType = skillBase.groupSetting.GetCombine(newBuff.groupSetting);
            switch (AppendType)
            {
                case BuffAppendType.Coexist:
                    continue;
                case BuffAppendType.Discard:
                    return null;
                case BuffAppendType.Refresh:
                    skillBase.OnBuffRefresh(this);
                    newBuff = skillBase;
                    goto end;
                case BuffAppendType.Overlay:
                    skillBase.OnBuffOverlay(this);
                    newBuff = skillBase;
                    goto end;
                case BuffAppendType.Replace:
                    skillBase.OnBuffReplace(this);
                    buffList.RemoveAt(i);
                    i--;
                    break;
            }
        }

        buffList.Add(newBuff);
        newBuff.OnBuffStart(this);
        AddBuffListener(newBuff);
        //发送事件
        if (handle.campType == ActorCampType.Player)
        {
            //发送buffList,用于UI显示--可以不用事件
        }

    //goto标签
    end:
        OnAddOtherBuff(handle, newBuff);
        return newBuff;
    }
    /// <summary>
    /// BUFF移除时
    /// </summary>
    private void OnBuffRemove(BuffBase buffBase, bool isCallOnRemove = true)
    {
        RemoveBuffListener(buffBase);
        if (isCallOnRemove)
        {
            OnRemoveOtherBuff(handle, buffBase.id);
        }
        OnBuffRemoveCallBack?.Invoke(buffBase);
    }
    /// <summary>
    /// 创建BUFF
    /// </summary>
    public static BuffBase CreateBuff(GameActor creator, GameActor target, BuffID buffId)
    {
        BuffBase temp = null;
        switch (buffId)
        {
            case BuffID.buff_10001:
                temp = new AttributeChange(creator, target, (int)buffId);
                break;
            default:
                Debuger.ToError($"BUFF生成失败:{buffId}");
                break;
        }
        return temp;
    }
    #endregion

    #region BUFF操作
    /// <summary>
    /// 清空BUFF
    /// </summary>
    public void ClearBuff()
    {
        for (var i = 0; i < buffList.Count; i++)
        {
            BuffBase buffBase = buffList[i];
            if (buffBase.buffFlagType == BuffFlagType.Common)
            {
                buffList.RemoveAt(i);
                OnBuffRemove(buffBase, false);
                i--;
            }
        }
    }
    /// <summary>
    /// 移除技能
    /// </summary>
    public void RemoveBuff(BuffID id)
    {
        for (var i = 0; i < buffList.Count; i++)
        {
            BuffBase buffBase = buffList[i];
            if (buffBase.id == id)
            {
                buffBase.OnBuffRemove(this);
                buffList.RemoveAt(i);
                OnBuffRemove(buffBase);
                buffBase.OnBuffDestroy(this);
                i--;
            }
        }
    }

    #endregion

    #region 获取值
    /// <summary>
    /// 获得是否有指定buff
    /// </summary>
    public bool GetHaveBuff(BuffID id)
    {
        for (int i = 0; i < buffList.Count; i++)
        {
            if (buffList[i].id == id)
            {
                return true;
            }
        }
        return false;
    }


    #endregion

    #region 事件绑定
    /// <summary>
    /// 添加BUFF事件到管理组里
    /// </summary>
    public void AddBuffListener(BuffBase buffBase)
    {
        foreach (BuffEventListenerBase buffEventListener in buffBase.allListeners)
        {
            buffEventRelation.AddEvent(buffEventListener.triggerPoint, buffEventListener.EventInvoke);
            buffEventListener.activeEvent += buffBase.ActiveInvoke;
            buffEventListener.deActiveEvent += buffBase.DeActiveInvoke;
            buffEventListener.IsCanActive = buffBase.buffEndChecker.IsCanEffective;
        }
        //触发添加事件
        foreach (BuffEventListenerBase buffListener in buffBase.allListeners)
        {
            if (buffListener.triggerPoint == BuffEventTriggerPoint.Add)
            {
                //事件参数初始化
                BuffEventArgBase buffEventArgBase = null;
                buffListener.EventInvoke(buffEventArgBase);
            }
        }
    }
    /// <summary>
    /// 从管理组里移除BUFF事件
    /// </summary>
    public void RemoveBuffListener(BuffBase buffBase)
    {
        foreach (BuffEventListenerBase buffEventListener in buffBase.allListeners)
        {
            //触发移除事件
            if (buffEventListener.triggerPoint == BuffEventTriggerPoint.Remove)
            {
                //事件参数初始化
                BuffEventArgBase buffEventArgBase = null;
                buffEventListener.EventInvoke(buffEventArgBase);
            }

            buffEventRelation.RemoveEvent(buffEventListener.triggerPoint, buffEventListener.EventInvoke);
            buffEventListener.activeEvent -= buffBase.ActiveInvoke;
            buffEventListener.deActiveEvent -= buffBase.DeActiveInvoke;
            buffEventListener.IsCanActive = null;
        }
    }

    #endregion

    #region 事件触发
    /// <summary>
    /// 获得新BUFF时
    /// </summary>
    private void OnAddOtherBuff(RoleBase trigger, BuffBase newBuff)
    {
        //TODO:添加参数
        buffEventRelation.EventInvoke(BuffEventTriggerPoint.AddOtherBuff, new BuffEventArgBase());
    }
    /// <summary>
    /// 移除BUFF时call
    /// </summary>
    private void OnRemoveOtherBuff(RoleBase trigger, BuffID otherId)
    {
        //TODO:添加参数
        buffEventRelation.EventInvoke(BuffEventTriggerPoint.RemoveOtherBuff, new BuffEventArgBase());
    }
    #endregion
}

/// <summary>
/// buff事件关联
/// </summary>
[Serializable]
public class BuffEventRelation
{
    /// <summary>
    /// buff事件委托
    /// </summary>
    /// <param name="buffEventArg">触发时传递的参数</param>
    public delegate void BuffEventDelegate(BuffEventArgBase buffEventArg);
    /// <summary>
    /// 全部事件点
    /// </summary>
    private readonly Dictionary<BuffEventTriggerPoint, BuffEventDelegate> allListener = new Dictionary<BuffEventTriggerPoint, BuffEventDelegate>();
    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="triggerPoint">触发点</param>
    /// <param name="eventCallBack">回调</param>
    public void AddEvent(BuffEventTriggerPoint triggerPoint, BuffEventDelegate eventCallBack)
    {
        if (allListener.ContainsKey(triggerPoint))
        {
            allListener[triggerPoint] += eventCallBack;
        }
        else
        {
            allListener.Add(triggerPoint, eventCallBack);
        }
    }
    /// <summary>
    /// 移除事件
    /// </summary>
    /// <param name="triggerPoint">触发点</param>
    /// <param name="eventCallBack">回调</param>
    public void RemoveEvent(BuffEventTriggerPoint triggerPoint, BuffEventDelegate eventCallBack)
    {
        if (allListener.ContainsKey(triggerPoint))
        {
            allListener[triggerPoint] -= eventCallBack;
            if (allListener[triggerPoint] == null)
            {
                allListener.Remove(triggerPoint);
            }
        }
    }
    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="triggerPoint">触发点</param>
    /// <param name="eventArg">参数</param>
    public void EventInvoke(BuffEventTriggerPoint triggerPoint, BuffEventArgBase eventArg)
    {
        if (allListener.TryGetValue(triggerPoint, out BuffEventDelegate @event))
        {
            @event?.Invoke(eventArg);
        }
    }
}
/// <summary>
/// buff事件参数基类(根据不同事件定义不同参数类型)
/// </summary>
public class BuffEventArgBase
{
    public bool deActive;//失效是否触发
    public RoleBase trigger;//触发者
    public RoleBase owner;//属于谁
}
/// <summary>
/// buff事件触发点
/// </summary>
public enum BuffEventTriggerPoint
{
    None,
    Add,
    Update,
    Remove,
    AddOtherBuff,
    RemoveOtherBuff,
}
/// <summary>
/// BUFF分组设置(为了区分优先级和处理排异)
/// </summary>
[Serializable]
public class BuffGroupSetting
{
    /// <summary>
    /// 分组
    /// </summary>
    public int group;
    /// <summary>
    /// 优先级
    /// </summary>
    public int priority;
    /// <summary>
    /// 同优先级的处理方式
    /// </summary>
    public BuffAppendType bufferAppendType;
    /// <summary>
    /// 根据分组与优先级返回结果
    /// </summary>
    /// <returns>最终叠加模式</returns>
    public BuffAppendType GetCombine(BuffGroupSetting newBuff)
    {
        if (newBuff.group != group)
        {
            return BuffAppendType.Coexist;
        }
        if (bufferAppendType == BuffAppendType.Coexist || newBuff.bufferAppendType == BuffAppendType.Coexist)
        {
            return BuffAppendType.Coexist;
        }
        if (newBuff.priority > priority)
        {
            return BuffAppendType.Replace;
        }
        if (newBuff.priority < priority)
        {
            return BuffAppendType.Discard;
        }

        return bufferAppendType;
    }
}
/// <summary>
/// buff事件监听器
/// </summary>
public abstract class BuffEventListenerBase
{
    /// <summary>
    /// 触发点
    /// </summary>
    public readonly BuffEventTriggerPoint triggerPoint;
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
    /// 触发次数
    /// </summary>
    public int triggerNum;
    /// <summary>
    /// 目标触发次数
    /// </summary>
    private int maxTriggerNum;
    /// <summary>
    /// 概率值
    /// </summary>
    private float randomTargetNum;
    /// <summary>
    /// 参数
    /// </summary>
    public BuffEventArgBase buffListenerParams;
    public BuffEventListenerBase(BuffEventTriggerPoint _triggerPoint)
    {
        triggerPoint = _triggerPoint;
        triggerNum = 0;
    }
    /// <summary>
    /// 初始化参数
    /// </summary>
    public virtual void InitArgs(/*BuffListenerConfig buffListenerConfig*/)
    {
        //maxTriggerNum = buffListenerConfig.ListeneredNumber;
        //randomTargetNum = buffListenerConfig.Random;
        //buffListenerParams = CreateParams();
        //buffListenerParams.ListenerInit(buffListenerConfig);
    }
    /// <summary>
    /// 触发事件
    /// </summary>
    public void EventInvoke(BuffEventArgBase eventArg)
    {
        if (IsCanActive != null && IsCanActive())
        {
            triggerNum++;
            if (IsListenerSuccess(eventArg))
            {
                triggerNum = 0;
                activeEvent?.Invoke(eventArg.trigger, eventArg.owner);
            }
            else
            {
                if (eventArg.deActive)
                {
                    deActiveEvent?.Invoke(eventArg.trigger, eventArg.owner);
                }
            }
        }
    }
    /// <summary>
    /// 是否触发
    /// </summary>
    public virtual bool IsListenerSuccess(BuffEventArgBase eventArg)
    {
        return triggerNum >= maxTriggerNum && UnityEngine.Random.Range(0f, 1f) < randomTargetNum;
    }

    ///// <summary>
    ///// 创建事件监听器
    ///// </summary>
    //public static BuffListerBase CreateListener(BuffListenerConfig listenerConfig)
    //{
    //    BuffListerBase temp = null;

    //    switch (listenerConfig.ListenerType)
    //    {
    //        case BuffLifePoint.添加时:
    //            temp = new BuffAddListener(listenerConfig.ListenerType);
    //            break;
    //        case BuffLifePoint.攻击时:
    //            temp = new AtkListener(listenerConfig.ListenerType);
    //            break;
    //        case BuffLifePoint.受伤后:
    //            temp = new AtkListener(listenerConfig.ListenerType);
    //            break;
    //        default:
    //            break;
    //    }

    //    temp.InitParams(listenerConfig);
    //    return temp;
    //}

    ///// <summary>
    ///// 条件
    ///// </summary>
    //public Condition condition;
    ///// <summary>
    ///// 事件(需要通过反射获取--显示界面可用枚举?)
    ///// </summary>
    //public Action action;
    ///// <summary>
    ///// 使用事件
    ///// </summary>
    //public bool InvokeAction()
    //{
    //    if (condition.GetIsSuccee())
    //    {
    //        action?.Invoke();
    //        return true;
    //    }
    //    return false;
    //}

}

/// <summary>
/// BUFF结束检测器
/// </summary>
[Serializable]
public class BuffEndChecker
{
    /// <summary>
    /// 生效冷却时间
    /// </summary>
    public RefreshValue cdTimer;
    protected BuffEndChecker(BuffConfig buffConfig)
    {
        cdTimer = new RefreshValue(buffConfig.cd, false);
    }
    /// <summary>
    /// 能否生效
    /// </summary>
    /// <returns></returns>
    public bool IsCanEffective()
    {
        return cdTimer.currentValue <= 0 && !IsEnd();
    }
    public virtual void Update(float tickTime)
    {
        cdTimer.CutDown(tickTime);
    }
    /// <summary>
    /// 是否生效
    /// </summary>
    public virtual bool IsActive()
    {
        return false;
    }
    /// <summary>
    /// 是否结束
    /// </summary>
    public virtual bool IsEnd()
    {
        return false;
    }
    /// <summary>
    /// 技能生效时
    /// </summary>
    public virtual void OnActive()
    {
        cdTimer.RefreshToMax();
    }
    /// <summary>
    /// 重置为最大值(倒计时)
    /// </summary>
    public virtual void Refresh()
    {
        cdTimer.RefreshToMax();
    }
    public static BuffEndChecker Create(BuffConfig buffConfig)
    {
        switch (buffConfig.buffEndType)
        {
            case BuffEndType.TimeOver:
                return new TimeEndChecker(buffConfig);
            case BuffEndType.CountOver:
                return new CountEndChecker(buffConfig);
            case BuffEndType.TimeOrCountOver:
                return new CountOrTimeEndChecker(buffConfig);
        }
        return null;
    }
}

/// <summary>
/// 刷新值
/// </summary>
[Serializable]
public class RefreshValue
{
    private float maxValue;//最大值
    public float currentValue;//当前值
    public RefreshValue(float value, bool isSetMax)
    {
        maxValue = value;
        currentValue = isSetMax ? maxValue : 0;
    }
    /// <summary>
    /// 值减少
    /// </summary>
    public void CutDown(float value)
    {
        currentValue -= value;
    }
    
    /// <summary>
    /// 重置为最大值
    /// </summary>
    public void RefreshToMax()
    {
        currentValue = maxValue;
    }
}