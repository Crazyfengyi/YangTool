using System;
using System.Collections.Generic;
using YangTools.Scripts.Core.YangSaveData;

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
    SaveQuestItem GetQuest(string questId);

    /// <summary>
    /// 获取或创建指定任务存档
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务存档</returns>
    SaveQuestItem GetOrCreateQuest(string questId);

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
    private readonly Dictionary<string, SaveQuestItem> saveItems =
        new Dictionary<string, SaveQuestItem>(StringComparer.Ordinal); //内存任务存档

    /// <summary>
    /// 创建内存任务存档服务
    /// </summary>
    public QuestMemorySaveStore()
    {
    }

    /// <inheritdoc />
    public SaveQuestItem GetQuest(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            return null;
        }

        saveItems.TryGetValue(questId, out SaveQuestItem saveItem);
        return saveItem;
    }

    /// <inheritdoc />
    public SaveQuestItem GetOrCreateQuest(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            throw new ArgumentException("任务ID不能为空", nameof(questId));
        }

        if (saveItems.TryGetValue(questId, out SaveQuestItem saveItem))
        {
            return saveItem;
        }

        saveItem = new SaveQuestItem
        {
            questId = questId,
            state = QuestState.Locked,
            objectives = new List<SaveQuestObjectiveItem>()
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
