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
    [LabelText("任务稳定ID")]
    public string Id;
    [LabelText("任务类型")]
    public TaskType TaskType;
    [LabelText("任务标题")]
    [TextArea(1,2)]
    public string Title;
    [LabelText("任务描述")]
    [TextArea(1,2)]
    public string Description;
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
    [LabelText("备注")]
    [TextArea(1,1)]
    public string remark;
    [LabelText("目标标题")]
    [TextArea(1,1)]
    public string Title;
    [LabelText("目标描述")]
    [TextArea(1,1)]
    public string Description;
    [LabelText("条件满足后自动完成")]
    public bool AutoComplete = true;
    [LabelText("条件组合方式")]
    public QuestConditionGroupType ConditionGroupType = QuestConditionGroupType.And;
    [LabelText("条件列表")]
    public List<QuestConditionData> Conditions = new List<QuestConditionData>();
}

/// <summary>
/// 任务条件组合类型。
/// </summary>
public enum QuestConditionGroupType
{
    [LabelText("全部满足")]
    And,
    [LabelText("任一满足")]
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
    [LabelText("条件类型")]
    public QuestConditionType ConditionType = QuestConditionType.EventCount;
    [LabelText("进度事件类型")]
    public QuestProgressEventType EventType = QuestProgressEventType.Custom;
    [LabelText("事件目标ID（为空时匹配同类型全部事件）")]
    public string TargetId;
    [LabelText("目标数量")]
    [Min(1)] public int TargetCount = 1;
}

/// <summary>
/// 任务奖励类型。
/// </summary>
public enum QuestRewardType
{
    [LabelText("自定义")]
    Custom,
    [LabelText("现金")]
    Money,
    [LabelText("金币")]
    Gold,
    [LabelText("道具")]
    Item,
    [LabelText("经验")]
    Exp
}

/// <summary>
/// 任务奖励配置数据。
/// </summary>
[Serializable]
public class QuestRewardData
{
    [LabelText("奖励类型")]
    public QuestRewardType RewardType = QuestRewardType.Custom;
    [LabelText("奖励目标ID（道具填写道具ID，自定义奖励按业务约定）")]
    public string TargetKey;
    [LabelText("奖励数量")]
    [Min(1)] public int Count = 1;
}

/// <summary>
/// 任务业务类型
/// </summary>
public enum TaskType
{
    [LabelText("普通")]
    None,
    [LabelText("每日")]
    EveryDay,
    [LabelText("现金")]
    Money,
    [LabelText("收集")]
    Collect,
}
