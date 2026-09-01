#if YANGTOOLS_QUEST_INTEGRATION
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

/// <summary>
/// YooAsset任务配置加载适配器
/// </summary>
public sealed class YooAssetQuestDataLoader : IDisposable
{
    private readonly List<AssetHandle> assetHandles = new List<AssetHandle>();

    /// <summary>
    /// 从YooAsset加载任务配置
    /// </summary>
    /// <param name="manager">任务管理器</param>
    /// <param name="packageName">资源包名称</param>
    /// <param name="assetTag">任务资源标签</param>
    /// <returns>成功注册的任务数量</returns>
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

    /// <summary>
    /// 释放已加载的任务资源句柄
    /// </summary>
    public void Dispose()
    {
        for (int i = 0; i < assetHandles.Count; i++) assetHandles[i]?.Release();
        assetHandles.Clear();
    }

    /// <summary>
    /// 加载并注册单个任务配置
    /// </summary>
    /// <param name="manager">任务管理器</param>
    /// <param name="package">YooAsset资源包</param>
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
