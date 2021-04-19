using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimerStruct
{
	/// <summary>
	/// 时间流逝速度的标记，可自定义
	/// </summary>
	public enum TSOTFlag
	{ 
		手动处理 = 0,
		受时间流逝影响 = 1,
		回合制 = 2,
		受战斗时间影响 = 3,
	}
	/// <summary>
	/// 时间流逝标记，可自定义
	/// </summary>
	public enum TimeTickType
	{ 
		Update,
		FixedUpdate,
		LateUpdate,
	}
	/// <summary>
	/// 时间线信息
	/// </summary>
	public struct Z45TimeInfo
	{
		public readonly string des;
		public readonly TSOTFlag[] tsotFlags;
		public readonly TimeTickType tickType;
		/// <summary>
		/// 个人的时间流速,持有者在所有情况下都仅能修改该变量的值
		/// </summary>
		public float personalSpeedScale;
		/// <summary>
		/// 更新间隔单位
		/// </summary>
		public float updateUnit;
		/// <summary>
		/// 自定义标记物，可以为空
		/// </summary>
		public object target;

		public Z45TimeInfo(string des,TSOTFlag[] tsotFlags, TimeTickType tickType, float updateUnit,object target)
		{
			this.des = des;
			this.tsotFlags = tsotFlags;
			this.tickType = tickType;
			personalSpeedScale = 1f;
			this.updateUnit = updateUnit;
			this.target = target;
		}
	}
	//public struct Z45QueueInfo
	//{
	//	public System.Action<object> action;
	//	public long[] nextIds;
	//	public Z45QueueInfo(System.Action<object> action, long[] nextIds) 
	//	{
	//		this.action = action;
	//		this.nextIds = nextIds;
	//	}
	//}
}
