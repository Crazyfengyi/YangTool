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
    /// 创建有限状态机
    /// </summary>
    /// <typeparam name="T">持有者类型</typeparam>
    /// <param name="_name">状态机名称</param>
    /// <param name="owner">状态机持有者</param>
    /// <param name="states">状态集合</param>
    public YangFsm<T> CreateFsm<T>(string _name, T owner, List<FsmStateBase<T>> states) where T : class
    {
        string keyName = typeof(T).FullName + _name;
        //if (HasFsm<T>(name))
        //{
        //}

        YangFsm<T> fsm = YangFsm<T>.Create(keyName, owner, states);
        Fsms.Add(keyName, fsm);
        return fsm;
    }
}

public class YangFsm
{
    protected string fsmName;
    /// <summary>
    /// 状态机名字
    /// </summary>
    public string FsmName => fsmName;
    public virtual void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
    {
    }
    public virtual void Close()
    {
    }
}
/// <summary>
/// 状态机
/// </summary>
/// <typeparam name="T"></typeparam>
public class YangFsm<T> : YangFsm
{
    private T handle;
    /// <summary>
    /// 持有者
    /// </summary>
    public T Handle => handle;
    //所有状态
    private List<FsmStateBase<T>> allStateList;
    //所有状态
    private Dictionary<Type, FsmStateBase<T>> allStateDic;
    //当前状态
    private FsmStateBase<T> currentState;

    public YangFsm(string _fsmName, T _handle, List<FsmStateBase<T>> stateList)
    {
        handle = _handle;
        allStateList = stateList;
        fsmName = _fsmName;
    }

    public override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
    {
        currentState.StateUpdate();
    }

    public override void Close()
    {

    }
    public void ChangeStateTo<StateType>() where StateType : FsmStateBase<T>
    {
        currentState?.StateClose();
        FsmStateBase<T> toState = null;
        for (int i = 0; i < allStateList.Count; i++)
        {
            FsmStateBase<T> item = allStateList[i];
            if (item.GetType() == typeof(StateType))
            {
                toState = item;
                break;
            }
        }
        if (toState != null) currentState = toState;
        currentState?.StateStart();
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

/// <summary>
/// 状态基类
/// </summary>
public abstract class FsmStateBase<T>
{
    /// <summary>
    /// 持有者
    /// </summary>
    private T handle;
    public T Handle => handle;

    private YangFsm<T> Fsm;
    /// <summary>
    /// 状态机
    /// </summary>
    public YangFsm<T> FSM
    {
        get => Fsm;
        set
        {
            Fsm = value;
        }
    }

    /// <summary>
    /// 状态初始化
    /// </summary>
    public virtual void StateInit(YangFsm<T> yangFsm)
    {
        FSM = yangFsm;
    }
    /// <summary>
    /// 状态开始
    /// </summary>
    public virtual void StateStart() { }
    /// <summary>
    /// 状态更新
    /// </summary>
    public virtual void StateUpdate() { }
    /// <summary>
    /// 状态关闭
    /// </summary>
    public virtual void StateClose() { }
    /// <summary>
    /// 切换状态
    /// </summary>
    protected void ChangeState<TState>() where TState : FsmStateBase<T>
    {
        Fsm.ChangeStateTo<TState>();
    }
}