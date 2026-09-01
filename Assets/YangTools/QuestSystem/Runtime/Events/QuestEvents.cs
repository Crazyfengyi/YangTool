using UnityEngine;

/// <summary>
/// 任务进度事件类型
/// </summary>
public enum QuestProgressEventType
{
    [InspectorName("击杀")] Kill,
    [InspectorName("收集")] Collect,
    [InspectorName("对话")] Talk,
    [InspectorName("到达地点")] ReachLocation,
    [InspectorName("广告")] Ads,
    [InspectorName("时间")] Time,
    [InspectorName("物品已有数量")] ItemNum,
    [InspectorName("通关数")] PassNum,
    [InspectorName("在线时间(秒)")] OnLineTime,
    [InspectorName("自定义")] Custom = 10000
}

/// <summary>
/// 任务状态
/// </summary>
public enum QuestState
{
    [InspectorName("锁定")] Locked,
    [InspectorName("可接取")] Available,
    [InspectorName("进行中")] Active,
    [InspectorName("已完成")] Completed,
    [InspectorName("已领奖")] Rewarded
}

/// <summary>
/// 任务进度事件
/// </summary>
public class QuestProgressEvent : QuestEventMessageBase
{
    /// <summary>
    /// 进度事件类型
    /// </summary>
    public QuestProgressEventType EventType;

    /// <summary>
    /// 事件对应的目标ID
    /// </summary>
    public string TargetId;

    /// <summary>
    /// 本次事件增加的进度数量
    /// </summary>
    public float Amount;

    /// <summary>
    /// 时间或在线时长事件使用的秒数
    /// </summary>
    public float Value;

    /// <summary>
    /// 业务层携带的自定义数据
    /// </summary>
    public object UserData;

    /// <summary>
    /// 创建空进度事件
    /// </summary>
    public QuestProgressEvent()
    {
    }

    /// <summary>
    /// 创建任务进度事件
    /// </summary>
    public QuestProgressEvent(QuestProgressEventType eventType, string targetId, float amount = 1f,
        float value = 0f, object userData = null)
    {
        EventType = eventType;
        TargetId = targetId;
        Amount = amount;
        Value = value;
        UserData = userData;
    }
}

/// <summary>
/// 任务状态变化事件
/// </summary>
public class QuestChangedEvent : QuestEventMessageBase
{
    /// <summary>
    /// 发生变化的任务ID
    /// </summary>
    public string QuestId;

    /// <summary>
    /// 变化前的任务状态
    /// </summary>
    public QuestState OldState;

    /// <summary>
    /// 变化后的任务状态
    /// </summary>
    public QuestState NewState;

    /// <summary>
    /// 发生变化的任务运行时数据
    /// </summary>
    public QuestRuntime Quest;
}

/// <summary>
/// 任务目标进度变化事件
/// </summary>
public class QuestObjectiveChangedEvent : QuestEventMessageBase
{
    /// <summary>
    /// 发生变化的任务ID
    /// </summary>
    public string QuestId;

    /// <summary>
    /// 发生变化的目标ID
    /// </summary>
    public string ObjectiveId;

    /// <summary>
    /// 当前目标展示进度
    /// </summary>
    public float CurrentCount;

    /// <summary>
    /// 当前目标展示进度上限
    /// </summary>
    public int TargetCount;

    /// <summary>
    /// 当前目标条件是否全部满足
    /// </summary>
    public bool IsConditionsSatisfied;

    /// <summary>
    /// 当前目标是否已经完成
    /// </summary>
    public bool IsCompleted;

    /// <summary>
    /// 发生变化的目标运行时数据
    /// </summary>
    public ObjectiveRuntime Objective;
}

/// <summary>
/// 任务奖励发放事件
/// </summary>
public class QuestRewardEvent : QuestEventMessageBase
{
    /// <summary>
    /// 发生变化的任务ID
    /// </summary>
    public string QuestId;

    /// <summary>
    /// 奖励在任务奖励列表中的索引
    /// </summary>
    public int RewardIndex;

    /// <summary>
    /// 当前发放的奖励
    /// </summary>
    public Reward Reward;

    /// <summary>
    /// 奖励是否发放成功
    /// </summary>
    public bool Succeeded;

    /// <summary>
    /// 奖励失败时的错误信息
    /// </summary>
    public string Error;
}

/// <summary>
/// 任务进度重置事件
/// </summary>
public class QuestResetEvent : QuestEventMessageBase
{
    /// <summary>
    /// 被重置的任务ID
    /// </summary>
    public string QuestId;

    /// <summary>
    /// 是否为每日任务刷新
    /// </summary>
    public bool IsDailyReset;

    /// <summary>
    /// 被重置的任务运行时数据
    /// </summary>
    public QuestRuntime Quest;
}

/// <summary>
/// 任务配置加载完成事件
/// </summary>
public class QuestDataLoadedEvent : QuestEventMessageBase
{
    /// <summary>
    /// 已加载的任务数量
    /// </summary>
    public int QuestCount;
}
