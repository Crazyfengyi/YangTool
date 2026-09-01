using System;
using System.Collections.Generic;

#region 接口

/// <summary>
/// 任务存档服务
/// </summary>
public interface IQuestSaveStore
{
    /// <summary>
    /// 获取指定任务存档
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务存档，不存在时返回null</returns>
    QuestSaveItem GetQuest(string questId);

    /// <summary>
    /// 获取或创建指定任务存档
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务存档</returns>
    QuestSaveItem GetOrCreateQuest(string questId);

    /// <summary>
    /// 清空全部任务存档
    /// </summary>
    void Clear();

    /// <summary>
    /// 标记任务存档已修改
    /// </summary>
    void MarkDirty();
}

/// <summary>
/// 任务道具服务
/// </summary>
public interface IQuestItemService
{
    /// <summary>
    /// 获取道具数量
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <returns>当前数量</returns>
    float GetItemCount(int itemId);

    /// <summary>
    /// 判断道具数量是否充足
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="count">需要数量</param>
    /// <returns>数量充足返回true</returns>
    bool HasItem(int itemId, float count);

    /// <summary>
    /// 尝试消耗道具
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="count">消耗数量</param>
    /// <returns>消耗成功返回true</returns>
    bool TryConsume(int itemId, float count);
}

/// <summary>
/// 任务奖励服务
/// </summary>
public interface IQuestRewardService
{
    /// <summary>
    /// 发放现金
    /// </summary>
    bool TryGrantMoney(int amount);

    /// <summary>
    /// 发放金币
    /// </summary>
    bool TryGrantGold(int amount);

    /// <summary>
    /// 发放经验
    /// </summary>
    bool TryGrantExp(int amount);

    /// <summary>
    /// 发放道具
    /// </summary>
    bool TryGrantItem(string itemId, int count);

    /// <summary>
    /// 发放自定义奖励
    /// </summary>
    bool TryGrantCustom(string rewardKey, int count);
}

/// <summary>
/// 任务时间服务
/// </summary>
public interface IQuestTimeProvider
{
    long UtcNowSeconds { get; }
    string LocalDateKey { get; }
}

#endregion

#region 默认保底实现

/// <summary>
/// 内存任务存档服务
/// </summary>
public sealed class QuestMemorySaveStore : IQuestSaveStore
{
    private readonly Dictionary<string, QuestSaveItem> saveItems =
        new Dictionary<string, QuestSaveItem>(StringComparer.Ordinal); //内存任务存档

    /// <summary>
    /// 创建内存任务存档服务
    /// </summary>
    public QuestMemorySaveStore()
    {
    }

    /// <inheritdoc />
    public QuestSaveItem GetQuest(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            return null;
        }

        saveItems.TryGetValue(questId, out QuestSaveItem saveItem);
        return saveItem;
    }

    /// <inheritdoc />
    public QuestSaveItem GetOrCreateQuest(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            throw new ArgumentException("任务ID不能为空", nameof(questId));
        }

        if (saveItems.TryGetValue(questId, out QuestSaveItem saveItem))
        {
            return saveItem;
        }

        saveItem = new QuestSaveItem
        {
            questId = questId,
            state = QuestState.Locked,
            objectives = new List<QuestSaveObjectiveItem>()
        };
        saveItems.Add(questId, saveItem);
        return saveItem;
    }

    /// <inheritdoc />
    public void Clear()
    {
        saveItems.Clear();
    }

    /// <inheritdoc />
    public void MarkDirty()
    {
    }
}

/// <summary>
/// 空任务道具服务
/// </summary>
public sealed class NullQuestItemService : IQuestItemService
{
    public static NullQuestItemService Instance { get; } = new NullQuestItemService();

    private NullQuestItemService()
    {
    }

    /// <inheritdoc />
    public float GetItemCount(int itemId)
    {
        return 0f;
    }

    /// <inheritdoc />
    public bool HasItem(int itemId, float count)
    {
        return false;
    }

    /// <inheritdoc />
    public bool TryConsume(int itemId, float count)
    {
        return false;
    }
}

/// <summary>
/// 空任务奖励服务
/// </summary>
public sealed class NullQuestRewardService : IQuestRewardService
{
    public static NullQuestRewardService Instance { get; } = new NullQuestRewardService();

    private NullQuestRewardService()
    {
    }

    public bool TryGrantMoney(int amount) => false;
    public bool TryGrantGold(int amount) => false;
    public bool TryGrantExp(int amount) => false;
    public bool TryGrantItem(string itemId, int count) => false;
    public bool TryGrantCustom(string rewardKey, int count) => false;
}

/// <summary>
/// 内存任务奖励服务
/// </summary>
public sealed class QuestMemoryRewardService : IQuestRewardService
{
    private readonly Dictionary<string, int> amounts = new Dictionary<string, int>(StringComparer.Ordinal);

    public bool TryGrantMoney(int amount) => Add("Money", amount);
    public bool TryGrantGold(int amount) => Add("Gold", amount);
    public bool TryGrantExp(int amount) => Add("Exp", amount);
    public bool TryGrantItem(string itemId, int count) => Add("Item:" + itemId, count);
    public bool TryGrantCustom(string rewardKey, int count) => Add("Custom:" + rewardKey, count);

    /// <summary>
    /// 获取内存服务记录的奖励数量
    /// </summary>
    /// <param name="key">奖励键</param>
    /// <returns>奖励数量</returns>
    public int GetAmount(string key)
    {
        return !string.IsNullOrEmpty(key) && amounts.TryGetValue(key, out int amount) ? amount : 0;
    }

    private bool Add(string key, int amount)
    {
        if (string.IsNullOrEmpty(key) || amount <= 0)
        {
            return false;
        }

        amounts[key] = GetAmount(key) + amount;
        return true;
    }
}

/// <summary>
/// 系统任务时间服务
/// </summary>
public sealed class SystemQuestTimeProvider : IQuestTimeProvider
{
    public static SystemQuestTimeProvider Instance { get; } = new SystemQuestTimeProvider();

    public long UtcNowSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public string LocalDateKey => DateTime.Now.ToString("yyyyMMdd");

    private SystemQuestTimeProvider()
    {
    }
}

#endregion
