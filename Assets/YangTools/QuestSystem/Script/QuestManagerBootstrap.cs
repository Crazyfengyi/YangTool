using System;
using System.Collections.Generic;
using UnityEngine;

#if YANGTOOLS_QUEST_INTEGRATION
using System.Threading;
using Cysharp.Threading.Tasks;
using YooAsset;
#endif

/// <summary>
/// 任务管理器启动引导器
/// </summary>
/// <remarks>
/// 未启用项目集成宏时仅使用核心内存服务 可在本地列表中配置任务
/// </remarks>
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(QuestManager))]
[AddComponentMenu("YangTools/Quest/Quest Manager Bootstrap")]
public sealed class QuestManagerBootstrap : MonoBehaviour
{
    private const string DefaultPackageName = "DefaultPackage";
    private const string DefaultAssetTag = "SO";
    private const float DefaultWaitTimeoutSeconds = 30f;
    private const int PollIntervalMilliseconds = 100;

    [SerializeField]
    [InspectorName("自动加载任务配置")]
    [Tooltip("集成模式下从YooAsset加载  通用模式下从本地任务列表加载")]
    private bool autoLoadQuestData = true;
    [SerializeField]
    [InspectorName("资源包名称")]
    private string packageName = DefaultPackageName;
    [SerializeField]
    [InspectorName("任务资源标签")]
    private string assetTag = DefaultAssetTag;
    [SerializeField]
    [InspectorName("等待资源系统秒数")]
    [Min(0f)]
    private float waitTimeoutSeconds = DefaultWaitTimeoutSeconds;
    [SerializeField]
    [InspectorName("通用模式本地任务")]
    private List<QuestData> localQuestDatas = new List<QuestData>();

    public QuestManager manager;
    private bool started;

#if YANGTOOLS_QUEST_INTEGRATION
    private YangQuestEventBridge eventBridge;
    private YooAssetQuestDataLoader questLoader;
#endif

    /// <summary>
    /// 组件启动时执行默认初始化
    /// </summary>
    private void Start()
    {
        if (started) return;
        started = true;

        if (manager == null) manager = GetComponent<QuestManager>();
#if YANGTOOLS_QUEST_INTEGRATION
        RunIntegratedBootstrapAsync().Forget(HandleBootstrapException);
#else
        RunStandaloneBootstrap();
#endif
    }

    /// <summary>
    /// 组件销毁时释放可选适配器
    /// </summary>
    private void OnDestroy()
    {
#if YANGTOOLS_QUEST_INTEGRATION
        eventBridge?.Dispose();
        eventBridge = null;
        questLoader?.Dispose();
        questLoader = null;
#endif
    }

#if YANGTOOLS_QUEST_INTEGRATION
    /// <summary>
    /// 执行项目集成模式启动流程
    /// </summary>
    private async UniTask RunIntegratedBootstrapAsync()
    {
        if (manager == null)
        {
            Debug.LogError("任务启动引导器找不到QuestManager", this);
            return;
        }

        manager.Initialize();
        if (!manager.IsServicesConfigured)
        {
            try
            {
                manager.ConfigureServices(new YangQuestSaveStore(), new BagQuestItemService(),
                    questRewardService: new BagQuestRewardService());
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogWarning($"任务服务已被其他代码配置 保留现有配置 {exception.Message}", this);
            }
        }

        eventBridge = new YangQuestEventBridge(manager);
        if (!autoLoadQuestData)
        {
            manager.CompleteRegistration();
            return;
        }

        bool resourceReady = await WaitForResourcePackageAsync(this.GetCancellationTokenOnDestroy());
        if (!resourceReady)
        {
            CompleteRegistrationWithWarning("等待YooAsset资源包超时");
            return;
        }

        try
        {
            questLoader = new YooAssetQuestDataLoader();
            await questLoader.LoadAsync(manager, GetPackageName(), GetAssetTag());
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            CompleteRegistrationWithWarning($"加载任务配置失败 {exception.Message}");
        }
    }

    /// <summary>
    /// 等待YooAsset目标资源包就绪
    /// </summary>
    private async UniTask<bool> WaitForResourcePackageAsync(CancellationToken cancellationToken)
    {
        float elapsedSeconds = 0f;
        float timeoutSeconds = Mathf.Max(0f, waitTimeoutSeconds);
        while (elapsedSeconds < timeoutSeconds)
        {
            if (IsResourcePackageReady()) return true;
            await UniTask.Delay(PollIntervalMilliseconds, DelayType.Realtime, PlayerLoopTiming.Update, cancellationToken);
            elapsedSeconds += PollIntervalMilliseconds / 1000f;
        }
        return IsResourcePackageReady();
    }

    private bool IsResourcePackageReady()
    {
        if (!YooAssets.Initialized) return false;
        ResourcePackage package = YooAssets.TryGetPackage(GetPackageName());
        return package != null && package.InitializeStatus == EOperationStatus.Succeed && package.PackageValid;
    }

    private void CompleteRegistrationWithWarning(string reason)
    {
        Debug.LogError($"任务系统默认初始化降级 {reason}", this);
        if (manager != null && manager.IsInitialized && !manager.IsDataLoaded) manager.CompleteRegistration();
    }

    private void HandleBootstrapException(Exception exception)
    {
        if (exception is OperationCanceledException) return;
        CompleteRegistrationWithWarning(exception.Message);
    }

    private string GetPackageName() => string.IsNullOrWhiteSpace(packageName) ? DefaultPackageName : packageName.Trim();
    private string GetAssetTag() => string.IsNullOrWhiteSpace(assetTag) ? DefaultAssetTag : assetTag.Trim();
#else
    /// <summary>
    /// 执行无项目依赖的核心模式启动流程
    /// </summary>
    private void RunStandaloneBootstrap()
    {
        if (manager == null)
        {
            Debug.LogError("任务启动引导器找不到QuestManager", this);
            return;
        }

        manager.Initialize();
        if (!manager.IsServicesConfigured)
        {
            manager.ConfigureServices(new QuestMemorySaveStore(), NullQuestItemService.Instance,
                questRewardService: new QuestMemoryRewardService());
        }

        if (autoLoadQuestData && localQuestDatas != null)
        {
            manager.RegisterQuests(localQuestDatas);
        }
        else
        {
            manager.CompleteRegistration();
        }
    }
#endif
}
