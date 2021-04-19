/** 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION# 
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-06-14 
*/
using System.Collections.Generic;

/// <summary>
/// 事件监听器
/// </summary>
public class YangToolEventListener
{
    /// <summary>
    /// 单例
    /// </summary>
    private static YangToolEventListener instance;
    /// <summary>
    /// 线程锁
    /// </summary>
    private static object eventLock = new object();
    /// <summary>
    /// 单例
    /// </summary>
    public static YangToolEventListener Instance
    {
        get
        {
            if (instance == null)
            {
                lock (eventLock)
                {
                    if (instance == null)
                    {
                        instance = new YangToolEventListener();
                    }
                }
            }
            return instance;
        }
    }

    #region 可以绑定对象的事件管理器
    /// <summary>
    /// 事件字典
    /// </summary>
    public readonly Dictionary<string, List<EventInspector>> eventDic = new Dictionary<string, List<EventInspector>>();

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="eventListener"></param>
    public void Add(EventInspector eventListener)
    {
        if (eventDic.TryGetValue(eventListener.eventName, out List<EventInspector> toAdd))
        {
            toAdd.Add(eventListener);
        }
        else
        {
            toAdd = new List<EventInspector>();
            toAdd.Add(eventListener);
            eventDic.Add(eventListener.eventName, toAdd);
        }
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    public void Remove(string eventName)
    {
        if (eventDic.TryGetValue(eventName, out List<EventInspector> list))
        {
            eventDic.Remove(eventName);
        }
    }

    /// <summary>
    /// 移除一个监听
    /// </summary>
    public void Remove(EventInspector eventInspector)
    {
        foreach (var i in eventDic)
        {
            i.Value.Remove(eventInspector);
        }
    }

    /// <summary>
    /// 移除一个对象上的监听
    /// </summary>
    public void Remove(UnityEngine.Object target)
    {
        foreach (var i in eventDic)
        {
            for (int k = i.Value.Count - 1; k >= 0; k--)
            {
                EventInspector v = i.Value[k];
                if (ReferenceEquals(v.Holder, target))
                {
                    i.Value.RemoveAt(k);
                    return;
                }
            }
        }
    }

    /// <summary> 发送事件 </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="eventArgs">参数列表</param>
    public void Send(string eventName, params object[] eventArgs)
    {
        if (eventDic.TryGetValue(eventName, out List<EventInspector> list))
        {
            EventInfo e = new EventInfo(eventName, eventArgs);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                EventInspector item = list[i];

                if (item.isEnabled)
                {
                    if (!item.Invoke(e))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 清理所有事件监听器
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }

    #endregion
}

#region 事件相关类
/// <summary>
/// 委托
/// </summary>
/// <typeparam name="T">参数类型</typeparam>
/// <param name="arg">参数</param>
public delegate void EventCallback<T>(T arg);

/// <summary>
/// 事件检查器
/// </summary>
//[System.Serializable]
public class EventInspector
{
    /// <summary>
    /// 事件名称
    /// </summary>
    public string eventName;
    /// <summary>
    /// 事件
    /// </summary>
    public EventCallback<EventInfo> callback;
    /// <summary>
    /// 弱引用绑定的物体
    /// </summary>
    public System.WeakReference<UnityEngine.Object> weakObjectTagrt;
    /// <summary>
    /// 是否可以使用事件--弱引用目标是否存活(存活--可以使用，不存活--不可使用)
    /// </summary>
    public bool CanUse
    {
        get
        {
            UnityEngine.Object nowObj = null;
            weakObjectTagrt.TryGetTarget(out nowObj);
            return nowObj != null;
        }
    }
    /// <summary>
    /// 获取绑定的物体
    /// </summary>
    public UnityEngine.Object Holder
    {
        get
        {
            UnityEngine.Object nowObj = null;
            weakObjectTagrt.TryGetTarget(out nowObj);
            return nowObj;
        }
    }
    /// <summary>
    /// 可用状态
    /// </summary>
    public bool isEnabled = true;

    /// <summary>
    /// 创建事件检查者
    /// </summary>
    /// <param name="_holder">(谁持有检查器)检查器绑定某个物体上(只要继承自UnityObject就行了),null表示全局监听</param>
    /// <param name="_eventName">事件名称</param>
    /// <param name="_eventCallback">事件回调</param>
    public EventInspector(UnityEngine.Object _holder, string _eventName, EventCallback<EventInfo> _eventCallback)
    {
        eventName = _eventName;
        weakObjectTagrt = new System.WeakReference<UnityEngine.Object>(_holder);
        callback = _eventCallback;
    }

    /// <summary>
    /// 调用事件（返回调用是否成功）
    /// </summary>
    public bool Invoke(EventInfo e)
    {
        if (Holder == null) return false;

        callback?.Invoke(e);
        return true;
    }
}

/// <summary>
/// 事件信息
/// </summary>
public class EventInfo
{
    /// <summary>
    /// 事件名称
    /// </summary>
    public string name { get; private set; }
    /// <summary>
    /// 事件参数
    /// </summary>
    public object[] args { get; private set; }

    /// <summary>
    ///构造方法
    /// </summary>
    /// <param name="_name">事件名称</param>
    /// <param name="_args">事件参数</param>
    public EventInfo(string _name, params object[] _args)
    {
        name = _name;
        args = _args;
    }
}
#endregion