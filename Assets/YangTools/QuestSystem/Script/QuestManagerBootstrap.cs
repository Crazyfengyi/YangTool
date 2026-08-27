using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

/// <summary>
/// 任务管理器项目启动引导器
/// </summary>
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(QuestManager))]
[AddComponentMenu("YangTools/Quest/Quest Manager Bootstrap")]
public sealed class QuestManagerBootstrap : MonoBehaviour
{
    private const string DefaultPackageName = "DefaultPackage"; //默认资源包名称
    private const string DefaultAssetTag = "SO"; //默认任务资源标签
    private const float DefaultWaitTimeoutSeconds = 30f; //默认资源系统等待时长
    private const int PollIntervalMilliseconds = 100; //资源系统轮询间隔

    [SerializeField]
    [InspectorName("自动加载任务配置")]
    [Tooltip("启用后会在YooAsset就绪后自动加载SO标签下的QuestData")]
    private bool autoLoadQuestData = true; //是否自动加载任务配置
    [SerializeField]
    [InspectorName("资源包名称")]
    private string packageName = DefaultPackageName; //任务资源包名称
    [SerializeField]
    [InspectorName("任务资源标签")]
    private string assetTag = DefaultAssetTag; //任务资源标签
    [SerializeField]
    [InspectorName("等待资源系统秒数")]
    [Min(0f)]
    private float waitTimeoutSeconds = DefaultWaitTimeoutSeconds; //等待资源系统的最大时长

    public QuestManager manager; //任务管理器
    private YangQuestEventBridge eventBridge; //项目事件桥接器
    private YooAssetQuestDataLoader questLoader; //任务配置加载器
    private bool started; //是否已启动引导流程

    /// <summary>
    /// 组件启动时开始默认初始化流程
    /// </summary>
    private void Start()
    {
        if (started)
        {
            return;
        }

        started = true;
        RunBootstrapAsync().Forget(HandleBootstrapException);
    }

    /// <summary>
    /// 组件销毁时释放事件和资源句柄
    /// </summary>
    private void OnDestroy()
    {
        eventBridge?.Dispose();
        eventBridge = null;
        questLoader?.Dispose();
        questLoader = null;
    }

    /// <summary>
    /// 执行项目默认任务初始化
    /// </summary>
    private async UniTask RunBootstrapAsync()
    {
        if (manager == null)
        {
            Debug.LogError("任务启动引导器找不到QuestManager", this);
            return;
        }

        manager.Initialize();
        ConfigureProjectServices();
        eventBridge = new YangQuestEventBridge(manager);

        if (!autoLoadQuestData)
        {
            return;
        }

        //加载任务数据
        CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
        bool resourceReady = await WaitForResourcePackageAsync(cancellationToken);
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
            throw;
        }
        catch (Exception exception)
        {
            CompleteRegistrationWithWarning($"加载任务配置失败 {exception.Message}");
        }
    }

    /// <summary>
    /// 注入项目默认服务
    /// </summary>
    private void ConfigureProjectServices()
    {
        if (manager.IsServicesConfigured)
        {
            return;
        }

        try
        {
            manager.ConfigureServices(new YangQuestSaveStore(), new BagQuestItemService());
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogWarning($"任务服务已被其他代码占用 保留现有配置 {exception.Message}", this);
        }
    }

    /// <summary>
    /// 等待YooAsset和目标资源包就绪
    /// </summary>
    /// <param name="cancellationToken">对象销毁取消令牌</param>
    /// <returns>资源系统就绪返回true</returns>
    private async UniTask<bool> WaitForResourcePackageAsync(CancellationToken cancellationToken)
    {
        float elapsedSeconds = 0f;
        float timeoutSeconds = Mathf.Max(0f, waitTimeoutSeconds);
        while (elapsedSeconds < timeoutSeconds)
        {
            if (IsResourcePackageReady())
            {
                return true;
            }

            await UniTask.Delay(PollIntervalMilliseconds, DelayType.Realtime, PlayerLoopTiming.Update,
                cancellationToken);
            elapsedSeconds += PollIntervalMilliseconds / 1000f;
        }

        return IsResourcePackageReady();
    }

    /// <summary>
    /// 检查目标资源包是否已经完成初始化
    /// </summary>
    /// <returns>资源包可用返回true</returns>
    private bool IsResourcePackageReady()
    {
        if (!YooAssets.Initialized)
        {
            return false;
        }

        ResourcePackage package = YooAssets.TryGetPackage(GetPackageName());
        return package != null
               && package.InitializeStatus == EOperationStatus.Succeed
               && package.PackageValid;
    }

    /// <summary>
    /// 资源加载失败时完成当前注册批次
    /// </summary>
    /// <param name="reason">失败原因</param>
    private void CompleteRegistrationWithWarning(string reason)
    {
        Debug.LogError($"任务系统默认初始化降级 {reason}", this);
        if (manager != null && manager.IsInitialized && !manager.IsDataLoaded)
        {
            manager.CompleteRegistration();
        }
    }
    
    #region 辅助方法

    /// <summary>
    /// 处理未捕获的启动异常
    /// </summary>
    /// <param name="exception">启动异常</param>
    private void HandleBootstrapException(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            return;
        }

        CompleteRegistrationWithWarning(exception.Message);
    }

    /// <summary>
    /// 获取有效资源包名称
    /// </summary>
    /// <returns>资源包名称</returns>
    private string GetPackageName()
    {
        return string.IsNullOrWhiteSpace(packageName) ? DefaultPackageName : packageName.Trim();
    }

    /// <summary>
    /// 获取有效任务资源标签
    /// </summary>
    /// <returns>资源标签</returns>
    private string GetAssetTag()
    {
        return string.IsNullOrWhiteSpace(assetTag) ? DefaultAssetTag : assetTag.Trim();
    }
    
    #endregion
}
