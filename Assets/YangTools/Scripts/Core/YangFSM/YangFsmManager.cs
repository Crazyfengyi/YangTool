/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2023-02-15 
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using System.Collections.Generic;

/// <summary>
/// 状态机管理器
/// </summary>
public class YangFsmManager : GameModuleBase
{
    private readonly Dictionary<string, YangFsm> Fsms = new Dictionary<string, YangFsm>();
    private readonly List<YangFsm> tempFsms = new List<YangFsm>();

    internal override void InitModule()
    {
    }

    internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
    {
        tempFsms.Clear();
        if (Fsms.Count <= 0)
        {
            return;
        }

        foreach (KeyValuePair<string, YangFsm> fsm in Fsms)
        {
            tempFsms.Add(fsm.Value);
        }

        foreach (YangFsm fsm in tempFsms)
        {
            fsm.Update(delaTimeSeconds, unscaledDeltaTimeSeconds);
        }
    }

    internal override void CloseModule()
    {
        foreach (KeyValuePair<string, YangFsm> fsm in Fsms)
        {
            fsm.Value.Close();
        }

        Fsms.Clear();
        tempFsms.Clear();
    }

    /// <summary>
    /// 创建状态机
    /// </summary>
    /// <typeparam name="T">持有者类型</typeparam>
    /// <param name="name">状态机名称</param>
    /// <param name="handle">状态机持有者</param>
    /// <param name="states">状态集合</param>
    public YangFsm<T> CreateFsm<T>(string name, T handle, List<FsmStateBase<T>> states) where T : class
    {
        string keyName = name;
        if (Fsms.ContainsKey(keyName))
        {
            Debug.LogError($"重复创建状态机:{keyName}");
            return null;
        }

        YangFsm<T> fsm = YangFsm<T>.Create(keyName, handle, states);
        Fsms.Add(keyName, fsm);
        return fsm;
    }
    /// <summary>
    /// 获得状态机
    /// </summary>
    public YangFsm GetFsmFromName(string name)
    {
        if (Fsms.TryGetValue(name, out YangFsm fsm))
        {
            return fsm;
        }
        
        return null;
    }
}

/// <summary>
/// 状态基类
/// </summary>
public abstract class FsmStateBase<T>
{
    /// <summary>
    /// 状态机
    /// </summary>
    public YangFsm<T> Fsm { get; set; }

    /// <summary>
    /// 状态初始化
    /// </summary>
    public virtual void StateInit(YangFsm<T> fsm)
    {
        Fsm = fsm;
    }

    /// <summary>
    /// 状态开始
    /// </summary>
    public virtual void StateStart()
    {
    }

    /// <summary>
    /// 状态更新
    /// </summary>
    public virtual void StateUpdate()
    {
    }

    /// <summary>
    /// 状态关闭
    /// </summary>
    public virtual void StateClose()
    {
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    public void ChangeState<TState>() where TState : FsmStateBase<T>
    {
        Fsm.ChangeStateTo<TState>();
    }
}

/// <summary>
/// 状态机基类
/// </summary>
public abstract class YangFsm
{
    /// <summary>
    /// 状态机名字
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 状态机全名字
    /// </summary>
    public string FullName { get; set; }

    public virtual void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
    {
    }

    public virtual void Close()
    {
    }
}

/// <summary>
/// 状态机泛型类
/// </summary>
public class YangFsm<T> : YangFsm
{
    /// <summary>
    /// 持有者
    /// </summary>
    public T Handle { get; set; }

    //所有状态
    private List<FsmStateBase<T>> allStateList;

    //所有状态
    private Dictionary<Type, FsmStateBase<T>> allStateDic;

    //当前状态
    private FsmStateBase<T> currentState;

    private YangFsm(string _Name, T _handle, List<FsmStateBase<T>> stateList)
    {
        Name = _Name;
        FullName = typeof(T).Name + Name;
        Handle = _handle;
        allStateList = stateList;
    }

    public override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
    {
        currentState.StateUpdate();
    }

    public override void Close()
    {
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <typeparam name="TStateType">目标状态</typeparam>
    public void ChangeStateTo<TStateType>() where TStateType : FsmStateBase<T>
    {
        currentState?.StateClose();
        FsmStateBase<T> toState = null;
        foreach (var item in allStateList)
        {
            if (item.GetType() == typeof(TStateType))
            {
                toState = item;
                break;
            }
        }

        if (toState != null) currentState = toState;
        currentState?.StateStart();
    }

    /// <summary>
    /// 当前状态是否为目标状态
    /// </summary>
    /// <typeparam name="TStateType">目标状态</typeparam>
    public bool CurrentStataIsTarget<TStateType>() where TStateType : FsmStateBase<T>
    {
        Type temp = currentState.GetType();
        if (typeof(TStateType) == temp)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 创建状态机
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="owner">持有者</param>
    /// <param name="states">状态集合</param>
    public static YangFsm<T> Create(string name, T owner, List<FsmStateBase<T>> states)
    {
        if (owner == null) return null;
        if (states == null || states.Count < 1) return null;

        YangFsm<T> fsm = new YangFsm<T>(name, owner, states);
        foreach (FsmStateBase<T> state in states)
        {
            if (state == null)
            {
                throw new Exception("状态是空的");
            }

            Type stateType = state.GetType();
            if (fsm.allStateDic.ContainsKey(stateType))
            {
                throw new Exception($"重复状态:{stateType.Name}");
            }

            fsm.allStateDic.Add(stateType, state);
            state.StateInit(fsm);
        }

        return fsm;
    }
}