using System;
using System.Collections.Generic;

/// <summary>
/// 任务系统独立存档模型
/// </summary>
[Serializable]
public sealed class QuestSaveItem
{
    public string questId;
    public string dailyRefreshDate;
    public QuestState state;
    public List<QuestSaveObjectiveItem> objectives = new List<QuestSaveObjectiveItem>();

    /// <summary>
    /// 修正反序列化后的空列表
    /// </summary>
    public void OnAfterDeserialize()
    {
        dailyRefreshDate ??= string.Empty;
        objectives ??= new List<QuestSaveObjectiveItem>();
        for (int i = 0; i < objectives.Count; i++) objectives[i]?.OnAfterDeserialize();
    }

    /// <summary>
    /// 获取指定目标存档
    /// </summary>
    public QuestSaveObjectiveItem GetObjective(string objectiveId)
    {
        if (objectives == null || string.IsNullOrEmpty(objectiveId))
        {
            return null;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestSaveObjectiveItem item = objectives[i];
            if (item != null && string.Equals(item.objectiveId, objectiveId, StringComparison.Ordinal))
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// 按目标索引获取存档
    /// </summary>
    public QuestSaveObjectiveItem GetObjectiveAt(int index)
    {
        return objectives != null && index >= 0 && index < objectives.Count ? objectives[index] : null;
    }
}

/// <summary>
/// 任务目标独立存档模型
/// </summary>
[Serializable]
public sealed class QuestSaveObjectiveItem
{
    public string objectiveId;
    public bool isCompleted;
    public List<QuestSaveConditionItem> conditions = new List<QuestSaveConditionItem>();

    /// <summary>
    /// 修正反序列化后的空列表
    /// </summary>
    public void OnAfterDeserialize()
    {
        conditions ??= new List<QuestSaveConditionItem>();
        for (int i = 0; i < conditions.Count; i++) conditions[i]?.OnAfterDeserialize();
    }

    /// <summary>
    /// 获取指定条件存档
    /// </summary>
    public QuestSaveConditionItem GetCondition(string conditionId)
    {
        if (conditions == null || string.IsNullOrEmpty(conditionId))
        {
            return null;
        }

        for (int i = 0; i < conditions.Count; i++)
        {
            QuestSaveConditionItem item = conditions[i];
            if (item != null && string.Equals(item.conditionId, conditionId, StringComparison.Ordinal))
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// 按条件索引获取存档
    /// </summary>
    public QuestSaveConditionItem GetConditionAt(int index)
    {
        return conditions != null && index >= 0 && index < conditions.Count ? conditions[index] : null;
    }
}

/// <summary>
/// 任务条件独立存档模型
/// </summary>
[Serializable]
public sealed class QuestSaveConditionItem
{
    public string conditionId;
    public float currentCount;
    public bool isCompleted;
    public long startUtcSeconds;
    // 字段名保持兼容 实际存储单位为秒
    public float onlineTimeSeconds;
    public List<QuestSaveConditionItem> children = new List<QuestSaveConditionItem>();

    /// <summary>
    /// 修正反序列化后的非法数值
    /// </summary>
    public void OnAfterDeserialize()
    {
        conditionId ??= string.Empty;
        children ??= new List<QuestSaveConditionItem>();
        currentCount = NormalizeNonNegative(currentCount);
        startUtcSeconds = Math.Max(0L, startUtcSeconds);
        onlineTimeSeconds = NormalizeNonNegative(onlineTimeSeconds);
        for (int i = 0; i < children.Count; i++) children[i]?.OnAfterDeserialize();
    }

    private static float NormalizeNonNegative(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Math.Max(0f, value);
    }
}
