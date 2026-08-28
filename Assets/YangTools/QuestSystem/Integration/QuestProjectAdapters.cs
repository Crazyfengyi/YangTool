#if YANGTOOLS_QUEST_INTEGRATION
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameMain;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core.YangSaveData;
using YooAsset;

/// <summary>
/// 项目存档与任务独立存档模型之间的映射适配器
/// </summary>
public sealed class YangQuestSaveStore : IQuestSaveStore
{
    private readonly QuestMemorySaveStore fallbackStore = new QuestMemorySaveStore();
    private readonly Dictionary<string, QuestSaveItem> cache = new Dictionary<string, QuestSaveItem>(StringComparer.Ordinal);

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

    public void Clear()
    {
        cache.Clear();
        fallbackStore.Clear();
        Save_QuestData saveData = GetSaveData(true);
        if (saveData != null) saveData.quests = new List<SaveQuestItem>();
    }

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

    private static Save_QuestData GetSaveData(bool isDirty = false)
    {
        YangSaveDataManager saveManager = YangSaveDataManager.Instance;
        return saveManager != null && saveManager.DataCenter != null
            ? saveManager.DataCenter.GetLocalSave<Save_QuestData>(isDirty)
            : null;
    }

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
                    SaveQuestConditionItem sourceCondition = sourceObjective.conditions[j];
                    objective.conditions.Add(sourceCondition == null ? null : new QuestSaveConditionItem
                    {
                        conditionId = sourceCondition.conditionId,
                        currentCount = sourceCondition.currentCount,
                        startUtcSeconds = sourceCondition.startUtcSeconds,
                        onlineTimeSeconds = sourceCondition.onlineTimeSeconds
                    });
                }
            }
            result.objectives.Add(objective);
        }
        return result;
    }

    private static SaveQuestItem ToProject(QuestSaveItem source)
    {
        SaveQuestItem result = new SaveQuestItem();
        CopyToProject(source, result);
        return result;
    }

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
                    QuestSaveConditionItem sourceCondition = sourceObjective.conditions[j];
                    objective.conditions.Add(sourceCondition == null ? null : new SaveQuestConditionItem
                    {
                        conditionId = sourceCondition.conditionId,
                        currentCount = sourceCondition.currentCount,
                        startUtcSeconds = sourceCondition.startUtcSeconds,
                        onlineTimeSeconds = sourceCondition.onlineTimeSeconds
                    });
                }
            }
            target.objectives.Add(objective);
        }
    }
}

/// <summary>
/// 项目背包任务适配器
/// </summary>
public sealed class BagQuestItemService : IQuestItemService
{
    public float GetItemCount(int itemId)
    {
        BagMgr bagManager = BagMgr.Instance;
        return bagManager != null ? Math.Max(0f, bagManager.GetBagPropCount(itemId)) : 0f;
    }

    public bool HasItem(int itemId, float count)
    {
        BagMgr bagManager = BagMgr.Instance;
        return bagManager != null && itemId > 0 && count > 0f && bagManager.BagPropEnough(itemId, count, false);
    }

    public bool TryConsume(int itemId, float count)
    {
        if (!HasItem(itemId, count)) return false;
        BagMgr.Instance.RemoveBagProp(itemId, count);
        return true;
    }
}

/// <summary>
/// 项目全局事件系统任务桥接器
/// </summary>
public sealed class YangQuestEventBridge : IDisposable
{
    private readonly QuestManager manager;
    private readonly YangEventGroup eventGroup = new YangEventGroup();
    private bool disposed;

    public YangQuestEventBridge(QuestManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
        QuestEventMessageBase.Sender = SendQuestEvent;
        eventGroup.AddListener<EventMessageBase>(OnProgressEvent);
        manager.QuestChanged += Publish;
        manager.ObjectiveChanged += Publish;
        manager.RewardIssued += Publish;
        manager.QuestReset += Publish;
        manager.DataLoaded += Publish;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        eventGroup.RemoveAllListener();
        manager.QuestChanged -= Publish;
        manager.ObjectiveChanged -= Publish;
        manager.RewardIssued -= Publish;
        manager.QuestReset -= Publish;
        manager.DataLoaded -= Publish;
        if (QuestEventMessageBase.Sender == SendQuestEvent)
        {
            QuestEventMessageBase.Sender = null;
        }
    }

    private void OnProgressEvent(EventData eventData)
    {
        manager.ReportProgress(eventData?.Args as QuestProgressEvent);
    }

    private static void SendQuestEvent(QuestEventMessageBase eventData)
    {
        if (eventData != null)
        {
            YangExtend.SendEvent(eventData.GetType(), eventData);
        }
    }

    private static void Publish(QuestChangedEvent eventData) => YangExtend.SendEvent(typeof(QuestChangedEvent), eventData);
    private static void Publish(QuestObjectiveChangedEvent eventData) => YangExtend.SendEvent(typeof(QuestObjectiveChangedEvent), eventData);
    private static void Publish(QuestRewardEvent eventData) => YangExtend.SendEvent(typeof(QuestRewardEvent), eventData);
    private static void Publish(QuestResetEvent eventData) => YangExtend.SendEvent(typeof(QuestResetEvent), eventData);
    private static void Publish(QuestDataLoadedEvent eventData) => YangExtend.SendEvent(typeof(QuestDataLoadedEvent), eventData);
}

/// <summary>
/// YooAsset任务配置加载适配器
/// </summary>
public sealed class YooAssetQuestDataLoader : IDisposable
{
    private readonly List<AssetHandle> assetHandles = new List<AssetHandle>();

    public async UniTask<int> LoadAsync(QuestManager manager, string packageName = "DefaultPackage", string assetTag = "SO")
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (!YooAssets.Initialized) throw new InvalidOperationException("YooAsset尚未初始化");
        ResourcePackage package = YooAssets.TryGetPackage(packageName);
        if (package == null) throw new InvalidOperationException($"未找到资源包 {packageName}");

        AssetInfo[] assetInfos = package.GetAssetInfos(assetTag);
        if (assetInfos == null || assetInfos.Length == 0)
        {
            manager.CompleteRegistration();
            return 0;
        }

        List<AssetInfo> sortedAssetInfos = new List<AssetInfo>(assetInfos);
        sortedAssetInfos.Sort((left, right) => string.CompareOrdinal(left.Address, right.Address));
        int registeredCount = 0;
        for (int i = 0; i < sortedAssetInfos.Count; i++)
        {
            registeredCount += await LoadQuestAsync(manager, package, sortedAssetInfos[i]);
        }
        manager.CompleteRegistration();
        return registeredCount;
    }

    public void Dispose()
    {
        for (int i = 0; i < assetHandles.Count; i++) assetHandles[i]?.Release();
        assetHandles.Clear();
    }

    private async UniTask<int> LoadQuestAsync(QuestManager manager, ResourcePackage package, AssetInfo assetInfo)
    {
        AssetHandle handle = null;
        try
        {
            handle = package.LoadAssetAsync<ScriptableObject>(assetInfo.Address);
            await handle.ToUniTask();
            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"加载任务配置失败:{assetInfo.Address} {handle.LastError}");
                return 0;
            }

            QuestData questData = handle.AssetObject as QuestData;
            if (questData == null) return 0;
            manager.RegisterQuest(questData);
            assetHandles.Add(handle);
            handle = null;
            return 1;
        }
        finally
        {
            handle?.Release();
        }
    }
}
#endif
