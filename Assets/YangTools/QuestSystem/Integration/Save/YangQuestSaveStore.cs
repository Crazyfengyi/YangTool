#if YANGTOOLS_QUEST_INTEGRATION
using System;
using System.Collections.Generic;
using YangTools.Scripts.Core.YangSaveData;

/// <summary>
/// 项目存档与任务独立存档模型之间的映射适配器
/// </summary>
public sealed class YangQuestSaveStore : IQuestSaveStore
{
    private readonly QuestMemorySaveStore fallbackStore = new QuestMemorySaveStore();
    private readonly Dictionary<string, QuestSaveItem> cache = new Dictionary<string, QuestSaveItem>(StringComparer.Ordinal);

    /// <summary>
    /// 获取项目存档中的任务数据
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务存档</returns>
    public QuestSaveItem GetQuest(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId)) return null;
        if (cache.TryGetValue(questId, out QuestSaveItem cachedItem)) return cachedItem;

        Save_QuestData saveData = GetSaveData();
        if (saveData == null) return fallbackStore.GetQuest(questId);
        QuestSaveItem item = FromProject(saveData.GetQuest(questId));
        if (item != null) cache[questId] = item;
        return item;
    }

    /// <summary>
    /// 获取或创建项目存档中的任务数据
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务存档</returns>
    public QuestSaveItem GetOrCreateQuest(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId)) throw new ArgumentException("任务ID不能为空", nameof(questId));
        QuestSaveItem item = GetQuest(questId);
        if (item != null) return item;

        Save_QuestData saveData = GetSaveData(true);
        if (saveData == null) return fallbackStore.GetOrCreateQuest(questId);
        item = new QuestSaveItem { questId = questId, state = QuestState.Locked, objectives = new List<QuestSaveObjectiveItem>() };
        cache[questId] = item;
        saveData.quests ??= new List<SaveQuestItem>();
        saveData.quests.Add(ToProject(item));
        return item;
    }

    /// <summary>
    /// 清空项目存档中的全部任务数据
    /// </summary>
    public void Clear()
    {
        cache.Clear();
        fallbackStore.Clear();
        Save_QuestData saveData = GetSaveData(true);
        if (saveData != null) saveData.quests = new List<SaveQuestItem>();
    }

    /// <summary>
    /// 将缓存中的任务存档写回项目存档
    /// </summary>
    public void MarkDirty()
    {
        Save_QuestData saveData = GetSaveData(true);
        if (saveData == null)
        {
            fallbackStore.MarkDirty();
            return;
        }

        saveData.quests ??= new List<SaveQuestItem>();
        foreach (KeyValuePair<string, QuestSaveItem> pair in cache)
        {
            SaveQuestItem projectItem = saveData.GetQuest(pair.Key);
            if (projectItem == null)
            {
                saveData.quests.Add(ToProject(pair.Value));
            }
            else
            {
                CopyToProject(pair.Value, projectItem);
            }
        }
    }

    /// <summary>
    /// 获取项目本地任务存档
    /// </summary>
    /// <param name="isDirty">是否标记存档数据已修改</param>
    /// <returns>项目任务存档</returns>
    private static Save_QuestData GetSaveData(bool isDirty = false)
    {
        YangSaveDataManager saveManager = YangSaveDataManager.Instance;
        return saveManager != null && saveManager.DataCenter != null
            ? saveManager.DataCenter.GetLocalSave<Save_QuestData>(isDirty)
            : null;
    }

    /// <summary>
    /// 将项目存档转换为任务独立存档
    /// </summary>
    /// <param name="source">项目任务存档</param>
    /// <returns>任务独立存档</returns>
    private static QuestSaveItem FromProject(SaveQuestItem source)
    {
        if (source == null) return null;
        QuestSaveItem result = new QuestSaveItem
        {
            questId = source.questId,
            dailyRefreshDate = source.dailyRefreshDate,
            state = source.state,
            objectives = new List<QuestSaveObjectiveItem>()
        };
        if (source.objectives == null) return result;
        for (int i = 0; i < source.objectives.Count; i++)
        {
            SaveQuestObjectiveItem sourceObjective = source.objectives[i];
            if (sourceObjective == null)
            {
                result.objectives.Add(null);
                continue;
            }

            QuestSaveObjectiveItem objective = new QuestSaveObjectiveItem
            {
                objectiveId = sourceObjective.objectiveId,
                isCompleted = sourceObjective.isCompleted,
                conditions = new List<QuestSaveConditionItem>()
            };
            if (sourceObjective.conditions != null)
            {
                for (int j = 0; j < sourceObjective.conditions.Count; j++)
                {
                    objective.conditions.Add(FromProject(sourceObjective.conditions[j]));
                }
            }
            result.objectives.Add(objective);
        }
        return result;
    }

    /// <summary>
    /// 将任务独立存档转换为项目存档
    /// </summary>
    /// <param name="source">任务独立存档</param>
    /// <returns>项目任务存档</returns>
    private static SaveQuestItem ToProject(QuestSaveItem source)
    {
        SaveQuestItem result = new SaveQuestItem();
        CopyToProject(source, result);
        return result;
    }

    /// <summary>
    /// 将任务独立存档复制到项目存档
    /// </summary>
    /// <param name="source">任务独立存档</param>
    /// <param name="target">项目任务存档</param>
    private static void CopyToProject(QuestSaveItem source, SaveQuestItem target)
    {
        target.questId = source.questId;
        target.dailyRefreshDate = source.dailyRefreshDate;
        target.state = source.state;
        target.objectives = new List<SaveQuestObjectiveItem>();
        if (source.objectives == null) return;
        for (int i = 0; i < source.objectives.Count; i++)
        {
            QuestSaveObjectiveItem sourceObjective = source.objectives[i];
            if (sourceObjective == null)
            {
                target.objectives.Add(null);
                continue;
            }

            SaveQuestObjectiveItem objective = new SaveQuestObjectiveItem
            {
                objectiveId = sourceObjective.objectiveId,
                isCompleted = sourceObjective.isCompleted,
                conditions = new List<SaveQuestConditionItem>()
            };
            if (sourceObjective.conditions != null)
            {
                for (int j = 0; j < sourceObjective.conditions.Count; j++)
                {
                    objective.conditions.Add(ToProject(sourceObjective.conditions[j]));
                }
            }
            target.objectives.Add(objective);
        }
    }

    /// <summary>
    /// 将项目组合条件存档递归转换为任务独立存档
    /// </summary>
    /// <param name="source">项目条件存档</param>
    /// <returns>任务条件存档</returns>
    private static QuestSaveConditionItem FromProject(SaveQuestConditionItem source)
    {
        if (source == null) return null;
        QuestSaveConditionItem result = new QuestSaveConditionItem
        {
            conditionId = source.conditionId,
            currentCount = source.currentCount,
            isCompleted = source.isCompleted,
            startUtcSeconds = source.startUtcSeconds,
            onlineTimeSeconds = source.onlineTimeSeconds,
            children = new List<QuestSaveConditionItem>()
        };
        if (source.children != null)
        {
            for (int i = 0; i < source.children.Count; i++)
            {
                result.children.Add(FromProject(source.children[i]));
            }
        }
        return result;
    }

    /// <summary>
    /// 将任务独立组合条件存档递归转换为项目存档
    /// </summary>
    /// <param name="source">任务条件存档</param>
    /// <returns>项目条件存档</returns>
    private static SaveQuestConditionItem ToProject(QuestSaveConditionItem source)
    {
        if (source == null) return null;
        SaveQuestConditionItem result = new SaveQuestConditionItem
        {
            conditionId = source.conditionId,
            currentCount = source.currentCount,
            isCompleted = source.isCompleted,
            startUtcSeconds = source.startUtcSeconds,
            onlineTimeSeconds = source.onlineTimeSeconds,
            children = new List<SaveQuestConditionItem>()
        };
        if (source.children != null)
        {
            for (int i = 0; i < source.children.Count; i++)
            {
                result.children.Add(ToProject(source.children[i]));
            }
        }
        return result;
    }
}
#endif
