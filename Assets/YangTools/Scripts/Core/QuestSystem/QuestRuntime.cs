using System;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using YangTools.Scripts.Core.YangSaveData;

/// <summary>
/// 任务运行时数据。
/// </summary>
public class QuestRuntime
{
    public QuestData Data { get; }
    public string Id => Data != null ? Data.Id : string.Empty;
    public QuestState State { get; private set; }
    public List<ObjectiveRuntime> Objectives { get; } = new List<ObjectiveRuntime>();

    public QuestRuntime(QuestData data, SaveQuestItem saveItem)
    {
        Data = data;
        State = saveItem != null ? saveItem.state : QuestState.Active;
        BuildObjectives(saveItem);
    }

    /// <summary>
    /// 设置任务状态。
    /// </summary>
    /// <param name="state">目标状态</param>
    public void SetState(QuestState state)
    {
        State = state;
    }

    /// <summary>
    /// 处理任务进度事件。
    /// </summary>
    /// <param name="progressEvent">任务进度事件</param>
    /// <returns>是否发生进度变化</returns>
    public bool HandleProgress(QuestProgressEvent progressEvent)
    {
        if (State != QuestState.Active || progressEvent == null)
        {
            return false;
        }

        ObjectiveRuntime activeObjective = GetActiveObjective();
        return activeObjective != null && activeObjective.HandleProgress(Id, progressEvent);
    }

    /// <summary>
    /// 尝试完成当前开启的目标。
    /// </summary>
    /// <returns>完成成功返回true</returns>
    public bool TryCompleteActiveObjective()
    {
        if (State != QuestState.Active)
        {
            return false;
        }

        ObjectiveRuntime activeObjective = GetActiveObjective();
        return activeObjective != null && activeObjective.TryComplete(Id);
    }

    /// <summary>
    /// 启动任务内的时间条件计时。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool StartTimeConditions(long utcSeconds)
    {
        if (State != QuestState.Active)
        {
            return false;
        }

        ObjectiveRuntime activeObjective = GetActiveObjective();
        return activeObjective != null && activeObjective.StartTimeConditions(utcSeconds);
    }

    /// <summary>
    /// 刷新任务内的时间条件进度。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshTimeProgress(long utcSeconds)
    {
        if (State != QuestState.Active)
        {
            return false;
        }

        ObjectiveRuntime activeObjective = GetActiveObjective();
        return activeObjective != null && activeObjective.RefreshTimeProgress(Id, utcSeconds);
    }

    /// <summary>
    /// 刷新任务内的背包道具数量条件进度。
    /// </summary>
    /// <returns>是否发生变化</returns>
    public bool RefreshItemNumProgress()
    {
        if (State != QuestState.Active && State != QuestState.Completed)
        {
            return false;
        }

        ObjectiveRuntime activeObjective = GetActiveObjective();
        return activeObjective != null && activeObjective.RefreshItemNumProgress(Id);
    }

    /// <summary>
    /// 获取当前开启的目标索引。
    /// </summary>
    /// <returns>当前目标索引，全部完成时返回-1</returns>
    public int GetActiveObjectiveIndex()
    {
        for (int i = 0; i < Objectives.Count; i++)
        {
            if (!Objectives[i].IsCompleted)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 获取当前开启的任务目标。
    /// </summary>
    /// <returns>当前开启目标，全部完成时返回null</returns>
    public ObjectiveRuntime GetActiveObjective()
    {
        int activeIndex = GetActiveObjectiveIndex();
        return activeIndex >= 0 ? Objectives[activeIndex] : null;
    }

    /// <summary>
    /// 获取指定目标运行时数据。
    /// </summary>
    /// <param name="objectiveId">目标ID</param>
    /// <returns>目标运行时数据</returns>
    public ObjectiveRuntime GetObjective(string objectiveId)
    {
        if (string.IsNullOrEmpty(objectiveId))
        {
            return null;
        } 

        for (int i = 0; i < Objectives.Count; i++)
        {
            if (Objectives[i].Id == objectiveId)
            {
                return Objectives[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 判断任务目标是否全部完成。
    /// </summary>
    /// <returns>全部完成返回true</returns>
    public bool IsAllObjectivesCompleted()
    {
        if (Objectives.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < Objectives.Count; i++)
        {
            if (!Objectives[i].IsCompleted)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 将运行时状态写回存档对象。
    /// </summary>
    /// <param name="saveItem">存档对象</param>
    public void WriteToSave(SaveQuestItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.questId = Id;
        saveItem.state = State;
        saveItem.objectives ??= new List<SaveQuestObjectiveItem>();
        saveItem.objectives.Clear();
        for (int i = 0; i < Objectives.Count; i++)
        {
            SaveQuestObjectiveItem objectiveItem = new SaveQuestObjectiveItem();
            Objectives[i].WriteToSave(objectiveItem);
            saveItem.objectives.Add(objectiveItem);
        }
    }

    private void BuildObjectives(SaveQuestItem saveItem)
    {
        Objectives.Clear();
        if (Data?.Objectives == null)
        {
            return;
        }

        for (int i = 0; i < Data.Objectives.Count; i++)
        {
            QuestObjectiveData objectiveData = Data.Objectives[i];
            string objectiveId = ResolveRuntimeId(objectiveData.Id, $"Objective_{i}");
            SaveQuestObjectiveItem objectiveSave = saveItem?.GetObjective(objectiveId);
            Objectives.Add(new ObjectiveRuntime(objectiveData, objectiveSave, objectiveId));
        }
    }

    private static string ResolveRuntimeId(string configId, string fallbackId)
    {
        return string.IsNullOrEmpty(configId) ? fallbackId : configId;
    }
}

/// <summary>
/// 任务目标运行时数据。
/// </summary>
public class ObjectiveRuntime
{
    private readonly string runtimeId;

    public QuestObjectiveData Data { get; }
    public string Id => runtimeId;
    public bool IsCompleted { get; private set; }
    public bool IsConditionsSatisfied => CheckConditionsSatisfied();
    public List<ConditionRuntime> Conditions { get; } = new List<ConditionRuntime>();

    public ObjectiveRuntime(QuestObjectiveData data, SaveQuestObjectiveItem saveItem, string runtimeId)
    {
        this.runtimeId = runtimeId;
        Data = data;
        IsCompleted = saveItem != null && saveItem.isCompleted;
        BuildConditions(saveItem);
    }

    /// <summary>
    /// 处理目标进度事件。
    /// </summary>
    /// <param name="questId">所属任务ID</param>
    /// <param name="progressEvent">任务进度事件</param>
    /// <returns>是否发生进度变化</returns>
    public bool HandleProgress(string questId, QuestProgressEvent progressEvent)
    {
        if (IsCompleted)
        {
            return false;
        }

        bool changed = false;
        for (int i = 0; i < Conditions.Count; i++)
        {
            changed |= Conditions[i].HandleProgress(progressEvent);
        }

        if (!changed)
        {
            return false;
        }

        bool oldCompleted = IsCompleted;
        SendObjectiveChanged(questId);
        return oldCompleted != IsCompleted || changed;
    }

    /// <summary>
    /// 启动目标内的时间条件计时。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool StartTimeConditions(long utcSeconds)
    {
        if (IsCompleted)
        {
            return false;
        }

        bool changed = false;
        for (int i = 0; i < Conditions.Count; i++)
        {
            changed |= Conditions[i].StartTimeCondition(utcSeconds);
        }

        return changed;
    }

    /// <summary>
    /// 刷新目标内的时间条件进度。
    /// </summary>
    /// <param name="questId">所属任务ID</param>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshTimeProgress(string questId, long utcSeconds)
    {
        if (IsCompleted)
        {
            return false;
        }

        bool changed = false;
        for (int i = 0; i < Conditions.Count; i++)
        {
            changed |= Conditions[i].RefreshTimeProgress(utcSeconds);
        }

        if (!changed)
        {
            return false;
        }

        bool oldCompleted = IsCompleted;
        SendObjectiveChanged(questId);
        return oldCompleted != IsCompleted || changed;
    }

    /// <summary>
    /// 刷新目标内的背包道具数量条件进度。
    /// </summary>
    /// <param name="questId">所属任务ID</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshItemNumProgress(string questId)
    {
        if (IsCompleted)
        {
            return false;
        }

        bool changed = false;
        for (int i = 0; i < Conditions.Count; i++)
        {
            changed |= Conditions[i].RefreshItemNumProgress();
        }

        if (!changed)
        {
            return false;
        }

        bool oldCompleted = IsCompleted;
        SendObjectiveChanged(questId);
        return oldCompleted != IsCompleted || changed;
    }

    /// <summary>
    /// 获取指定条件运行时数据。
    /// </summary>
    /// <param name="conditionId">条件ID</param>
    /// <returns>条件运行时数据</returns>
    public ConditionRuntime GetCondition(string conditionId)
    {
        if (string.IsNullOrEmpty(conditionId))
        {
            return null;
        }

        for (int i = 0; i < Conditions.Count; i++)
        {
            if (Conditions[i].Id == conditionId)
            {
                return Conditions[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 将目标运行时状态写回存档对象。
    /// </summary>
    /// <param name="saveItem">存档对象</param>
    public void WriteToSave(SaveQuestObjectiveItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.objectiveId = Id;
        saveItem.isCompleted = IsCompleted;
        saveItem.conditions ??= new List<SaveQuestConditionItem>();
        saveItem.conditions.Clear();
        for (int i = 0; i < Conditions.Count; i++)
        {
            SaveQuestConditionItem conditionItem = new SaveQuestConditionItem();
            Conditions[i].WriteToSave(conditionItem);
            saveItem.conditions.Add(conditionItem);
        }
    }

    private void BuildConditions(SaveQuestObjectiveItem saveItem)
    {
        Conditions.Clear();
        if (Data?.Conditions == null)
        {
            return;
        }

        for (int i = 0; i < Data.Conditions.Count; i++)
        {
            QuestConditionData conditionData = Data.Conditions[i];
            string conditionId = string.IsNullOrEmpty(conditionData.Id) ? $"Condition_{i}" : conditionData.Id;
            SaveQuestConditionItem conditionSave = saveItem?.GetCondition(conditionId);
            Conditions.Add(new ConditionRuntime(conditionData, conditionSave, conditionId));
        }
    }

    /// <summary>
    /// 收集目标内的道具数量条件。
    /// </summary>
    /// <param name="results">道具数量条件列表</param>
    public void CollectItemNumConditions(List<ConditionRuntime> results)
    {
        if (results == null)
        {
            return;
        }

        for (int i = 0; i < Conditions.Count; i++)
        {
            if (Conditions[i].IsItemNumCondition)
            {
                results.Add(Conditions[i]);
            }
        }
    }

    /// <summary>
    /// 尝试确认完成目标。
    /// </summary>
    /// <param name="questId">所属任务ID</param>
    /// <returns>完成成功返回true</returns>
    public bool TryComplete(string questId)
    {
        if (IsCompleted)
        {
            return false;
        }

        if (!IsConditionsSatisfied)
        {
            return false;
        }

        IsCompleted = true;
        SendObjectiveChanged(questId);
        return true;
    }

    private bool CheckConditionsSatisfied()
    {
        if (Conditions.Count == 0)
        {
            return false;
        }

        if (Data.ConditionGroupType == QuestConditionGroupType.Or)
        {
            return Conditions.Exists(condition => condition.IsCompleted);
        }

        return Conditions.TrueForAll(condition => condition.IsCompleted);
    }

    private void SendObjectiveChanged(string questId)
    {
        ConditionRuntime displayCondition = Conditions.Count > 0 ? Conditions[0] : null;
        new QuestObjectiveChangedEvent
        {
            QuestId = questId,
            ObjectiveId = Id,
            CurrentCount = displayCondition != null ? displayCondition.CurrentCount : 0,
            TargetCount = displayCondition != null ? displayCondition.TargetCount : 0,
            IsConditionsSatisfied = IsConditionsSatisfied,
            IsCompleted = IsCompleted,
            Objective = this
        }.SendEvent();
    }
}

/// <summary>
/// 任务条件运行时数据。
/// </summary>
public class ConditionRuntime
{
    private readonly string runtimeId;
    private long startUtcSeconds;

    public QuestConditionData Data { get; }
    public string Id => runtimeId;
    public float CurrentCount { get; private set; }
    public int TargetCount => Data != null ? Math.Max(1, Data.TargetCount) : 1;
    public bool IsCompleted => CurrentCount >= TargetCount;
    public bool IsItemNumCondition => Data != null && Data.EventType == QuestProgressEventType.ItemNum;

    public ConditionRuntime(QuestConditionData data, SaveQuestConditionItem saveItem, string runtimeId)
    {
        this.runtimeId = runtimeId;
        Data = data;
        CurrentCount = saveItem != null ? Math.Max(0, saveItem.currentCount) : 0;
        startUtcSeconds = saveItem != null ? Math.Max(0, saveItem.startUtcSeconds) : 0;
        CurrentCount = Math.Min(CurrentCount, TargetCount);
    }

    /// <summary>
    /// 处理条件进度事件。
    /// </summary>
    /// <param name="progressEvent">任务进度事件</param>
    /// <returns>是否发生进度变化</returns>
    public bool HandleProgress(QuestProgressEvent progressEvent)
    {
        if (Data == null || progressEvent == null || IsCompleted)
        {
            return false;
        }

        // 时间条件由真实UTC时间自动刷新，避免业务事件和离线补偿重复计数。
        if (IsTimeCondition())
        {
            return false;
        }

        // 背包数量条件只读取BagMgr当前数量，业务事件仅作为刷新触发。
        if (IsItemNumCondition)
        {
            return false;
        }

        if (Data.EventType != progressEvent.EventType)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(Data.TargetId) && Data.TargetId != progressEvent.TargetId)
        {
            return false;
        }

        float addCount = Data.ConditionType == QuestConditionType.EventOnce ? TargetCount : Math.Max(1, progressEvent.Amount);
        float oldCount = CurrentCount;
        CurrentCount = Math.Min(TargetCount, CurrentCount + addCount);
        return !Mathf.Approximately(CurrentCount, oldCount);
    }

    /// <summary>
    /// 启动时间条件计时。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool StartTimeCondition(long utcSeconds)
    {
        if (!IsTimeCondition() || IsCompleted || startUtcSeconds > 0)
        {
            return false;
        }

        startUtcSeconds = Math.Max(0, utcSeconds);
        return true;
    }

    /// <summary>
    /// 按真实经过时间刷新时间条件进度。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshTimeProgress(long utcSeconds)
    {
        if (!IsTimeCondition() || IsCompleted)
        {
            return false;
        }

        if (startUtcSeconds <= 0)
        {
            startUtcSeconds = Math.Max(0, utcSeconds);
            return true;
        }

        long elapsedSeconds = Math.Max(0, utcSeconds - startUtcSeconds);
        int elapsedMinutes = (int)(elapsedSeconds / 60);
        float oldCount = CurrentCount;
        CurrentCount = Math.Min(TargetCount, elapsedMinutes);
        return !Mathf.Approximately(CurrentCount, oldCount);
    }

    /// <summary>
    /// 获取时间条件剩余秒数。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>剩余秒数，非时间条件返回0</returns>
    public long GetRemainSeconds(long utcSeconds)
    {
        if (!IsTimeCondition())
        {
            return 0;
        }

        long targetSeconds = (long)TargetCount * 60L;
        if (startUtcSeconds <= 0)
        {
            return targetSeconds;
        }

        long elapsedSeconds = Math.Max(0, utcSeconds - startUtcSeconds);
        return Math.Max(0, targetSeconds - elapsedSeconds);
    }

    /// <summary>
    /// 按背包当前道具数量刷新条件进度。
    /// </summary>
    /// <returns>是否发生变化</returns>
    public bool RefreshItemNumProgress()
    {
        if (!IsItemNumCondition)
        {
            return false;
        }

        int propId;
        if (!int.TryParse(Data.TargetId, out propId))
        {
            float tempOldCount = CurrentCount;
            CurrentCount = 0;
            return !Mathf.Approximately(CurrentCount, tempOldCount);
        }

        // if (BagMgr.Instance == null)
        // {
        //     return false;
        // }

        float oldCount = CurrentCount;
        // int bagCount = Math.Max(0, (int)Math.Floor(BagMgr.Instance.GetBagPropCount(propId)));
        int bagCount = Math.Max(0, TargetCount);
        CurrentCount = Math.Min(TargetCount, bagCount);
        return !Mathf.Approximately(CurrentCount, oldCount);
    }

    /// <summary>
    /// 尝试解析背包道具ID。
    /// </summary>
    /// <param name="propId">道具ID</param>
    /// <returns>解析成功返回true</returns>
    public bool TryGetItemNumPropId(out int propId)
    {
        propId = 0;
        return IsItemNumCondition && int.TryParse(Data.TargetId, out propId);
    }

    /// <summary>
    /// 将条件运行时状态写回存档对象。
    /// </summary>
    /// <param name="saveItem">存档对象</param>
    public void WriteToSave(SaveQuestConditionItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.conditionId = Id;
        saveItem.currentCount = CurrentCount;
        saveItem.startUtcSeconds = startUtcSeconds;
    }

    private bool IsTimeCondition()
    {
        return Data != null && Data.EventType == QuestProgressEventType.Time;
    }

}
