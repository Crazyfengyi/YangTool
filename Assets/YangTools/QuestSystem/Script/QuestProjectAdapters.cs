using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameMain;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core.YangSaveData;
using YooAsset;

/// <summary>
/// 项目本地存档任务适配器
/// </summary>
public sealed class YangQuestSaveStore : IQuestSaveStore
{
    /// <inheritdoc />
    public SaveQuestItem GetQuest(string questId)
    {
        return GetSaveData().GetQuest(questId);
    }

    /// <inheritdoc />
    public SaveQuestItem GetOrCreateQuest(string questId)
    {
        return GetSaveData().GetOrCreateQuest(questId);
    }

    /// <inheritdoc />
    public void Clear()
    {
        GetSaveData(true).quests = new List<SaveQuestItem>();
    }

    /// <inheritdoc />
    public void MarkDirty()
    {
        GetSaveData(true);
    }

    /// <summary>
    /// 获取项目任务存档数据
    /// </summary>
    /// <param name="isDirty">是否标记为已修改</param>
    /// <returns>任务存档数据</returns>
    private static Save_QuestData GetSaveData(bool isDirty = false)
    {
        return YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_QuestData>(isDirty);
    }
}

/// <summary>
/// 项目背包任务适配器
/// </summary>
public sealed class BagQuestItemService : IQuestItemService
{
    /// <inheritdoc />
    public float GetItemCount(int itemId)
    {
        BagMgr bagManager = BagMgr.Instance;
        return bagManager != null ? Math.Max(0f, bagManager.GetBagPropCount(itemId)) : 0f;
    }

    /// <inheritdoc />
    public bool HasItem(int itemId, float count)
    {
        BagMgr bagManager = BagMgr.Instance;
        return bagManager != null && itemId > 0 && count > 0f
               && bagManager.BagPropEnough(itemId, count, false);
    }

    /// <inheritdoc />
    public bool TryConsume(int itemId, float count)
    {
        BagMgr bagManager = BagMgr.Instance;
        if (bagManager == null || itemId <= 0 || count <= 0f
                               || !bagManager.BagPropEnough(itemId, count, false))
        {
            return false;
        }

        bagManager.RemoveBagProp(itemId, count);
        return true;
    }
}

/// <summary>
/// 项目全局事件系统任务桥接器
/// </summary>
public sealed class YangQuestEventBridge : IDisposable
{
    private readonly QuestManager manager; //任务管理器
    private readonly YangEventGroup eventGroup = new YangEventGroup(); //项目事件监听组
    private bool disposed; //是否已释放

    /// <summary>
    /// 创建任务事件桥接器
    /// </summary>
    /// <param name="manager">任务管理器</param>
    public YangQuestEventBridge(QuestManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
        eventGroup.AddListener<QuestProgressEvent>(OnProgressEvent);
        manager.QuestChanged += Publish;
        manager.ObjectiveChanged += Publish;
        manager.RewardIssued += Publish;
        manager.QuestReset += Publish;
        manager.DataLoaded += Publish;
    }

    /// <summary>
    /// 解除任务事件桥接
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        eventGroup.RemoveAllListener();
        manager.QuestChanged -= Publish;
        manager.ObjectiveChanged -= Publish;
        manager.RewardIssued -= Publish;
        manager.QuestReset -= Publish;
        manager.DataLoaded -= Publish;
    }

    /// <summary>
    /// 将项目任务进度事件转发给独立任务模块
    /// </summary>
    /// <param name="eventData">项目事件数据</param>
    private void OnProgressEvent(EventData eventData)
    {
        manager.ReportProgress(eventData?.Args as QuestProgressEvent);
    }

    /// <summary>
    /// 发布任务状态变化事件
    /// </summary>
    /// <param name="eventData">任务状态变化事件</param>
    private static void Publish(QuestChangedEvent eventData)
    {
        eventData?.SendEvent();
    }

    /// <summary>
    /// 发布任务目标变化事件
    /// </summary>
    /// <param name="eventData">任务目标变化事件</param>
    private static void Publish(QuestObjectiveChangedEvent eventData)
    {
        eventData?.SendEvent();
    }

    /// <summary>
    /// 发布任务奖励事件
    /// </summary>
    /// <param name="eventData">任务奖励事件</param>
    private static void Publish(QuestRewardEvent eventData)
    {
        eventData?.SendEvent();
    }

    /// <summary>
    /// 发布任务重置事件
    /// </summary>
    /// <param name="eventData">任务重置事件</param>
    private static void Publish(QuestResetEvent eventData)
    {
        eventData?.SendEvent();
    }

    /// <summary>
    /// 发布任务数据加载完成事件
    /// </summary>
    /// <param name="eventData">任务数据加载完成事件</param>
    private static void Publish(QuestDataLoadedEvent eventData)
    {
        eventData?.SendEvent();
    }
}

/// <summary>
/// YooAsset任务配置加载适配器
/// </summary>
public sealed class YooAssetQuestDataLoader : IDisposable
{
    private readonly List<AssetHandle> assetHandles = new List<AssetHandle>(); //任务资源句柄

    /// <summary>
    /// 加载并注册任务配置
    /// </summary>
    /// <param name="manager">任务管理器</param>
    /// <param name="packageName">资源包名称</param>
    /// <param name="assetTag">任务资源标签</param>
    /// <returns>成功注册的任务数量</returns>
    public async UniTask<int> LoadAsync(QuestManager manager, string packageName = "DefaultPackage",
        string assetTag = "SO")
    {
        if (manager == null)
        {
            throw new ArgumentNullException(nameof(manager));
        }

        if (!YooAssets.Initialized)
        {
            throw new InvalidOperationException("YooAsset尚未初始化");
        }

        ResourcePackage package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            throw new InvalidOperationException($"未找到资源包 {packageName}");
        }

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

    /// <summary>
    /// 释放任务资源句柄
    /// </summary>
    public void Dispose()
    {
        for (int i = 0; i < assetHandles.Count; i++)
        {
            assetHandles[i]?.Release();
        }

        assetHandles.Clear();
    }

    /// <summary>
    /// 加载单个任务配置
    /// </summary>
    /// <param name="manager">任务管理器</param>
    /// <param name="package">资源包</param>
    /// <param name="assetInfo">资源信息</param>
    /// <returns>成功注册返回1</returns>
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
            if (questData == null)
            {
                return 0;
            }

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
