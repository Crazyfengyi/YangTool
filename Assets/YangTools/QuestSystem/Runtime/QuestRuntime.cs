using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务运行时数据
/// </summary>
public class QuestRuntime
{
    private readonly Action<QuestObjectiveChangedEvent> objectiveChanged; //目标变化回调

    public QuestData Data { get; }
    public string Id => Data != null ? Data.Id : string.Empty;
    public QuestState State { get; private set; }
    public List<ObjectiveRuntime> Objectives { get; } = new List<ObjectiveRuntime>();

    /// <summary>
    /// 创建任务运行时数据
    /// </summary>
    /// <param name="data">任务配置</param>
    /// <param name="saveItem">任务存档</param>
    /// <param name="objectiveChanged">目标变化回调</param>
    public QuestRuntime(QuestData data, QuestSaveItem saveItem,
        Action<QuestObjectiveChangedEvent> objectiveChanged = null)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        this.objectiveChanged = objectiveChanged;
        State = ResolveSavedState(saveItem);
        BuildObjectives(saveItem);
    }

    #region 任务进度

    /// <summary>
    /// 设置任务状态。
    /// </summary>
    /// <param name="state">目标状态</param>
    public void SetState(QuestState state)
    {
        if (!Enum.IsDefined(typeof(QuestState), state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "无效的任务状态");
        }

        State = state;
    }

    /// <summary>
    /// 重置每日任务进度
    /// </summary>
    /// <param name="activate">是否保持进行中状态</param>
    public void ResetForDailyRefresh(bool activate = true)
    {
        State = activate ? QuestState.Active : QuestState.Locked;
        BuildObjectives(null);
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
    /// <param name="itemService">任务道具服务</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshItemNumProgress(IQuestItemService itemService)
    {
        if (State != QuestState.Active && State != QuestState.Completed)
        {
            return false;
        }

        ObjectiveRuntime activeObjective = GetActiveObjective();
        return activeObjective != null && activeObjective.RefreshItemNumProgress(Id, itemService);
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

    #endregion

    #region 存档

    /// <summary>
    /// 将运行时状态写回存档对象。
    /// </summary>
    /// <param name="saveItem">存档对象</param>
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
    /// 根据配置和存档构建任务目标
    /// </summary>
    /// <param name="saveItem">任务存档</param>
    private void BuildObjectives(QuestSaveItem saveItem)
    {
        Objectives.Clear();
        if (Data.Objectives == null)
        {
            return;
        }

        HashSet<string> runtimeIds = new HashSet<string>(StringComparer.Ordinal);
        int runtimeIndex = 0;
        for (int i = 0; i < Data.Objectives.Count; i++)
        {
            QuestObjectiveData objectiveData = Data.Objectives[i];
            if (objectiveData == null)
            {
                Debug.LogWarning($"任务 {Id} 的目标列表第 {i} 项为空 已跳过");
                continue;
            }

            // 目标不再要求人工ID 运行时使用列表索引生成内部键
            string objectiveId = QuestRuntimeIdUtility.ResolveUniqueId(
                string.Empty, $"Objective_{i}", runtimeIds, "任务目标");
            QuestSaveObjectiveItem objectiveSave = saveItem?.GetObjective(objectiveId)
                ?? saveItem?.GetObjectiveAt(runtimeIndex);
            Objectives.Add(new ObjectiveRuntime(objectiveData, objectiveSave, objectiveId, objectiveChanged));
            runtimeIndex++;
        }
    }

    /// <summary>
    /// 获得有效的存档任务状态
    /// </summary>
    /// <param name="saveItem">任务存档</param>
    /// <returns>有效任务状态</returns>
    private static QuestState ResolveSavedState(QuestSaveItem saveItem)
    {
        if (saveItem == null || !Enum.IsDefined(typeof(QuestState), saveItem.state))
        {
            return QuestState.Locked;
        }

        return saveItem.state;
    }

    #endregion
}

/// <summary>
/// 任务目标运行时数据。
/// </summary>
public class ObjectiveRuntime
{
    private readonly string runtimeId; //运行时稳定ID
    private readonly Action<QuestObjectiveChangedEvent> objectiveChanged; //目标变化回调
    public QuestObjectiveData Data { get; }
    public string Id => runtimeId;
    public bool IsCompleted { get; private set; }
    public bool IsConditionsSatisfied
    {
        get
        {
            if (Data == null || Conditions.Count == 0)
            {
                return false;
            }

            if (Data.ConditionGroupType == QuestConditionGroupType.Or)
            {
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (Conditions[i].IsCompleted)
                    {
                        return true;
                    }
                }

                return false;
            }

            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }
    }
    public List<ConditionRuntime> Conditions { get; } = new List<ConditionRuntime>();

    /// <summary>
    /// 创建任务目标运行时数据
    /// </summary>
    /// <param name="data">目标配置</param>
    /// <param name="saveItem">目标存档</param>
    /// <param name="runtimeId">运行时稳定ID</param>
    /// <param name="objectiveChanged">目标变化回调</param>
    public ObjectiveRuntime(QuestObjectiveData data, QuestSaveObjectiveItem saveItem, string runtimeId,
        Action<QuestObjectiveChangedEvent> objectiveChanged = null)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        this.runtimeId = string.IsNullOrWhiteSpace(runtimeId)
            ? throw new ArgumentException("目标运行时ID不能为空", nameof(runtimeId))
            : runtimeId;
        this.objectiveChanged = objectiveChanged;
        IsCompleted = saveItem != null && saveItem.isCompleted;
        BuildConditions(saveItem);
    }

    #region 目标进度

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

        SendObjectiveChanged(questId);
        return true;
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

        SendObjectiveChanged(questId);
        return true;
    }

    /// <summary>
    /// 刷新目标内的背包道具数量条件进度。
    /// </summary>
    /// <param name="questId">所属任务ID</param>
    /// <param name="itemService">任务道具服务</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshItemNumProgress(string questId, IQuestItemService itemService)
    {
        if (IsCompleted)
        {
            return false;
        }

        bool changed = false;
        for (int i = 0; i < Conditions.Count; i++)
        {
            changed |= Conditions[i].RefreshItemNumProgress(itemService);
        }

        if (!changed)
        {
            return false;
        }

        SendObjectiveChanged(questId);
        return true;
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
        for (int i = 0; i < Conditions.Count; i++)
        {
            QuestSaveConditionItem conditionItem = new QuestSaveConditionItem();
            Conditions[i].WriteToSave(conditionItem);
            saveItem.conditions.Add(conditionItem);
        }
    }

    /// <summary>
    /// 根据配置和存档构建任务条件
    /// </summary>
    /// <param name="saveItem">目标存档</param>
    private void BuildConditions(QuestSaveObjectiveItem saveItem)
    {
        Conditions.Clear();
        if (Data.Conditions == null)
        {
            return;
        }

        HashSet<string> runtimeIds = new HashSet<string>(StringComparer.Ordinal);
        int runtimeIndex = 0;
        for (int i = 0; i < Data.Conditions.Count; i++)
        {
            QuestConditionData conditionData = Data.Conditions[i];
            if (conditionData == null)
            {
                Debug.LogWarning($"目标 {Id} 的条件列表第 {i} 项为空 已跳过");
                continue;
            }

            // 条件不再要求人工ID 运行时使用列表索引生成内部键
            string conditionId = QuestRuntimeIdUtility.ResolveUniqueId(
                string.Empty, $"Condition_{i}", runtimeIds, "任务条件");
            QuestSaveConditionItem conditionSave = saveItem?.GetCondition(conditionId)
                ?? saveItem?.GetConditionAt(runtimeIndex);
            Conditions.Add(new ConditionRuntime(conditionData, conditionSave, conditionId));
            runtimeIndex++;
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

    /// <summary>
    /// 发送目标进度变化事件
    /// </summary>
    /// <param name="questId">所属任务ID</param>
    private void SendObjectiveChanged(string questId)
    {
        ConditionRuntime displayCondition = Conditions.Count > 0 ? Conditions[0] : null;
        QuestObjectiveChangedEvent changedEvent = new QuestObjectiveChangedEvent
        {
            QuestId = questId,
            ObjectiveId = Id,
            CurrentCount = displayCondition != null ? displayCondition.CurrentCount : 0,
            TargetCount = displayCondition != null ? displayCondition.TargetCount : 0,
            IsConditionsSatisfied = IsConditionsSatisfied,
            IsCompleted = IsCompleted,
            Objective = this
        };
        objectiveChanged?.Invoke(changedEvent);
    }

    #endregion
}

/// <summary>
/// 任务条件运行时数据。
/// </summary>
public class ConditionRuntime
{
    private readonly string runtimeId; //运行时稳定ID
    private long startUtcSeconds; //时间条件起始UTC秒数
    private float onlineTimeSeconds; //累计在线秒数

    public QuestConditionData Data { get; }
    public string Id => runtimeId;
    public float CurrentCount { get; private set; }
    public float OnlineTimeSeconds => IsOnlineTimeCondition() ? onlineTimeSeconds : 0f;
    public int TargetCount => Data != null ? Math.Max(1, Data.TargetCount) : 1;
    public bool IsCompleted => CurrentCount >= TargetCount;
    public bool IsItemNumCondition => Data != null && Data.EventType == QuestProgressEventType.ItemNum;

    /// <summary>
    /// 创建任务条件运行时数据
    /// </summary>
    /// <param name="data">条件配置</param>
    /// <param name="saveItem">条件存档</param>
    /// <param name="runtimeId">运行时稳定ID</param>
    public ConditionRuntime(QuestConditionData data, QuestSaveConditionItem saveItem, string runtimeId)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        this.runtimeId = string.IsNullOrWhiteSpace(runtimeId)
            ? throw new ArgumentException("条件运行时ID不能为空", nameof(runtimeId))
            : runtimeId;
        CurrentCount = NormalizeNonNegative(saveItem?.currentCount ?? 0f);
        startUtcSeconds = Math.Max(0L, saveItem?.startUtcSeconds ?? 0L);
        onlineTimeSeconds = NormalizeNonNegative(saveItem?.onlineTimeSeconds ?? 0f);
        if (IsOnlineTimeCondition())
        {
            onlineTimeSeconds = Math.Min(TargetCount, onlineTimeSeconds);
        }

        CurrentCount = Math.Min(CurrentCount, TargetCount);
    }

    #region 事件进度

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

        // 时间条件由真实UTC时间自动刷新
        if (IsTimeCondition())
        {
            return false;
        }

        // 道具数量条件只读取外部道具服务
        if (IsItemNumCondition)
        {
            return false;
        }

        if (IsOnlineTimeCondition())
        {
            return HandleOnlineTimeProgress(progressEvent);
        }

        if (Data.EventType != progressEvent.EventType)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(Data.TargetId)
            && !string.Equals(Data.TargetId, progressEvent.TargetId, StringComparison.Ordinal))
        {
            return false;
        }

        float eventAmount = NormalizePositive(progressEvent.Amount, 1f);
        float addCount = Data.ConditionType == QuestConditionType.EventOnce ? TargetCount : eventAmount;
        float oldCount = CurrentCount;
        CurrentCount = Math.Min(TargetCount, CurrentCount + addCount);
        return !Mathf.Approximately(CurrentCount, oldCount);
    }

    /// <summary>
    /// 累计在线时长事件，目标值和事件数值单位均为秒。
    /// </summary>
    /// <param name="progressEvent">在线时长进度事件</param>
    /// <returns>在线时长是否发生变化</returns>
    private bool HandleOnlineTimeProgress(QuestProgressEvent progressEvent)
    {
        if (progressEvent.EventType != QuestProgressEventType.OnLineTime
            || float.IsNaN(progressEvent.Value)
            || float.IsInfinity(progressEvent.Value)
            || progressEvent.Value <= 0f)
        {
            return false;
        }

        float oldSeconds = onlineTimeSeconds;
        float oldCount = CurrentCount;
        onlineTimeSeconds = Math.Min(TargetCount, onlineTimeSeconds + progressEvent.Value);
        CurrentCount = Mathf.Min(TargetCount, onlineTimeSeconds);
        return !Mathf.Approximately(onlineTimeSeconds, oldSeconds)
               || !Mathf.Approximately(CurrentCount, oldCount);
    }

    #endregion

    #region 时间进度

    /// <summary>
    /// 启动时间条件计时。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool StartTimeCondition(long utcSeconds)
    {
        if (!IsTimeCondition() || IsCompleted || startUtcSeconds > 0 || utcSeconds <= 0)
        {
            return false;
        }

        startUtcSeconds = utcSeconds;
        return true;
    }

    /// <summary>
    /// 按真实经过时间刷新时间条件进度。
    /// </summary>
    /// <param name="utcSeconds">当前UTC秒数</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshTimeProgress(long utcSeconds)
    {
        if (!IsTimeCondition() || IsCompleted || utcSeconds <= 0)
        {
            return false;
        }

        if (startUtcSeconds <= 0)
        {
            startUtcSeconds = utcSeconds;
            return true;
        }

        long elapsedSeconds = Math.Max(0, utcSeconds - startUtcSeconds);
        long elapsedMinutes = elapsedSeconds / 60L;
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

    #endregion

    #region 道具数量进度

    /// <summary>
    /// 按背包当前道具数量刷新条件进度。
    /// </summary>
    /// <param name="itemService">任务道具服务</param>
    /// <returns>是否发生变化</returns>
    public bool RefreshItemNumProgress(IQuestItemService itemService)
    {
        if (!IsItemNumCondition || itemService == null)
        {
            return false;
        }

        if (!TryGetItemNumPropId(out int propId))
        {
            float oldInvalidCount = CurrentCount;
            CurrentCount = 0;
            return !Mathf.Approximately(CurrentCount, oldInvalidCount);
        }

        float oldCount = CurrentCount;
        float bagCount = NormalizeNonNegative(itemService.GetItemCount(propId));
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
        return IsItemNumCondition && int.TryParse(Data.TargetId, out propId) && propId > 0;
    }

    #endregion

    #region 存档

    /// <summary>
    /// 将条件运行时状态写回存档对象。
    /// </summary>
    /// <param name="saveItem">存档对象</param>
    public void WriteToSave(QuestSaveConditionItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.conditionId = Id;
        saveItem.currentCount = CurrentCount;
        saveItem.startUtcSeconds = startUtcSeconds;
        saveItem.onlineTimeSeconds = onlineTimeSeconds;
    }

    /// <summary>
    /// 判断是否为真实时间条件
    /// </summary>
    /// <returns>真实时间条件返回true</returns>
    private bool IsTimeCondition()
    {
        return Data != null && Data.EventType == QuestProgressEventType.Time;
    }

    /// <summary>
    /// 判断是否为在线时长条件。
    /// </summary>
    /// <returns>在线时长条件返回true</returns>
    private bool IsOnlineTimeCondition()
    {
        return Data != null && Data.EventType == QuestProgressEventType.OnLineTime;
    }

    /// <summary>
    /// 将非法数值转换为非负数
    /// </summary>
    /// <param name="value">待转换数值</param>
    /// <returns>有效的非负数</returns>
    private static float NormalizeNonNegative(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Math.Max(0f, value);
    }

    /// <summary>
    /// 将非法数值转换为正数
    /// </summary>
    /// <param name="value">待转换数值</param>
    /// <param name="fallback">无效时的默认值</param>
    /// <returns>有效的正数</returns>
    private static float NormalizePositive(float value, float fallback)
    {
        return float.IsNaN(value) || float.IsInfinity(value) || value <= 0f ? fallback : value;
    }

    #endregion
}

/// <summary>
/// 任务运行时稳定ID工具
/// </summary>
internal static class QuestRuntimeIdUtility
{
    /// <summary>
    /// 生成当前列表内唯一的运行时ID
    /// </summary>
    /// <param name="configuredId">配置ID</param>
    /// <param name="fallbackId">配置为空时的默认ID</param>
    /// <param name="usedIds">已使用的ID集合</param>
    /// <param name="idType">ID类型名称</param>
    /// <returns>唯一运行时ID</returns>
    public static string ResolveUniqueId(string configuredId, string fallbackId, HashSet<string> usedIds,
        string idType)
    {
        if (usedIds == null)
        {
            throw new ArgumentNullException(nameof(usedIds));
        }

        string baseId = string.IsNullOrWhiteSpace(configuredId) ? fallbackId : configuredId;
        if (usedIds.Add(baseId))
        {
            return baseId;
        }

        int suffix = 1;
        string uniqueId;
        do
        {
            uniqueId = $"{baseId}_{suffix}";
            suffix++;
        } while (!usedIds.Add(uniqueId));

        Debug.LogWarning($"{idType}ID重复 {baseId} 已自动使用 {uniqueId}");
        return uniqueId;
    }
}
