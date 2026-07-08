using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YangTools.Scripts.Core.YangSaveData;

/// <summary>
/// 任务配置数据。
/// </summary>
[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string Id;
    public string Title;
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
    public float Value;
}

#region 存档相关

 /// <summary>
    /// 任务系统存档。
    /// </summary>
    public class Save_QuestData : SaveGameSet
    {
        public List<SaveQuestItem> quests;

        public override void SetDefaultData(string tableData)
        {
            quests = new List<SaveQuestItem>();
        }

        public override void OnAfterDeserialize()
        {
            quests ??= new List<SaveQuestItem>();
            for (int i = 0; i < quests.Count; i++)
            {
                quests[i].OnAfterDeserialize();
            }
        }

        /// <summary>
        /// 获取或创建任务存档。
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>任务存档</returns>
        public SaveQuestItem GetOrCreateQuest(string questId)
        {
            quests ??= new List<SaveQuestItem>();
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].questId == questId)
                {
                    return quests[i];
                }
            }

            SaveQuestItem item = new SaveQuestItem
            {
                questId = questId,
                state = QuestState.Active,
                objectives = new List<SaveQuestObjectiveItem>()
            };
            quests.Add(item);
            return item;
        }
    }

    /// <summary>
    /// 单个任务存档。
    /// </summary>
    [Serializable]
    public class SaveQuestItem
    {
        public string questId;
        public QuestState state;
        public bool firstObjectiveMoneyTipShown;
        public List<SaveQuestObjectiveItem> objectives;

        public void OnAfterDeserialize()
        {
            objectives ??= new List<SaveQuestObjectiveItem>();
            for (int i = 0; i < objectives.Count; i++)
            {
                objectives[i].OnAfterDeserialize();
            }
        }

        /// <summary>
        /// 获取指定目标存档。
        /// </summary>
        /// <param name="objectiveId">目标ID</param>
        /// <returns>目标存档</returns>
        public SaveQuestObjectiveItem GetObjective(string objectiveId)
        {
            objectives ??= new List<SaveQuestObjectiveItem>();
            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].objectiveId == objectiveId)
                {
                    return objectives[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 单个任务目标存档。
    /// </summary>
    [Serializable]
    public class SaveQuestObjectiveItem
    {
        public string objectiveId;
        public bool isCompleted;
        public List<SaveQuestConditionItem> conditions;

        public void OnAfterDeserialize()
        {
            conditions ??= new List<SaveQuestConditionItem>();
        }

        /// <summary>
        /// 获取指定条件存档。
        /// </summary>
        /// <param name="conditionId">条件ID</param>
        /// <returns>条件存档</returns>
        public SaveQuestConditionItem GetCondition(string conditionId)
        {
            conditions ??= new List<SaveQuestConditionItem>();
            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i].conditionId == conditionId)
                {
                    return conditions[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 单个任务条件存档。
    /// </summary>
    [Serializable]
    public class SaveQuestConditionItem
    {
        public string conditionId;
        public float currentCount;
        public long startUtcSeconds;
    }

#endregion
