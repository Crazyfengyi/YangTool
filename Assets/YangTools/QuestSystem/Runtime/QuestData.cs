using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务配置数据。
/// </summary>
[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest/QuestData")]
public class QuestData : ScriptableObject
{
    [InspectorName("任务稳定ID")]
    public string Id;
    [InspectorName("任务类型")]
    public TaskType TaskType;
    [InspectorName("默认激活任务")]
    [Tooltip("任务注册且前置任务满足后，是否直接进入进行中状态")]
    public bool DefaultActive;
    [InspectorName("任务标题")]
    [TextArea(1,2)]
    public string Title;
    [InspectorName("任务描述")]
    [TextArea(1,2)]
    public string Description;
    [InspectorName("任务前置条件ID列表")]
    public List<string> PrerequisiteQuestIds = new List<string>();
    [InspectorName("任务目标数据列表")]
    public List<QuestObjectiveData> Objectives = new List<QuestObjectiveData>();
    [InspectorName("任务奖励数据列表")]
    public List<QuestRewardData> Rewards = new List<QuestRewardData>();

    /// <summary>
    /// 获取指定奖励类型的总数量。
    /// </summary>
    /// <param name="rewardType">奖励类型</param>
    /// <returns>奖励总数量</returns>
    public int GetRewardCount(QuestRewardType rewardType)
    {
        return GetRewardCount(rewardType, string.Empty);
    }

    /// <summary>
    /// 获取指定奖励类型和目标ID的总数量。
    /// </summary>
    /// <param name="rewardType">奖励类型</param>
    /// <param name="targetId">目标ID，为空时不限制目标ID</param>
    /// <returns>奖励总数量</returns>
    public int GetRewardCount(QuestRewardType rewardType, string targetId)
    {
        if (Rewards == null || Rewards.Count == 0)
        {
            return 0;
        }

        long count = 0;
        for (int i = 0; i < Rewards.Count; i++)
        {
            QuestRewardData rewardData = Rewards[i];
            if (rewardData == null || rewardData.RewardType != rewardType)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(targetId)
                && !string.Equals(rewardData.TargetKey, targetId, StringComparison.Ordinal))
            {
                continue;
            }

            count += Math.Max(0, rewardData.Count);
            if (count >= int.MaxValue)
            {
                return int.MaxValue;
            }
        }

        return (int)count;
    }
}

/// <summary>
/// 任务目标配置数据。
/// </summary>
[Serializable]
public class QuestObjectiveData
{
    [InspectorName("备注")]
    [TextArea(1,1)]
    public string remark;
    [InspectorName("目标标题")]
    [TextArea(1,1)]
    public string Title;
    [InspectorName("目标描述")]
    [TextArea(1,1)]
    public string Description;
    [InspectorName("条件满足后自动完成")]
    public bool AutoComplete = true;
    [InspectorName("条件组合方式")]
    public QuestConditionGroupType ConditionGroupType = QuestConditionGroupType.And;
    [InspectorName("条件列表")]
    public List<QuestConditionData> Conditions = new List<QuestConditionData>();
}

/// <summary>
/// 任务条件组合类型。
/// </summary>
public enum QuestConditionGroupType
{
    [InspectorName("全部满足")]
    And,
    [InspectorName("任一满足")]
    Or
}

/// <summary>
/// 任务条件类型。
/// </summary>
public enum QuestConditionType
{
    [InspectorName("计数类型-记录事件发生的次数")]
    EventCount,
    [InspectorName("事件类型-表示事件只发生一次")]
    EventOnce
}

[Serializable]
public class QuestConditionData
{
    [InspectorName("条件类型")]
    public QuestConditionType ConditionType = QuestConditionType.EventCount;
    [InspectorName("进度事件类型")]
    public QuestProgressEventType EventType = QuestProgressEventType.Custom;
    [InspectorName("事件目标ID（为空时匹配同类型全部事件）")]
    public string TargetId;
    [InspectorName("目标数量")]
    [Min(1)] public int TargetCount = 1;
}

/// <summary>
/// 任务奖励类型。
/// </summary>
public enum QuestRewardType
{
    [InspectorName("自定义")]
    Custom,
    [InspectorName("现金")]
    Money,
    [InspectorName("金币")]
    Gold,
    [InspectorName("道具")]
    Item,
    [InspectorName("经验")]
    Exp
}

/// <summary>
/// 任务奖励配置数据。
/// </summary>
[Serializable]
public class QuestRewardData
{
    [InspectorName("奖励类型")]
    public QuestRewardType RewardType = QuestRewardType.Custom;
    [InspectorName("奖励目标ID（道具填写道具ID，自定义奖励按业务约定）")]
    public string TargetKey;
    [InspectorName("奖励数量")]
    [Min(1)] public int Count = 1;
}

/// <summary>
/// 任务业务类型
/// </summary>
public enum TaskType
{
    [InspectorName("普通")]
    None,
    [InspectorName("每日")]
    EveryDay,
    [InspectorName("现金")]
    Money,
    [InspectorName("收集")]
    Collect,
}
