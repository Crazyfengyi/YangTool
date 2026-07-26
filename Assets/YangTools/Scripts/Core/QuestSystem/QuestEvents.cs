using YangTools;
using Sirenix.OdinInspector;

/// <summary>
/// 任务进度事件类型，用于统一驱动任务条件进度。
/// </summary>
public enum QuestProgressEventType
{
    [LabelText("击杀")]
    Kill,
    [LabelText("收集")]
    Collect,
    [LabelText("对话")]
    Talk,
    [LabelText("到达地点")]
    ReachLocation,
    [LabelText("广告")]
    Ads,
    [LabelText("时间")]
    Time,
    [LabelText("物品已有数量")]
    ItemNum,
    [LabelText("通关数")]
    PassNum,
    [LabelText("在线时间")]
    OnLineTime,
    [LabelText("自定义")]
    Custom = 10000
}

/// <summary>
/// 任务状态。
/// </summary>
public enum QuestState
{
    [LabelText("锁定")]
    Locked,
    [LabelText("可接取")]
    Available,
    [LabelText("进行中")]
    Active,
    [LabelText("已完成")]
    Completed,
    [LabelText("已领奖")]
    Rewarded
}

/// <summary>
/// 任务进度事件，业务通过发送该事件推进任务条件。
/// </summary>
public class QuestProgressEvent : EventMessageBase
{
    [LabelText("事件类型")]
    public QuestProgressEventType EventType;
    [LabelText("目标ID")]
    public string TargetId;
    [LabelText("数量")]
    public float Amount;
    [LabelText("数值")]
    public float Value;
    [LabelText("用户数据")]
    public object UserData;

    public QuestProgressEvent()
    {
    }

    public QuestProgressEvent(QuestProgressEventType eventType, string targetId, int amount = 1, float value = 0f,
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
public class QuestChangedEvent : EventMessageBase
{
    [LabelText("任务ID")]
    public string QuestId;
    [LabelText("旧状态")]
    public QuestState OldState;
    [LabelText("新状态")]
    public QuestState NewState;
    [LabelText("任务运行时数据")]
    public QuestRuntime Quest;
}

/// <summary>
/// 任务目标进度变化事件。
/// </summary>
public class QuestObjectiveChangedEvent : EventMessageBase
{
    [LabelText("任务ID")]
    public string QuestId;
    [LabelText("目标ID")]
    public string ObjectiveId;
    [LabelText("当前计数")]
    public float CurrentCount;
    [LabelText("目标计数")]
    public int TargetCount;
    [LabelText("条件是否满足")]
    public bool IsConditionsSatisfied;
    [LabelText("是否完成")]
    public bool IsCompleted;
    [LabelText("目标运行时数据")]
    public ObjectiveRuntime Objective;
}

/// <summary>
/// 任务奖励发放事件，由业务监听后处理具体奖励。
/// </summary>
public class QuestRewardEvent : EventMessageBase
{
    [LabelText("任务ID")]
    public string QuestId;
    [LabelText("奖励数据")]
    public QuestRewardData RewardData;
}

/// <summary>
/// 任务配置加载完成事件。
/// </summary>
public class QuestDataLoadedEvent : EventMessageBase
{
}
