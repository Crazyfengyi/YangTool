using System;
using System.Collections.Generic;
using TimerStruct;
public class SingleTimer
{
    public Z45TimeInfo flag;
	private float timer;//该类的计时器计算方式独立于游戏时间
	private List<float> timePots;//执行事件的节点
	private List<Action> timeEvents;//时间节点上对应需要执行的事件
	private Action onUpdate;//每次达到更新间隔时的更新调用事件
	private int _maxFlashBackCount;
	private float lastCallOnUpdate;//上次达到更新间隔调用的時間
	private List<Action> checkedAction;//检测使用过的队列事件，在一下次检测开始前会从队列中移除(既在下次队列开始前可以重复用)
	private Dictionary<long,Queue<Action<object>>> queueDic;//线性队列集合
	public WeakReference weakReference;//弱应用用于计时器回收
	private bool isSpeedDirty;
	public float lastTimeSpeed;
	/// <summary>
	/// 最大能回溯的检测时间次数(最小为1)
	/// </summary>
	public int MaxFlashBackCount
	{
		get
		{
			return _maxFlashBackCount;
		}
		private set
		{
			if (value < 1) value = 1;
			_maxFlashBackCount = value;
			while (checkedAction.Count > 0 && checkedAction.Count > _maxFlashBackCount)
			{
				checkedAction.RemoveAt(0);
			}
		}
	}
	/// <summary>
	/// 计时器当前的时间流速，受计时器类型和计时器的自身流速影响
	/// </summary>
	public float TimerSpeed
	{
		get
		{
			if (!isSpeedDirty) return lastTimeSpeed;
			float tickSpeed = flag.personalSpeedScale;
			for (int i = 0; i < flag.tsotFlags.Length; i++)
			{
				tickSpeed *= Z45Timer.globalSpeedDic[flag.tsotFlags[i]];
			}
			lastTimeSpeed = tickSpeed;
			isSpeedDirty = false;
			return tickSpeed;
		}
	}
	/// <summary>
	/// 设置计时器流速是否曾经发生改变
	/// </summary>
	public void SetSpeedDirty() 
	{
		isSpeedDirty = true;
	}
	public SingleTimer(string des,TSOTFlag[] tsotFlags,TimeTickType tickType,float updateUnit, int maxFlashBackCount,object target,object handle)
	{
		timer = 0f;
		lastCallOnUpdate = 0f;
		timePots = new List<float>();
		timeEvents = new List<Action>();
		checkedAction = new List<Action>();
		MaxFlashBackCount = maxFlashBackCount;
		flag = new Z45TimeInfo(des, tsotFlags, tickType, updateUnit,target);
        queueDic = new Dictionary<long, Queue<Action<object>>>();
		weakReference = new WeakReference(handle);
		isSpeedDirty = true;
	}
	/// <summary>
	/// 添加一个在指定时间后发生的事件
	/// </summary>
	/// <param name="timePot"></param>
	/// <param name="timeEvent"></param>
	public void AddTimePot(float timePot, Action timeEvent)
	{
		timePots.Add(timer + timePot);//从当前时间往后推
		timeEvents.Add(timeEvent);
	}
	/// <summary>
	/// 删除指定事件
	/// </summary>
	/// <param name="index"></param>
	public void RemoveTimePot(int index)
	{
		timePots.RemoveAt(index);
		timeEvents.RemoveAt(index);
	}
	/// <summary>
	/// 删除所有事件，包括更新事件/时间节点事件/事件队列
	/// </summary>
	public void RemoveAllPot()
	{
		timePots.Clear();
		timeEvents.Clear();
		ClearUpdateEvent();
		queueDic.Clear();
	}
	/// <summary>
	/// 删除指定时间节点事件
	/// </summary>
	/// <param name="timePot"></param>
	/// <returns></returns>
	public bool RemoveTimePot(float timePot)
	{
		List<int> indexs = new List<int>();
		for (int i = timePots.Count - 1; i >= 0; i--)
		{
			if (timePot == timePots[i])
			{
				indexs.Add(i);
			}
		}
		for (int i = 0; i < indexs.Count; i++)
		{
			RemoveTimePot(indexs[i]);
		}
		return indexs.Count > 0;
	}
	/// <summary>
	/// 检测时间事件发生
	/// </summary>
	/// <param name="curCheckAction"></param>
	private void CheckTimePot(Action curCheckAction)
	{
		for (int i = 0; i < timePots.Count; i++)
		{
			if (timer >= timePots[i])
			{
				curCheckAction += timeEvents[i];
				RemoveTimePot(i);
				i--;//因为删除了当前位,所以下次依旧从当前位遍历
			}
		}
		AddCheckAction(curCheckAction);//将本次检测结果计入可回溯队列
	}
	/// <summary>
	/// 计时器按步长前进一次
	/// </summary>
	/// <param name="tick"></param>
	public void TimerTick(float tick)
	{
		timer += tick * TimerSpeed;
		Action checkAction = null;
		if (timer - lastCallOnUpdate >= flag.updateUnit && timer != lastCallOnUpdate)
		{
			checkAction += onUpdate;
			lastCallOnUpdate = timer;
		}
		CheckTimePot(checkAction);
		Update();
	}
	/// <summary>
	/// 获得计时器当前时间
	/// </summary>
	/// <returns></returns>
	public float GetTime()
	{
		return timer;
	}
	/// <summary>
	/// 清空更新时事件
	/// </summary>
	public void ClearUpdateEvent()
	{
		onUpdate = null;
	}
	/// <summary>
	/// 添加更新时事件
	/// </summary>
	/// <param name="timeEvent"></param>
	public void AddOnUpdateEvent(Action timeEvent)
	{
		onUpdate += timeEvent;
	}
	/// <summary>
	/// 删除指定更新时事件
	/// </summary>
	/// <param name="timeEvent"></param>
	public void RemoveOnUpdateEvent(Action timeEvent)
	{
		onUpdate -= timeEvent;
	}
	/// <summary>
	/// 调用最后一次更新
	/// </summary>
	private void Update() 
	{
		GetFlashBack(checkedAction.Count - 1)?.Invoke();
	}
	/// <summary>
	/// 往可回溯列表里添加一次检测事件
	/// </summary>
	/// <param name="action"></param>
	private void AddCheckAction(Action action)
	{
		checkedAction.Add(action);
		if (checkedAction.Count > MaxFlashBackCount)
		{
			checkedAction.RemoveAt(0);
		}
	}
	/// <summary>
	/// 获得回溯队列中的某次事件
	/// </summary>
	public Action GetFlashBack(int index) 
	{
		int count = checkedAction.Count;
		for (int i = 0; i < count; i++)
		{
			if (i > index) return null;
			if (index == i) return checkedAction[i];
		}
		return null;
	}
	/// <summary>
	/// 向一个队列中添加事件(只能向后添加)
	/// </summary>
	/// <param name="id">将要添加的队列id</param>
	/// <param name="timeEvent">事件</param>
	/// <param name="nextIds">该队列完成时启动的下个队列id</param>
	public void AddQueue(long id,Action<object> timeEvent)
	{
		if (!queueDic.ContainsKey(id)) queueDic.Add(id,new Queue<Action<object>>());
		queueDic[id].Enqueue(timeEvent);
	}
	/// <summary>
	/// 事件队列调用并前进一次
	/// </summary>
	/// <param name="exParams"></param>
	/// <param name="nextIds"></param>
	/// <returns></returns>
	public bool MoveNext(object exParams,long[] nextIds) 
	{
		bool isEnd = true;
        for (int i = 0; i < nextIds.Length; i++)
        {
			long id = nextIds[i];
			if (!queueDic.ContainsKey(id)) continue;
			if (queueDic[id].Count <= 0) continue;
			isEnd = false;
			var a = queueDic[id].Dequeue();
			a?.Invoke(exParams);
		}
		return isEnd;
	}
}
