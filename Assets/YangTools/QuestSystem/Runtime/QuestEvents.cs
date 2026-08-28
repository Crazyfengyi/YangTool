
using UnityEngine;

/// <summary>
/// 任务进度事件类型，用于统一驱动任务条件进度。
/// </summary>
public enum QuestProgressEventType
{
    [InspectorName("击杀")]
    Kill,
    [InspectorName("收集")]
    Collect,
    [InspectorName("对话")]
    Talk,
    [InspectorName("到达地点")]
    ReachLocation,
    [InspectorName("广告")]
    Ads,
    [InspectorName("时间")]
    Time,
    [InspectorName("物品已有数量")]
    ItemNum,
    [InspectorName("通关数")]
    PassNum,
    [InspectorName("在线时间(秒)")]
    OnLineTime,
    [InspectorName("自定义")]
    Custom = 10000
}

/// <summary>
/// 任务状态。
/// </summary>
public enum QuestState
{
    [InspectorName("锁定")]
    Locked,
    [InspectorName("可接取")]
    Available,
    [InspectorName("进行中")]
    Active,
    [InspectorName("已完成")]
    Completed,
    [InspectorName("已领奖")]
    Rewarded
}

/// <summary>
/// 任务进度事件，业务通过发送该事件推进任务条件。
/// </summary>
public class QuestProgressEvent : QuestEventMessageBase
{
    [InspectorName("事件类型")]
    public QuestProgressEventType EventType;
    [InspectorName("目标ID")]
    public string TargetId;
    [InspectorName("数量")]
    public float Amount;
    [InspectorName("数值（在线时长单位为秒）")]
    public float Value;
    [InspectorName("用户数据")]
    public object UserData;
    /// <summary>
    /// 创建空的任务进度事件
    /// </summary>
    public QuestProgressEvent()
    {
    }

    /// <summary>
    /// 创建任务进度事件
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="targetId">事件目标ID</param>
    /// <param name="amount">事件数量</param>
    /// <param name="value">事件附加数值</param>
    /// <param name="userData">业务数据</param>
    public QuestProgressEvent(QuestProgressEventType eventType, string targetId, float amount = 1f, float value = 0f,
        object userData = null)
    {
        EventType = eventType;
        TargetId = targetId;
        Amount = amount;
        Value = value;
        UserData = userData;
    }
}

/// <summary>
/// 任务状态变化事件。
/// </summary>
public class QuestChangedEvent : QuestEventMessageBase
{
    [InspectorName("任务ID")]
    public string QuestId;
    [InspectorName("旧状态")]
    public QuestState OldState;
    [InspectorName("新状态")]
    public QuestState NewState;
    [InspectorName("任务运行时数据")]
    public QuestRuntime Quest;
}

/// <summary>
/// 任务目标进度变化事件。
/// </summary>
public class QuestObjectiveChangedEvent : QuestEventMessageBase
{
    [InspectorName("任务ID")]
    public string QuestId;
    [InspectorName("目标ID")]
    public string ObjectiveId;
    [InspectorName("当前计数")]
    public float CurrentCount;
    [InspectorName("目标计数")]
    public int TargetCount;
    [InspectorName("条件是否满足")]
    public bool IsConditionsSatisfied;
    [InspectorName("是否完成")]
    public bool IsCompleted;
    [InspectorName("目标运行时数据")]
    public ObjectiveRuntime Objective;
}

/// <summary>
/// 任务奖励发放事件，由业务监听后处理具体奖励。
/// </summary>
public class QuestRewardEvent : QuestEventMessageBase
{
    [InspectorName("任务ID")]
    public string QuestId;
    [InspectorName("奖励列表索引")]
    public int RewardIndex;
    [InspectorName("奖励数据")]
    public QuestRewardData RewardData;
}

/// <summary>
/// 任务进度重置事件
/// </summary>
public class QuestResetEvent : QuestEventMessageBase
{
    [InspectorName("任务ID")]
    public string QuestId;
    [InspectorName("是否每日重置")]
    public bool IsDailyReset;
    [InspectorName("任务运行时数据")]
    public QuestRuntime Quest;
}

/// <summary>
/// 任务配置加载完成事件。
/// </summary>
public class QuestDataLoadedEvent : QuestEventMessageBase
{
    [InspectorName("已加载任务数量")]
    public int QuestCount;
}
