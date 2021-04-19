using System;
using System.Collections;
using System.Collections.Generic;
using TimerStruct;

public static class Z45Timer
{
	/// <summary>
	/// 存储所有的定时器
	/// </summary>
	private static readonly Dictionary<long, SingleTimer> allTimers = new Dictionary<long, SingleTimer>();
	/// <summary>
	/// 根据时间流逝类型分类的所有计时器
	/// </summary>
	private static readonly Dictionary<TimeTickType, List<SingleTimer>> singleTimerDic = new Dictionary<TimeTickType, List<SingleTimer>>();
	/// <summary>
	/// 不同类型时间流逝速度的计时器的当前类型流逝速度
	/// </summary>
	public static readonly Dictionary<TSOTFlag, float> globalSpeedDic = new Dictionary<TSOTFlag, float>();
	/// <summary>
	/// 根据target分类的所有计时器
	/// </summary>
	public static readonly Dictionary<object, List<SingleTimer>> targetTimerDic = new Dictionary<object, List<SingleTimer>>();
	/// <summary>
	/// 回收间隔
	/// </summary>
	private const float checkGCTime = 20f;
	/// <summary>
	/// 上次回收时间
	/// </summary>
	private static float lastCheckTime;
	/// <summary>
	/// 将要移除的计时器id
	/// </summary>
	private readonly static List<long> removeIds = new List<long>();
	/// <summary>
	/// 参与排序的临时列表
	/// </summary>
	private readonly static SortedDictionary<long,long> tempList = new SortedDictionary<long, long>();
	#region 指定id  
	/// <summary>
	/// 向指定时间线添加一个事件
	/// </summary>
	/// <param name="timerId">时间线Id</param>
	/// <param name="timePot">时间节点（在时间线的当前时间节点后递增）</param>
	/// <param name="timeEvent">事件</param>
	/// <returns>是否添加成功，若失败请检测是否存在该id时间线</returns>
	public static bool AddTimePot(this long timerId, float timePot, Action timeEvent)
	{
		try
		{
			allTimers[timerId].AddTimePot(timePot, timeEvent);
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ timerId }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 像指定时间线事件队列添加一个事件
	/// </summary>
	/// <param name="timerId">时间线id</param>
	/// <param name="queueId">队列id</param>
	/// <param name="timeEvent">事件</param>
	/// <returns></returns>
	public static bool AddQueue(this long timerId, long queueId, Action<object> timeEvent) 
	{
		try
		{
			allTimers[timerId].AddQueue(queueId, timeEvent);
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ timerId }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 调用并前进某个时间线的某条队列
	/// </summary>
	/// <param name="timerId">时间线id</param>
	/// <param name="exParams">额外参数</param>
	/// <param name="nextIds">事件调用完成后的下个队列id</param>
	/// <returns></returns>
	public static bool MoveNext(this long timerId,object exParams,params long[] nextIds)
	{
		SingleTimer timer = null;
		try
		{
			timer = allTimers[timerId];
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ timerId }的定时器");
			return false;
		}
		return timer.MoveNext(exParams, nextIds);

	}
	/// <summary>
	/// 向指定时间线添加一个更新时事件
	/// </summary>
	/// <param name="timerId">时间线Id</param>
	/// <param name="onUpdate">更新事件</param>
	/// <returns>是否添加成功，若失败请检测是否存在该id时间线</returns>
	public static bool AddOnUpdate(this long timerId, Action onUpdate)
	{
		try
		{
			allTimers[timerId].AddOnUpdateEvent(onUpdate);
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ timerId }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 向指定时间线删除一个更新时事件
	/// </summary>
	/// <param name="timerId">时间线Id</param>
	/// <param name="onUpdate">更新事件</param>
	/// <returns>是否添加成功，若失败请检测是否存在该id时间线</returns>
	public static bool RemoveOnUpdate(this long timerId, Action onUpdate)
	{
		try
		{
			allTimers[timerId].RemoveOnUpdateEvent(onUpdate);
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ timerId }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 删除一个事件
	/// </summary>
	/// <param name="id">时间线id</param>
	/// <param name="index">事件下标</param>
	/// <returns></returns>
	public static bool RemoveTimePot(this long id, int index)
	{
		try
		{
			allTimers[id].RemoveTimePot(index);
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ id }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 删除指定时间线的更新事件
	/// </summary>
	/// <param name="timerId">时间线Id</param>
	/// <returns>是否删除成功，若失败请检测是否存在该id时间线</returns>
	public static bool ClearOnUpdate(this long timerId)
	{
		try
		{
			allTimers[timerId].ClearUpdateEvent();
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ timerId }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 删除某条时间线的全部事件(包括OnUpdate)
	/// </summary>
	/// <param name="id"></param>
	public static bool RemoveAllPotById(this long id)
	{
		try
		{
			allTimers[id].RemoveAllPot();
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ id }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 删除某条时间线的某个时间节点的事件
	/// </summary>
	/// <param name="id"></param>
	/// <param name="timePot"></param>
	/// <returns></returns>
	public static bool RemoveTimePotById(this long id, float timePot)
	{
		try
		{
			return allTimers[id].RemoveTimePot(timePot);
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ id }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 根据id删除整条时间线，请谨慎使用
	/// </summary>
	/// <param name="timerKey"></param>
	/// <returns></returns>
	public static bool RemoveSingleTimerByTimerKey(this long id)
	{
		try
		{
			SingleTimer timer = allTimers[id];
			singleTimerDic[timer.flag.tickType].Remove(timer);
			TargetTimerDic_Remove(timer);
			return allTimers.Remove(id);
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ id }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 变更某条时间线的个人流逝速度
	/// </summary>
	/// <param name="id"></param>
	/// <param name="speedScale"></param>
	/// <returns></returns>
	public static bool ChangeSingleTimerPersonalSpeedScale(this long id, float speedScale)
	{
		try
		{
			if (speedScale != allTimers[id].flag.personalSpeedScale)
			{
				allTimers[id].flag.personalSpeedScale = speedScale;
				allTimers[id].SetSpeedDirty();
			}
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ id }的定时器");
			return false;
		}
	}
	///// <summary>
	///// 根据id流动定时器
	///// </summary>
	public static bool TimerTick(this long id, float unit)
	{
		try
		{
			allTimers[id].TimerTick(unit);
			return true;
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ id }的定时器");
			return false;
		}
	}
	/// <summary>
	/// 为指定时间线设置target
	/// </summary>
	/// <param name="id">时间线id</param>
	/// <param name="target"></param>
	public static void SetTarget(this long id,object target) 
	{
		try
		{
			SingleTimer timer = allTimers[id];
			//删除以前的
			object beforeTarget = timer.flag.target;
			if (beforeTarget != null)
			{
				targetTimerDic[beforeTarget].Remove(timer);
			}
			//添加进新的
			allTimers[id].flag.target = target;
			TargetTimerDic_Add(timer);
		}
		catch
		{
			UnityEngine.Debug.LogError($"找不到id为{ id }的定时器");
		}
	}
	#endregion
	#region 指定类型
	/// <summary>
	/// 删除某个相同标记的所有时间线的全部事件(影响范围很大,请谨慎使用)
	/// </summary>
	/// <param name="id"></param>
	public static bool RemoveAllPotByTarget(object target)
	{
		//根据目标删除事件调用相对不会频繁，且即使没有也属于正常
		if (!targetTimerDic.ContainsKey(target)) return false;
		List<SingleTimer> timers = targetTimerDic[target];
		for (int i = 0; i < timers.Count; i++)
		{
			timers[i].RemoveAllPot();
		}
		return true;
	}
	/// <summary>
	/// 变更某种类型时间线的全局流逝速度
	/// </summary>
	/// <param name="id"></param>
	/// <param name="speedScale"></param>
	/// <returns></returns>
	public static void ChangeSingleTimerGlobalSpeedScale(TSOTFlag tsotFlag, float speedScale)
	{
		//类型定时器数量较少，且即使没有也属于正常
		if (!globalSpeedDic.ContainsKey(tsotFlag)) return;
		if (globalSpeedDic[tsotFlag] != speedScale)
		{
			globalSpeedDic[tsotFlag] = speedScale;
            foreach (var timer in allTimers.Values)
            {
				timer.SetSpeedDirty();
			}
		}
	}
	///// <summary>
	///// 根据类型流动定时器
	///// </summary>
	public static void TimerTick(TimeTickType tickType, float unit)
	{
		//类型定时器数量较少，且即使没有也属于正常
		if (!singleTimerDic.ContainsKey(tickType)) return;
		List<SingleTimer> timers = singleTimerDic[tickType];
		for (int i = 0; i < timers.Count; i++)
		{
			timers[i].TimerTick(unit);
		}
	}
	/// <summary>
	/// 获取时间线当前流逝速度
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static float GetTimeSpeedById(this long id) 
	{
		return allTimers[id].TimerSpeed;
	}
	/// <summary>
	/// 获取时间线当前时间
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static float GetTime(this long id)
	{
		return allTimers[id].GetTime();
	}
	#endregion
	/// <summary>
	/// 创建一条新的时间线
	/// </summary>
	/// <param name="des">时间线描述</param>
	/// <param name="tickType">时间线流逝类型</param>
	/// <param name="maxFlashBackCount">最大可回溯次数(不小于1)</param>
	/// <returns>新时间线id，若小于0则为生成失败</returns>
	public static long CreateTimeLine(string des, float updateUnit,int maxFlashBackCount, object target, object handle,TimeTickType tickType,params TSOTFlag[] tsotFlags)
	{
		CheckCollet();
		if (handle.Equals(null))
		{
			UnityEngine.Debug.LogError("你必须为时间线指定一个绑定的handle");
			return -1;
		}
		long lastId = -1;
		long newTimerId = 0;
		maxFlashBackCount = Math.Min(1,maxFlashBackCount);
		tempList.Clear();
        foreach (var id in allTimers.Keys)
        {
			tempList.Add(id,id);//进行排序
		}
        foreach (long id in tempList.Keys)
        {
			if (id - lastId > 1)//id出现不连续的情况时，在空闲的位置插入新的时间线
			{
				newTimerId = id - 1;
				break;
			}
			lastId = id;
			newTimerId++;
		}
		if (newTimerId > long.MaxValue) newTimerId = -1;
		if (newTimerId >= 0)
		{
			allTimers.Add(newTimerId, new SingleTimer(des, tsotFlags, tickType, updateUnit, maxFlashBackCount,target,handle));
			if (!singleTimerDic.ContainsKey(tickType)) singleTimerDic.Add(tickType, new List<SingleTimer>());
			singleTimerDic[tickType].Add(allTimers[newTimerId]);
			for (int i = 0; i < tsotFlags.Length; i++)
			{
				TSOTFlag tsotFlag = tsotFlags[i];
				if (!globalSpeedDic.ContainsKey(tsotFlag)) globalSpeedDic.Add(tsotFlag, 1f);
			}
		}
		return newTimerId;
	}
	/// <summary>
	/// 像Target时间线分类中添加指定时间线
	/// </summary>
	/// <param name="timer"></param>
	private static void TargetTimerDic_Add(SingleTimer timer) 
	{
		object target = timer.flag.target;
		if (target == null) return;
		if (!targetTimerDic.ContainsKey(target)) targetTimerDic.Add(target,new List<SingleTimer>());
		targetTimerDic[target].Add(timer);
	}
	/// <summary>
	/// 删除Target时间线分类中指定时间线
	/// </summary>
	/// <param name="timer"></param>
	/// <returns></returns>
	private static bool TargetTimerDic_Remove(SingleTimer timer)
	{
		foreach (var timers in targetTimerDic.Values)
		{
			if (timers.Remove(timer)) return true;
		}
		return false;
	}
	/// <summary>
	/// 自动回收检测
	/// </summary>
	private static void CheckCollet() 
	{
		if (UnityEngine.Time.realtimeSinceStartup - lastCheckTime >= checkGCTime)
		{
			removeIds.Clear();
            foreach (long item in allTimers.Keys)
            {
				if (!allTimers[item].weakReference.IsTargetAlive())
				{
					removeIds.Add(item);
				}
            }
            for (int i = 0; i < removeIds.Count; i++)
            {
				removeIds[i].RemoveSingleTimerByTimerKey();
            }
		}
	}

	/// <summary>
	/// 检查弱引用值是否还生存中（比原生版本更精准，推荐使用）
	/// </summary>
	/// <param name="wr">弱引用</param>
	public static bool IsTargetAlive(this System.WeakReference wr)
	{
		if (wr != null && wr.IsAlive)
		{
			if (wr.Target is UnityEngine.Object)
			{
				return (wr.Target as UnityEngine.Object) != null;
			}
			return true;
		}

		return false;
	}
}
