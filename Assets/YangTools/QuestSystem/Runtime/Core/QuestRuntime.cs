using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务运行时数据
/// </summary>
public sealed class QuestRuntime
{
    private readonly Action<QuestObjectiveChangedEvent> objectiveChanged;

    /// <summary>
    /// 对应的任务配置
    /// </summary>
    public QuestData Data { get; }

    /// <summary>
    /// 任务稳定ID
    /// </summary>
    public string Id => Data != null ? Data.Id : string.Empty;

    /// <summary>
    /// 当前任务状态
    /// </summary>
    public QuestState State { get; private set; }

    /// <summary>
    /// 任务目标运行时列表
    /// </summary>
    public List<ObjectiveRuntime> Objectives { get; } = new List<ObjectiveRuntime>();

    /// <summary>
    /// 创建任务运行时数据
    /// </summary>
    public QuestRuntime(QuestData data, QuestSaveItem saveItem,
        Action<QuestObjectiveChangedEvent> objectiveChanged = null,
        IQuestItemService itemService = null)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        this.objectiveChanged = objectiveChanged;
        State = ResolveSavedState(saveItem);
        BuildObjectives(saveItem, itemService);
    }

    /// <summary>
    /// 设置任务状态
    /// </summary>
    public void SetState(QuestState state)
    {
        if (!Enum.IsDefined(typeof(QuestState), state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "无效的任务状态");
        }

        State = state;
    }

    /// <summary>
    /// 初始化当前目标条件
    /// </summary>
    public void InitializeActiveObjective()
    {
        GetActiveObjective()?.Initialize();
    }

    /// <summary>
    /// 释放全部目标条件监听
    /// </summary>
    public void DisposeConditions()
    {
        for (int i = 0; i < Objectives.Count; i++)
        {
            Objectives[i]?.Dispose();
        }
    }

    /// <summary>
    /// 重置每日任务进度
    /// </summary>
    /// <param name="activate">是否保持进行中状态</param>
    public void ResetForDailyRefresh(bool activate = true)
    {
        DisposeConditions();
        State = activate ? QuestState.Active : QuestState.Locked;
        for (int i = 0; i < Objectives.Count; i++)
        {
            Objectives[i].ResetRuntimeState();
        }

        if (State == QuestState.Active)
        {
            InitializeActiveObjective();
        }
    }

    /// <summary>
    /// 兼容旧调用的进度处理入口
    /// </summary>
    public bool HandleProgress(QuestProgressEvent progressEvent)
    {
        return false;
    }

    /// <summary>
    /// 尝试完成当前开启的目标
    /// </summary>
    public bool TryCompleteActiveObjective()
    {
        ObjectiveRuntime activeObjective = GetActiveObjective();
        return activeObjective != null && activeObjective.TryComplete(Id);
    }

    /// <summary>
    /// 兼容旧时间条件刷新接口
    /// </summary>
    public bool StartTimeConditions(long utcSeconds)
    {
        return false;
    }

    /// <summary>
    /// 兼容旧时间条件刷新接口
    /// </summary>
    public bool RefreshTimeProgress(long utcSeconds)
    {
        return false;
    }

    /// <summary>
    /// 刷新当前目标的背包条件
    /// </summary>
    public bool RefreshItemNumProgress(IQuestItemService itemService)
    {
        ObjectiveRuntime objective = GetActiveObjective();
        return objective != null && objective.RefreshItemNumProgress(Id, itemService);
    }

    /// <summary>
    /// 获取当前开启的目标索引
    /// </summary>
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
    /// 获取当前开启的任务目标
    /// </summary>
    public ObjectiveRuntime GetActiveObjective()
    {
        int index = GetActiveObjectiveIndex();
        return index >= 0 ? Objectives[index] : null;
    }

    /// <summary>
    /// 获取指定目标运行时数据
    /// </summary>
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
    /// 判断所有目标是否完成
    /// </summary>
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
    /// 将运行时状态写入存档
    /// </summary>
    public void WriteToSave(QuestSaveItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.questId = Id;
        saveItem.state = State;
        saveItem.objectives ??= new List<QuestSaveObjectiveItem>();
        saveItem.objectives.Clear();
        for (int i = 0; i < Objectives.Count; i++)
        {
            QuestSaveObjectiveItem objectiveItem = new QuestSaveObjectiveItem();
            Objectives[i].WriteToSave(objectiveItem);
            saveItem.objectives.Add(objectiveItem);
        }
    }

    /// <summary>
    /// 构建任务目标运行时数据
    /// </summary>
    private void BuildObjectives(QuestSaveItem saveItem, IQuestItemService itemService)
    {
        if (Data.Objectives == null)
        {
            return;
        }

        HashSet<string> usedIds = new HashSet<string>(StringComparer.Ordinal);
        int runtimeIndex = 0;
        for (int i = 0; i < Data.Objectives.Count; i++)
        {
            QuestObjectiveData objectiveData = Data.Objectives[i];
            if (objectiveData == null)
            {
                continue;
            }

            string objectiveId = QuestRuntimeIdUtility.ResolveUniqueId(
                string.Empty, $"Objective_{i}", usedIds, "任务目标");
            QuestSaveObjectiveItem objectiveSave = saveItem?.GetObjective(objectiveId)
                ?? saveItem?.GetObjectiveAt(runtimeIndex);
            Objectives.Add(new ObjectiveRuntime(objectiveData, objectiveSave, objectiveId, Id,
                objectiveChanged, itemService));
            runtimeIndex++;
        }
    }

    private static QuestState ResolveSavedState(QuestSaveItem saveItem)
    {
        return saveItem != null && Enum.IsDefined(typeof(QuestState), saveItem.state)
            ? saveItem.state
            : QuestState.Locked;
    }
}

/// <summary>
/// 任务目标运行时数据
/// </summary>
public sealed class ObjectiveRuntime
{
    private readonly string runtimeId;
    private readonly string questId;
    private readonly Action<QuestObjectiveChangedEvent> objectiveChanged;
    private readonly IQuestItemService itemService;

    /// <summary>
    /// 对应的目标配置
    /// </summary>
    public QuestObjectiveData Data { get; }

    /// <summary>
    /// 目标运行时ID
    /// </summary>
    public string Id => runtimeId;

    /// <summary>
    /// 目标是否已经完成
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// 目标的根条件运行时数据
    /// </summary>
    public ConditionRuntime Condition { get; private set; }

    /// <summary>
    /// 根条件是否满足
    /// </summary>
    public bool IsConditionsSatisfied => Condition != null && Condition.IsCompleted;

    /// <summary>
    /// 创建任务目标运行时数据
    /// </summary>
    public ObjectiveRuntime(QuestObjectiveData data, QuestSaveObjectiveItem saveItem, string runtimeId,
        string questId,
        Action<QuestObjectiveChangedEvent> objectiveChanged = null, IQuestItemService itemService = null)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        this.runtimeId = string.IsNullOrWhiteSpace(runtimeId)
            ? throw new ArgumentException("目标运行时ID不能为空", nameof(runtimeId))
            : runtimeId;
        this.questId = questId ?? string.Empty;
        this.objectiveChanged = objectiveChanged;
        this.itemService = itemService ?? NullQuestItemService.Instance;
        IsCompleted = saveItem != null && saveItem.isCompleted;
        BuildConditions(saveItem);
    }

    /// <summary>
    /// 初始化目标条件
    /// </summary>
    public void Initialize()
    {
        Condition?.Initialize();
    }

    /// <summary>
    /// 释放目标条件
    /// </summary>
    public void Dispose()
    {
        Condition?.Dispose();
    }

    /// <summary>
    /// 重置目标及条件状态
    /// </summary>
    public void ResetRuntimeState()
    {
        IsCompleted = false;
        Condition?.ResetRuntimeState();
    }

    /// <summary>
    /// 刷新道具数量条件
    /// </summary>
    public bool RefreshItemNumProgress(string questId, IQuestItemService service)
    {
        bool changed = Condition != null && Condition.RefreshItemNumProgress(service ?? itemService);

        if (changed)
        {
            SendObjectiveChanged(questId);
        }

        return changed;
    }

    /// <summary>
    /// 获取指定条件运行时数据
    /// </summary>
    public ConditionRuntime GetCondition(string conditionId)
    {
        if (string.IsNullOrEmpty(conditionId))
        {
            return null;
        }

        return Condition != null && Condition.Id == conditionId ? Condition : null;
    }

    /// <summary>
    /// 收集目标内的道具数量条件
    /// </summary>
    public void CollectItemNumConditions(List<ConditionRuntime> results)
    {
        if (results == null)
        {
            return;
        }

        if (Condition != null && Condition.IsItemNumCondition)
        {
            results.Add(Condition);
        }
    }

    /// <summary>
    /// 收集目标中全部层级的道具数量条件
    /// </summary>
    public void CollectItemNumConditions(List<ItemNumCondition> results)
    {
        if (results == null)
        {
            return;
        }

        Condition?.CollectItemNumConditions(results);
    }

    /// <summary>
    /// 尝试确认完成目标
    /// </summary>
    public bool TryComplete(string questId)
    {
        if (IsCompleted || !IsConditionsSatisfied)
        {
            return false;
        }

        IsCompleted = true;
        SendObjectiveChanged(questId);
        return true;
    }

    /// <summary>
    /// 将目标状态写入存档
    /// </summary>
    public void WriteToSave(QuestSaveObjectiveItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.objectiveId = Id;
        saveItem.isCompleted = IsCompleted;
        saveItem.conditions ??= new List<QuestSaveConditionItem>();
        saveItem.conditions.Clear();
        if (Condition != null)
        {
            QuestSaveConditionItem conditionItem = new QuestSaveConditionItem();
            Condition.WriteToSave(conditionItem);
            saveItem.conditions.Add(conditionItem);
        }
    }

    private void BuildConditions(QuestSaveObjectiveItem saveItem)
    {
        if (Data.Condition == null)
        {
            return;
        }

        const string conditionId = "Condition_0";
        QuestSaveConditionItem conditionSave = saveItem?.GetCondition(conditionId)
            ?? saveItem?.GetConditionAt(0);
        Condition = new ConditionRuntime(Data.Condition, conditionSave, conditionId, itemService,
            OnConditionChanged);
    }

    private void OnConditionChanged()
    {
        SendObjectiveChanged(questId);
    }

    private void SendObjectiveChanged(string questId)
    {
        ConditionRuntime displayCondition = Condition;
        objectiveChanged?.Invoke(new QuestObjectiveChangedEvent
        {
            QuestId = questId,
            ObjectiveId = Id,
            CurrentCount = displayCondition != null ? displayCondition.CurrentCount : 0f,
            TargetCount = displayCondition != null ? displayCondition.TargetCount : 0,
            IsConditionsSatisfied = IsConditionsSatisfied,
            IsCompleted = IsCompleted,
            Objective = this
        });
    }
}

/// <summary>
/// 任务条件运行时视图
/// </summary>
public sealed class ConditionRuntime
{
    private readonly string runtimeId;
    private readonly Condition condition;
    private readonly IQuestItemService itemService;
    private bool initialized;

    /// <summary>
    /// 当前条件配置的运行时副本
    /// </summary>
    public Condition Data => condition;

    /// <summary>
    /// 当前条件对象
    /// </summary>
    public Condition Condition => condition;

    /// <summary>
    /// 条件运行时ID
    /// </summary>
    public string Id => runtimeId;

    /// <summary>
    /// 当前展示进度
    /// </summary>
    public float CurrentCount => condition != null ? condition.CurrentCount : 0f;

    /// <summary>
    /// 当前展示目标数量
    /// </summary>
    public int TargetCount => condition != null ? Mathf.Max(1, condition.TargetCount) : 1;

    /// <summary>
    /// 当前在线时长秒数
    /// </summary>
    public float OnlineTimeSeconds => condition != null ? condition.OnlineTimeSeconds : 0f;

    /// <summary>
    /// 条件是否已经完成
    /// </summary>
    public bool IsCompleted => condition != null && condition.IsCompleted;

    /// <summary>
    /// 当前条件是否为道具数量条件
    /// </summary>
    public bool IsItemNumCondition => condition is ItemNumCondition;

    /// <summary>
    /// 当前条件是否为在线时长条件
    /// </summary>
    public bool IsOnlineTimeCondition => condition is OnlineTimeCondition;

    /// <summary>
    /// 当前条件是否为任一子条件满足的组合条件
    /// </summary>
    public bool IsOrComposite => condition is OrCondition;

    /// <summary>
    /// 当前条件关联的目标ID
    /// </summary>
    public string TargetId => condition is ProgressCondition progress ? progress.TargetId : GetTargetId(condition);

    /// <summary>
    /// 创建条件运行时视图
    /// </summary>
    public ConditionRuntime(Condition prototype, QuestSaveConditionItem saveItem, string runtimeId,
        IQuestItemService itemService, Action changed)
    {
        condition = prototype?.CreateRuntimeCopy()
            ?? throw new ArgumentNullException(nameof(prototype));
        this.runtimeId = string.IsNullOrWhiteSpace(runtimeId)
            ? throw new ArgumentException("条件运行时ID不能为空", nameof(runtimeId))
            : runtimeId;
        this.itemService = itemService ?? NullQuestItemService.Instance;
        condition.SetServicesForChildren(this.itemService);
        condition.RestoreState(saveItem);
        condition.Changed += changed;
    }

    /// <summary>
    /// 初始化条件
    /// </summary>
    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        condition.Initialize();
    }

    /// <summary>
    /// 释放条件
    /// </summary>
    public void Dispose()
    {
        if (!initialized)
        {
            return;
        }

        initialized = false;
        condition.Dispose();
    }

    /// <summary>
    /// 重置条件状态
    /// </summary>
    public void ResetRuntimeState()
    {
        condition.ResetRuntimeState();
    }

    /// <summary>
    /// 刷新道具数量条件
    /// </summary>
    public bool RefreshItemNumProgress(IQuestItemService service)
    {
        condition.SetServicesForChildren(service ?? itemService);
        return condition.RefreshItemNumProgress(service ?? itemService);
    }

    /// <summary>
    /// 收集条件内部全部层级的道具数量条件
    /// </summary>
    public void CollectItemNumConditions(List<ItemNumCondition> results)
    {
        condition.CollectItemNumConditions(results);
    }

    /// <summary>
    /// 获取背包道具ID
    /// </summary>
    public bool TryGetItemNumPropId(out int propId)
    {
        propId = 0;
        return condition is ItemNumCondition itemNumCondition
               && int.TryParse(itemNumCondition.ItemId, out propId) && propId > 0;
    }

    /// <summary>
    /// 将条件状态写入存档
    /// </summary>
    public void WriteToSave(QuestSaveConditionItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.conditionId = Id;
        condition.WriteState(saveItem);
    }

    private static string GetTargetId(Condition value)
    {
        if (value is ItemNumCondition itemNumCondition)
        {
            return itemNumCondition.ItemId;
        }

        return value is KillCondition killCondition ? killCondition.MonsterId : string.Empty;
    }
}

/// <summary>
/// 任务运行时ID工具
/// </summary>
internal static class QuestRuntimeIdUtility
{
    /// <summary>
    /// 生成列表内唯一的运行时ID
    /// </summary>
    public static string ResolveUniqueId(string configuredId, string fallbackId, HashSet<string> usedIds,
        string idType)
    {
        string baseId = string.IsNullOrWhiteSpace(configuredId) ? fallbackId : configuredId;
        if (usedIds.Add(baseId))
        {
            return baseId;
        }

        int suffix = 1;
        string uniqueId;
        do
        {
            uniqueId = $"{baseId}_{suffix++}";
        } while (!usedIds.Add(uniqueId));

        Debug.LogWarning($"{idType}ID重复 {baseId} 已自动使用 {uniqueId}");
        return uniqueId;
    }
}
