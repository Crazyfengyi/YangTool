using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 任务配置数据。
/// </summary>
[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string Id;
    public TaskType TaskType;
    [LabelText("任务标题")]
    [TextArea]
    public string Title;
    [LabelText("任务描述")]
    [TextArea] public string Description;
    [LabelText("任务前置条件ID列表")]
    public List<string> PrerequisiteQuestIds = new List<string>();
    [LabelText("任务目标数据列表")]
    public List<QuestObjectiveData> Objectives = new List<QuestObjectiveData>();
    [LabelText("任务奖励数据列表")]
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

        int count = 0;
        for (int i = 0; i < Rewards.Count; i++)
        {
            QuestRewardData rewardData = Rewards[i];
            if (rewardData == null || rewardData.RewardType != rewardType)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(targetId) && rewardData.TargetId != targetId)
            {
                continue;
            }

            count += rewardData.Count;
        }

        return count;
    }
}

/// <summary>
/// 任务目标配置数据。
/// </summary>
[Serializable]
public class QuestObjectiveData
{
    public string Id;
    [TextArea] 
    public string Title;
    [TextArea] 
    public string Description;
    [TextArea] 
    public string remark;
    public QuestConditionGroupType ConditionGroupType = QuestConditionGroupType.And;
    public List<QuestConditionData> Conditions = new List<QuestConditionData>();
}

/// <summary>
/// 任务条件组合类型。
/// </summary>
public enum QuestConditionGroupType
{
    And,
    Or
}

/// <summary>
/// 任务条件类型。
/// </summary>
public enum QuestConditionType
{
    [LabelText("计数类型-记录事件发生的次数")]
    EventCount,
    [LabelText("事件类型-表示事件只发生一次")]
    EventOnce
}

[LabelText("任务条件配置数据")]
[Serializable]
public class QuestConditionData
{
    public string Id;
    public QuestConditionType ConditionType = QuestConditionType.EventCount;
    public QuestProgressEventType EventType = QuestProgressEventType.Custom;
    public string TargetId;
    [Min(1)] public int TargetCount = 1;
}

/// <summary>
/// 任务奖励类型。
/// </summary>
public enum QuestRewardType
{
    Custom,
    Money,
    Gold,
    Item,
    Exp
}

/// <summary>
/// 任务奖励配置数据。
/// </summary>
[Serializable]
public class QuestRewardData
{
    public string Id;
    public QuestRewardType RewardType = QuestRewardType.Custom;
    public string TargetId;
    public int Count;
}

public enum TaskType
{
    None,
    EveryDay,
    Money,
    Collect,
}
