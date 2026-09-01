using System;
using System.Collections.Generic;
using UnityEngine;

#region 基类

/// <summary>
/// 任务条件运行时基类
/// </summary>
[Serializable]
public abstract class Condition
{
    /// <summary>
    /// 条件是否已经完成
    /// </summary>
    [field: NonSerialized]
    public bool IsCompleted { get; protected set; }

    /// <summary>
    /// 条件进度或完成状态发生变化时触发
    /// </summary>
    [field: NonSerialized]
    public event Action Changed;

    [field: NonSerialized]
    protected IQuestItemService ItemService { get; private set; }

    /// <summary>
    /// 当前条件进度
    /// </summary>
    public virtual float CurrentCount => 0f;

    /// <summary>
    /// 条件目标进度
    /// </summary>
    public virtual int TargetCount => 1;

    /// <summary>
    /// 在线时长条件的秒数
    /// </summary>
    public virtual float OnlineTimeSeconds => 0f;

    /// <summary>
    /// 初始化条件事件监听
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// 释放条件事件监听
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// 重置条件运行时状态
    /// </summary>
    public virtual void ResetRuntimeState()
    {
        IsCompleted = false;
    }

    /// <summary>
    /// 创建配置条件的运行时副本
    /// </summary>
    /// <returns>运行时条件副本</returns>
    internal virtual Condition CreateRuntimeCopy()
    {
        Condition copy = CreateShallowRuntimeCopy();
        copy.ResetRuntimeState();
        return copy;
    }

    /// <summary>
    /// 创建只复制配置字段的浅运行时副本
    /// </summary>
    /// <returns>浅运行时副本</returns>
    internal Condition CreateShallowRuntimeCopy()
    {
        Condition copy = (Condition)MemberwiseClone();
        copy.Changed = null;
        copy.ItemService = null;
        copy.IsCompleted = false;
        return copy;
    }

    /// <summary>
    /// 注入运行时服务
    /// </summary>
    /// <param name="itemService">道具服务</param>
    internal void SetServices(IQuestItemService itemService)
    {
        ItemService = itemService ?? NullQuestItemService.Instance;
    }

    /// <summary>
    /// 设置条件及其子条件使用的运行时服务
    /// </summary>
    /// <param name="itemService">道具服务</param>
    internal virtual void SetServicesForChildren(IQuestItemService itemService)
    {
        SetServices(itemService);
    }

    /// <summary>
    /// 从存档恢复条件状态
    /// </summary>
    /// <param name="saveItem">条件存档</param>
    internal virtual void RestoreState(QuestSaveConditionItem saveItem)
    {
        IsCompleted = saveItem != null && saveItem.isCompleted;
    }

    /// <summary>
    /// 将条件状态写入存档
    /// </summary>
    /// <param name="saveItem">条件存档</param>
    internal virtual void WriteState(QuestSaveConditionItem saveItem)
    {
        if (saveItem != null)
        {
            saveItem.isCompleted = IsCompleted;
        }
    }

    /// <summary>
    /// 刷新当前条件及其子条件中的道具数量
    /// </summary>
    /// <param name="service">道具服务</param>
    /// <returns>是否发生变化</returns>
    internal virtual bool RefreshItemNumProgress(IQuestItemService service)
    {
        return false;
    }

    /// <summary>
    /// 收集当前条件及其子条件中的道具数量条件
    /// </summary>
    /// <param name="results">条件列表</param>
    internal virtual void CollectItemNumConditions(List<ItemNumCondition> results)
    {
    }

    /// <summary>
    /// 通知目标条件发生变化
    /// </summary>
    protected void MarkChanged()
    {
        Changed?.Invoke();
    }

    /// <summary>
    /// 设置条件完成状态并发送变化通知
    /// </summary>
    protected void Complete()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        MarkChanged();
    }
}

/// <summary>
/// 计数条件基类
/// </summary>
[Serializable]
public abstract class CountCondition : Condition
{
    /// <summary>
    /// 当前累计进度
    /// </summary>
    [NonSerialized]
    protected float currentCount;

    public override float CurrentCount => currentCount;

    public override int TargetCount => Mathf.Max(1, GetConfiguredTargetCount());

    /// <summary>
    /// 获取具体计数条件配置的目标数量
    /// </summary>
    /// <returns>目标数量</returns>
    protected virtual int GetConfiguredTargetCount()
    {
        return 1;
    }

    /// <summary>
    /// 增加条件进度
    /// </summary>
    /// <param name="amount">增加数量</param>
    protected void AddCount(float amount)
    {
        if (IsCompleted || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
        {
            return;
        }

        float oldCount = currentCount;
        currentCount = Mathf.Min(Mathf.Max(1, TargetCount), currentCount + amount);
        if (!Mathf.Approximately(oldCount, currentCount))
        {
            if (currentCount >= TargetCount)
            {
                IsCompleted = true;
            }

            MarkChanged();
        }
    }

    public override void ResetRuntimeState()
    {
        base.ResetRuntimeState();
        currentCount = 0f;
    }

    internal override void RestoreState(QuestSaveConditionItem saveItem)
    {
        currentCount = saveItem == null ? 0f : Mathf.Clamp(saveItem.currentCount, 0f, TargetCount);
        IsCompleted = saveItem != null && (saveItem.isCompleted || currentCount >= TargetCount);
    }

    internal override void WriteState(QuestSaveConditionItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.currentCount = currentCount;
        saveItem.isCompleted = IsCompleted;
    }
}

/// <summary>
/// 条件组合基类
/// </summary>
[Serializable]
public abstract class CompositeCondition : Condition
{
    /// <summary>
    /// 组合条件包含的子条件
    /// </summary>
    [SerializeReference] public List<Condition> Children = new List<Condition>();

    [NonSerialized] private bool initialized;

    public override float CurrentCount => Children != null && Children.Count > 0 ? Children[0].CurrentCount : 0f;
    public override int TargetCount => Children != null && Children.Count > 0 ? Children[0].TargetCount : 1;
    public override float OnlineTimeSeconds => Children != null && Children.Count > 0 ? Children[0].OnlineTimeSeconds : 0f;

    public override void Initialize()
    {
        if (initialized || Children == null)
        {
            return;
        }

        initialized = true;
        for (int i = 0; i < Children.Count; i++)
        {
            Condition child = Children[i];
            if (child == null)
            {
                continue;
            }

            child.Changed += OnChildChanged;
            child.SetServices(ItemService);
            child.Initialize();
        }

        UpdateCompletion();
    }

    public override void Dispose()
    {
        if (!initialized || Children == null)
        {
            return;
        }

        initialized = false;
        for (int i = 0; i < Children.Count; i++)
        {
            Condition child = Children[i];
            if (child == null)
            {
                continue;
            }

            child.Changed -= OnChildChanged;
            child.Dispose();
        }
    }

    internal override void SetServicesForChildren(IQuestItemService itemService)
    {
        base.SetServicesForChildren(itemService);
        if (Children == null)
        {
            return;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            Children[i]?.SetServicesForChildren(itemService);
        }
    }

    internal override bool RefreshItemNumProgress(IQuestItemService itemService)
    {
        bool changed = false;
        if (Children == null)
        {
            return false;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            changed |= Children[i]?.RefreshItemNumProgress(itemService) ?? false;
        }

        bool oldCompleted = IsCompleted;
        UpdateCompletion();
        if (oldCompleted != IsCompleted)
        {
            MarkChanged();
            changed = true;
        }

        return changed;
    }

    internal override void CollectItemNumConditions(List<ItemNumCondition> results)
    {
        if (Children == null || results == null)
        {
            return;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            Children[i]?.CollectItemNumConditions(results);
        }
    }

    internal override Condition CreateRuntimeCopy()
    {
        CompositeCondition copy = (CompositeCondition)CreateShallowRuntimeCopy();
        copy.initialized = false;
        copy.Children = new List<Condition>();
        if (Children != null)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                copy.Children.Add(Children[i]?.CreateRuntimeCopy());
            }
        }

        copy.ResetRuntimeState();

        return copy;
    }

    public override void ResetRuntimeState()
    {
        base.ResetRuntimeState();
        initialized = false;
        if (Children == null)
        {
            return;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            Children[i]?.ResetRuntimeState();
        }
    }

    protected abstract bool EvaluateCompletion();

    private void OnChildChanged()
    {
        UpdateCompletion();
        MarkChanged();
    }

    private void UpdateCompletion()
    {
        IsCompleted = EvaluateCompletion();
    }

    internal override void RestoreState(QuestSaveConditionItem saveItem)
    {
        if (Children != null)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i]?.RestoreState(saveItem?.children != null && i < saveItem.children.Count
                    ? saveItem.children[i]
                    : null);
            }
        }

        IsCompleted = EvaluateCompletion();
    }

    internal override void WriteState(QuestSaveConditionItem saveItem)
    {
        if (saveItem == null)
        {
            return;
        }

        saveItem.isCompleted = IsCompleted;
        saveItem.children = new List<QuestSaveConditionItem>();
        if (Children == null)
        {
            return;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            QuestSaveConditionItem childSave = new QuestSaveConditionItem();
            Children[i]?.WriteState(childSave);
            saveItem.children.Add(childSave);
        }
    }
}

#endregion

#region 条件实现

/// <summary>
/// 击杀条件
/// </summary>
[Serializable]
public sealed class KillCondition : CountCondition
{
    /// <summary>
    /// 需要击杀的怪物ID
    /// </summary>
    [InspectorName("怪物 ID")] public string MonsterId;

    /// <summary>
    /// 需要击杀的数量
    /// </summary>
    [InspectorName("目标数量")]
    [Min(1)] public new int TargetCount = 1;

    protected override int GetConfiguredTargetCount() => TargetCount;

    public override void Initialize()
    {
        QuestEventBus.OnMonsterKilled += OnMonsterKilled;
    }

    public override void Dispose()
    {
        QuestEventBus.OnMonsterKilled -= OnMonsterKilled;
    }

    private void OnMonsterKilled(string monsterId)
    {
        if (string.Equals(monsterId, MonsterId, StringComparison.Ordinal))
        {
            AddCount(1f);
        }
    }

}

/// <summary>
/// 收集条件
/// </summary>
[Serializable]
public sealed class CollectCondition : CountCondition
{
    /// <summary>
    /// 需要收集的道具ID
    /// </summary>
    [InspectorName("道具 ID")] public string ItemId;

    /// <summary>
    /// 需要收集的数量
    /// </summary>
    [InspectorName("目标数量")]
    [Min(1)] public new int TargetCount = 1;

    protected override int GetConfiguredTargetCount() => TargetCount;

    public override void Initialize()
    {
        QuestEventBus.OnItemAdded += OnItemAdded;
    }

    public override void Dispose()
    {
        QuestEventBus.OnItemAdded -= OnItemAdded;
    }

    private void OnItemAdded(string itemId, int count)
    {
        if (string.Equals(itemId, ItemId, StringComparison.Ordinal))
        {
            AddCount(count);
        }
    }
}

/// <summary>
/// 对话条件
/// </summary>
[Serializable]
public sealed class TalkCondition : Condition
{
    /// <summary>
    /// 需要对话的NPC ID
    /// </summary>
    [InspectorName("NPC ID")] public string NpcId;

    public override void Initialize()
    {
        QuestEventBus.OnNpcTalked += OnTalked;
    }

    public override void Dispose()
    {
        QuestEventBus.OnNpcTalked -= OnTalked;
    }

    private void OnTalked(string npcId)
    {
        if (string.Equals(npcId, NpcId, StringComparison.Ordinal))
        {
            Complete();
        }
    }
}

/// <summary>
/// 到达地点条件
/// </summary>
[Serializable]
public sealed class ReachLocationCondition : Condition
{
    /// <summary>
    /// 需要到达的地点ID
    /// </summary>
    [InspectorName("地点 ID")] public string LocationId;

    public override void Initialize()
    {
        QuestEventBus.OnLocationEntered += OnLocationEntered;
    }

    public override void Dispose()
    {
        QuestEventBus.OnLocationEntered -= OnLocationEntered;
    }

    private void OnLocationEntered(string locationId)
    {
        if (string.Equals(locationId, LocationId, StringComparison.Ordinal))
        {
            Complete();
        }
    }
}

/// <summary>
/// 通用进度事件条件
/// </summary>
[Serializable]
public class ProgressCondition : CountCondition
{
    /// <summary>
    /// 需要监听的进度事件类型
    /// </summary>
    [InspectorName("事件类型")] public QuestProgressEventType EventType = QuestProgressEventType.Custom;

    /// <summary>
    /// 进度事件对应的目标ID 为空时不限制目标
    /// </summary>
    [InspectorName("目标 ID")] public string TargetId;

    /// <summary>
    /// 需要累计的目标数量
    /// </summary>
    [InspectorName("目标数量")]
    [Min(1)] public new int TargetCount = 1;

    protected override int GetConfiguredTargetCount() => TargetCount;

    public override void Initialize()
    {
        QuestEventBus.OnProgressReported += OnProgressReported;
    }

    public override void Dispose()
    {
        QuestEventBus.OnProgressReported -= OnProgressReported;
    }

    protected virtual void OnProgressReported(QuestProgressEvent progressEvent)
    {
        if (progressEvent == null || progressEvent.EventType != EventType
            || !string.IsNullOrEmpty(TargetId)
            && !string.Equals(TargetId, progressEvent.TargetId, StringComparison.Ordinal))
        {
            return;
        }

        AddCount(progressEvent.Amount > 0f ? progressEvent.Amount : 1f);
    }
}

/// <summary>
/// 通用自定义事件条件
/// </summary>
[Serializable]
public sealed class CustomEventCondition : ProgressCondition
{
    public CustomEventCondition()
    {
        EventType = QuestProgressEventType.Custom;
    }
}

/// <summary>
/// 广告条件
/// </summary>
[Serializable]
public sealed class AdsCondition : ProgressCondition
{
    public AdsCondition()
    {
        EventType = QuestProgressEventType.Ads;
    }
}

/// <summary>
/// 通关数量条件
/// </summary>
[Serializable]
public sealed class PassNumCondition : ProgressCondition
{
    public PassNumCondition()
    {
        EventType = QuestProgressEventType.PassNum;
    }
}

/// <summary>
/// 真实时间进度条件
/// </summary>
[Serializable]
public sealed class TimeCondition : ProgressCondition
{
    public TimeCondition()
    {
        EventType = QuestProgressEventType.Time;
    }

    protected override void OnProgressReported(QuestProgressEvent progressEvent)
    {
        if (progressEvent == null || progressEvent.EventType != QuestProgressEventType.Time)
        {
            return;
        }

        AddCount(progressEvent.Value > 0f ? progressEvent.Value : progressEvent.Amount);
    }
}

/// <summary>
/// 在线时长条件
/// </summary>
[Serializable]
public sealed class OnlineTimeCondition : ProgressCondition
{
    /// <summary>
    /// 已累计的在线时长秒数
    /// </summary>
    [NonSerialized] private float onlineTimeSeconds;

    public OnlineTimeCondition()
    {
        EventType = QuestProgressEventType.OnLineTime;
    }

    public override float OnlineTimeSeconds => onlineTimeSeconds;

    protected override void OnProgressReported(QuestProgressEvent progressEvent)
    {
        if (progressEvent == null || progressEvent.EventType != QuestProgressEventType.OnLineTime)
        {
            return;
        }

        float seconds = progressEvent.Value > 0f ? progressEvent.Value : progressEvent.Amount;
        onlineTimeSeconds = Mathf.Min(TargetCount, onlineTimeSeconds + Mathf.Max(0f, seconds));
        AddCount(seconds);
    }

    public override void ResetRuntimeState()
    {
        base.ResetRuntimeState();
        onlineTimeSeconds = 0f;
    }

    internal override void RestoreState(QuestSaveConditionItem saveItem)
    {
        base.RestoreState(saveItem);
        onlineTimeSeconds = saveItem == null ? 0f : Mathf.Clamp(saveItem.onlineTimeSeconds, 0f, TargetCount);
    }

    internal override void WriteState(QuestSaveConditionItem saveItem)
    {
        base.WriteState(saveItem);
        if (saveItem != null)
        {
            saveItem.onlineTimeSeconds = onlineTimeSeconds;
        }
    }
}

/// <summary>
/// 背包已有数量条件
/// </summary>
[Serializable]
public sealed class ItemNumCondition : CountCondition
{
    /// <summary>
    /// 需要检查的道具ID
    /// </summary>
    [InspectorName("道具 ID")] public string ItemId;

    /// <summary>
    /// 背包中需要达到的道具数量
    /// </summary>
    [InspectorName("目标数量")]
    [Min(1)] public new int TargetCount = 1;

    protected override int GetConfiguredTargetCount() => TargetCount;

    public override void Initialize()
    {
        QuestEventBus.OnItemAdded += OnItemAdded;
        RefreshFromService();
    }

    public override void Dispose()
    {
        QuestEventBus.OnItemAdded -= OnItemAdded;
    }

    internal bool RefreshFromService()
    {
        if (ItemService == null || !int.TryParse(ItemId, out int itemId) || itemId <= 0)
        {
            return false;
        }

        float oldCount = currentCount;
        bool oldCompleted = IsCompleted;
        currentCount = Mathf.Clamp(ItemService.GetItemCount(itemId), 0f, TargetCount);
        IsCompleted = currentCount >= TargetCount;
        if (!Mathf.Approximately(oldCount, currentCount))
        {
            MarkChanged();
        }

        return !Mathf.Approximately(oldCount, currentCount) || oldCompleted != IsCompleted;
    }

    internal override bool RefreshItemNumProgress(IQuestItemService service)
    {
        SetServices(service);
        return RefreshFromService();
    }

    internal override void CollectItemNumConditions(List<ItemNumCondition> results)
    {
        results?.Add(this);
    }

    private void OnItemAdded(string itemId, int count)
    {
        if (string.Equals(itemId, ItemId, StringComparison.Ordinal))
        {
            RefreshFromService();
        }
    }
}

/// <summary>
/// 全部子条件满足的组合条件
/// </summary>
[Serializable]
public sealed class AndCondition : CompositeCondition
{
    protected override bool EvaluateCompletion()
    {
        if (Children == null || Children.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] == null || !Children[i].IsCompleted)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// 任一子条件满足的组合条件
/// </summary>
[Serializable]
public sealed class OrCondition : CompositeCondition
{
    protected override bool EvaluateCompletion()
    {
        if (Children == null)
        {
            return false;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] != null && Children[i].IsCompleted)
            {
                return true;
            }
        }

        return false;
    }
}

#endregion
