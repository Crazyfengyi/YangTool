using System;
using System.Collections.Generic;
using UnityEngine;
using YangTools.Scripts.Core.YangSaveData;

/// <summary>
/// 通用任务管理器。
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("YangTools/Quest/Quest Manager")]
public sealed class QuestManager : MonoBehaviour
{
    private const float TimeQuestRefreshInterval = 1f; //时间条件刷新间隔
    private const float OnlineTimeRefreshIntervalSeconds = 60f; //在线时长提交间隔

    private static QuestManager instance; //任务管理器单例

    [SerializeField]
    [InspectorName("跨场景保留")]
    [Tooltip("启用后任务管理器会在场景切换时保留")]
    private bool persistAcrossScenes = true; //是否跨场景保留

    private readonly Dictionary<string, QuestData> questDatas = //已注册的任务配置
        new Dictionary<string, QuestData>(StringComparer.Ordinal);

    private readonly Dictionary<string, QuestRuntime> questRuntimes = //任务运行时实例
        new Dictionary<string, QuestRuntime>(StringComparer.Ordinal);

    private readonly List<ConditionRuntime> itemNumConditionCache = new List<ConditionRuntime>(); //道具条件缓存
    private readonly List<ConditionRuntime> itemNumConsumeCache = new List<ConditionRuntime>(); //待扣除道具条件
    private IQuestSaveStore saveStore = new QuestMemorySaveStore(); //任务存档服务
    private IQuestItemService itemService = NullQuestItemService.Instance; //任务道具服务
    private IQuestTimeProvider timeProvider = SystemQuestTimeProvider.Instance; //任务时间服务
    private bool servicesConfigured; //是否已注入自定义服务
    private bool initialized; //是否已初始化
    private bool dataLoaded; //是否已完成任务配置加载
    private bool isApplicationForeground = true; //应用是否处于前台
    private float timeQuestRefreshTimer; //时间条件刷新计时器
    private float onlineTimeRefreshTimer; //在线时长累计计时器
    private string dailyRefreshDate; //最近一次每日任务刷新日期

    /// <summary>
    /// 是否已初始化任务系统
    /// </summary>
    public bool IsInitialized => initialized;

    /// <summary>
    /// 是否已完成任务配置加载
    /// </summary>
    public bool IsDataLoaded => dataLoaded;

    /// <summary>
    /// 是否已配置任务模块依赖服务
    /// </summary>
    public bool IsServicesConfigured => servicesConfigured;

    /// <summary>
    /// 任务状态变化事件
    /// </summary>
    public event Action<QuestChangedEvent> QuestChanged;

    /// <summary>
    /// 任务目标进度变化事件
    /// </summary>
    public event Action<QuestObjectiveChangedEvent> ObjectiveChanged;

    /// <summary>
    /// 任务奖励发放事件
    /// </summary>
    public event Action<QuestRewardEvent> RewardIssued;

    /// <summary>
    /// 任务进度重置事件
    /// </summary>
    public event Action<QuestResetEvent> QuestReset;

    /// <summary>
    /// 任务配置加载完成事件
    /// </summary>
    public event Action<QuestDataLoadedEvent> DataLoaded;

    /// <summary>
    /// 获取任务管理器单例
    /// </summary>
    public static QuestManager Instance => instance;

    /// <summary>
    /// 组件唤醒时注册场景单例
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 组件启动时初始化任务系统
    /// </summary>
    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 每帧驱动任务时间和在线时长进度
    /// </summary>
    private void Update()
    {
        if (instance == this)
        {
            Tick(Time.unscaledDeltaTime);
        }
    }

    /// <summary>
    /// 响应应用前后台切换
    /// </summary>
    /// <param name="hasFocus">应用是否获得焦点</param>
    private void OnApplicationFocus(bool hasFocus)
    {
        SetApplicationForeground(hasFocus);
    }

    /// <summary>
    /// 响应应用暂停和恢复
    /// </summary>
    /// <param name="pauseStatus">是否暂停</param>
    private void OnApplicationPause(bool pauseStatus)
    {
        SetApplicationForeground(!pauseStatus);
    }

    /// <summary>
    /// 组件销毁时清理单例和任务运行时
    /// </summary>
    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        Shutdown();
        instance = null;
    }

    #region 生命周期

    /// <summary>
    /// 配置任务模块依赖服务--任务存档服务和任务道具服务必须实现
    /// </summary>
    /// <param name="questSaveStore">任务存档服务</param>
    /// <param name="questItemService">任务道具服务</param>
    /// <param name="questTimeProvider">任务时间服务</param>
    public void ConfigureServices(IQuestSaveStore questSaveStore, IQuestItemService questItemService,
        IQuestTimeProvider questTimeProvider = null)
    {
        if (dataLoaded || questDatas.Count > 0 || questRuntimes.Count > 0)
        {
            throw new InvalidOperationException("任务数据加载后不能替换依赖服务");
        }

        saveStore = questSaveStore ?? throw new ArgumentNullException(nameof(questSaveStore));
        itemService = questItemService ?? NullQuestItemService.Instance;
        timeProvider = questTimeProvider ?? SystemQuestTimeProvider.Instance;
        servicesConfigured = true;
    }

    /// <summary>
    /// 初始化任务系统。
    /// </summary>
    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        dataLoaded = false;
        dailyRefreshDate = timeProvider.LocalDateKey;
    }

    /// <summary>
    /// 使用指定配置初始化任务系统
    /// </summary>
    /// <param name="questDataList">任务配置列表</param>
    public void Initialize(IEnumerable<QuestData> questDataList)
    {
        Initialize();
        RegisterQuests(questDataList);
    }

    /// <summary>
    /// 重置当前所有任务到初始状态。
    /// </summary>
    [Obsolete("请使用 ResetAll")]
    public void RestInit()
    {
        ResetAll();
    }

    /// <summary>
    /// 重置当前所有任务到初始状态
    /// </summary>
    public void ResetAll()
    {
        itemNumConditionCache.Clear();
        itemNumConsumeCache.Clear();
        timeQuestRefreshTimer = 0f;
        onlineTimeRefreshTimer = 0f;

        saveStore.Clear();

        List<QuestData> registeredQuestDatas = new List<QuestData>(questDatas.Values);
        questRuntimes.Clear();
        for (int i = 0; i < registeredQuestDatas.Count; i++)
        {
            QuestData questData = registeredQuestDatas[i];
            if (questData == null || string.IsNullOrEmpty(questData.Id))
            {
                continue;
            }

            QuestRuntime runtime = CreateRuntime(questData);
            questRuntimes[questData.Id] = runtime;
            RefreshQuestAvailability(runtime);
            EnsureAndRefreshTimeProgress(runtime);
            EnsureAndRefreshItemNumProgress(runtime);
            RefreshRuntimeStateAfterProgress(runtime);
            SaveRuntime(runtime);
            SendQuestResetEvent(runtime, false);
        }
    }

    /// <summary>
    /// 关闭任务系统并移除事件监听。
    /// </summary>
    public void Shutdown()
    {
        questDatas.Clear();
        questRuntimes.Clear();
        itemNumConditionCache.Clear();
        itemNumConsumeCache.Clear();
        initialized = false;
        dataLoaded = false;
        servicesConfigured = false;
        timeQuestRefreshTimer = 0f;
        onlineTimeRefreshTimer = 0f;
        dailyRefreshDate = string.Empty;
        isApplicationForeground = true;
        QuestChanged = null;
        ObjectiveChanged = null;
        RewardIssued = null;
        QuestReset = null;
        DataLoaded = null;
    }

    /// <summary>
    /// 设置应用是否处于前台，在线时长只在前台累计。
    /// </summary>
    /// <param name="isForeground">应用是否处于前台</param>
    public void SetApplicationForeground(bool isForeground)
    {
        isApplicationForeground = isForeground;
        if (!isForeground)
        {
            onlineTimeRefreshTimer = 0f;
        }
    }

    /// <summary>
    /// 更新任务系统。
    /// </summary>
    /// <param name="unscaledDeltaTime">真实流逝时间</param>
    public void Tick(float unscaledDeltaTime)
    {
        if (!initialized || float.IsNaN(unscaledDeltaTime) || float.IsInfinity(unscaledDeltaTime))
        {
            return;
        }

        unscaledDeltaTime = Mathf.Max(0f, unscaledDeltaTime);
        RefreshDailyQuestsIfNeeded();
        timeQuestRefreshTimer += unscaledDeltaTime;
        if (timeQuestRefreshTimer >= TimeQuestRefreshInterval)
        {
            timeQuestRefreshTimer %= TimeQuestRefreshInterval;
            RefreshAllTimeProgress();
        }

        if (!isApplicationForeground)
        {
            return;
        }

        // 在线时长计时器按秒累计，每分钟提交一次在线时长进度。
        onlineTimeRefreshTimer += unscaledDeltaTime;
        if (onlineTimeRefreshTimer < OnlineTimeRefreshIntervalSeconds)
        {
            return;
        }

        float onlineMinutes = onlineTimeRefreshTimer / OnlineTimeRefreshIntervalSeconds;
        onlineTimeRefreshTimer %= OnlineTimeRefreshIntervalSeconds;
        ReportProgress(new QuestProgressEvent(QuestProgressEventType.OnLineTime, string.Empty,
            value: onlineMinutes));
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 注册任务配置。
    /// </summary>
    /// <param name="questData">任务配置</param>
    public void RegisterQuest(QuestData questData)
    {
        if (questData == null || string.IsNullOrWhiteSpace(questData.Id))
        {
            Debug.LogWarning("任务配置为空或任务ID为空，注册失败");
            return;
        }

        dataLoaded = false;
        ValidateQuestConfiguration(questData);
        if (questDatas.TryGetValue(questData.Id, out QuestData registeredData) && registeredData != questData)
        {
            Debug.LogWarning($"任务ID重复 {questData.Id} 后注册的配置将覆盖先前配置");
        }

        questDatas[questData.Id] = questData;
        QuestRuntime runtime = CreateRuntime(questData);
        questRuntimes[questData.Id] = runtime;
        RefreshQuestAvailability(runtime);
        EnsureAndRefreshTimeProgress(runtime);
        EnsureAndRefreshItemNumProgress(runtime);
        RefreshRuntimeStateAfterProgress(runtime);
        SaveRuntime(runtime);
    }

    /// <summary>
    /// 通过代码注册任务配置列表。
    /// </summary>
    /// <param name="questDataList">任务配置列表</param>
    public void RegisterQuests(IEnumerable<QuestData> questDataList)
    {
        if (questDataList == null)
        {
            CompleteRegistration();
            return;
        }

        foreach (QuestData questData in questDataList)
        {
            RegisterQuest(questData);
        }

        CompleteRegistration();
    }

    /// <summary>
    /// 完成当前批次任务配置注册
    /// </summary>
    public void CompleteRegistration()
    {
        if (!initialized)
        {
            Initialize();
        }

        RefreshAllQuestAvailability();
        RefreshAllTimeProgress();
        RefreshAllItemNumProgress();
        dataLoaded = true;
        QuestDataLoadedEvent loadedEvent = new QuestDataLoadedEvent
        {
            QuestCount = questRuntimes.Count
        };
        DataLoaded?.Invoke(loadedEvent);
    }

    /// <summary>
    /// 接取任务。
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>是否接取成功</returns>
    public bool AcceptQuest(string questId)
    {
        QuestRuntime runtime = GetQuest(questId);
        if (runtime == null || runtime.State != QuestState.Available)
        {
            return false;
        }

        if (!CanAcceptQuest(runtime))
        {
            return false;
        }

        ChangeState(runtime, QuestState.Active);
        EnsureAndRefreshTimeProgress(runtime);
        EnsureAndRefreshItemNumProgress(runtime);
        RefreshRuntimeStateAfterProgress(runtime);
        return true;
    }

    /// <summary>
    /// 手动确认完成当前任务目标。
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <param name="consumeItem">是否扣除当前目标中的道具数量条件道具</param>
    /// <returns>是否完成成功</returns>
    public bool CompleteQuest(string questId, bool consumeItem = true)
    {
        QuestRuntime runtime = GetQuest(questId);
        if (runtime == null || runtime.State != QuestState.Active)
        {
            return false;
        }

        ObjectiveRuntime activeObjective = runtime.GetActiveObjective();
        if (activeObjective == null || !TryPrepareObjectiveItemNumConsume(runtime.Id, activeObjective, consumeItem))
        {
            return false;
        }

        if (!runtime.TryCompleteActiveObjective())
        {
            return false;
        }

        ConsumePreparedItemNumConditions();
        SaveRuntime(runtime);
        RefreshRuntimeStateAfterProgress(runtime);
        if (runtime.State == QuestState.Active)
        {
            EnsureAndRefreshTimeProgress(runtime);
            EnsureAndRefreshItemNumProgress(runtime);
        }

        return true;
    }

    /// <summary>
    /// 领取任务奖励。
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>是否领取成功</returns>
    public bool ClaimReward(string questId)
    {
        QuestRuntime runtime = GetQuest(questId);
        if (runtime == null || runtime.State != QuestState.Completed)
        {
            return false;
        }

        // 先落盘领奖状态 避免奖励事件回调重入后重复发奖
        ChangeState(runtime, QuestState.Rewarded);
        SendRewards(runtime);
        return true;
    }

    #endregion

    #region 道具条件

    /// <summary>
    /// 尝试准备任务目标物品数量消耗
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <param name="objective">任务运行时目标</param>
    /// <param name="consumeItem">是否消耗物品</param>
    /// <returns>是否成功准备</returns>
    private bool TryPrepareObjectiveItemNumConsume(string questId, ObjectiveRuntime objective, bool consumeItem)
    {
        itemNumConsumeCache.Clear();
        if (objective == null)
        {
            return false;
        }

        objective.RefreshItemNumProgress(questId, itemService);
        if (!objective.IsConditionsSatisfied)
        {
            return false;
        }

        itemNumConditionCache.Clear();
        objective.CollectItemNumConditions(itemNumConditionCache);
        if (!consumeItem || itemNumConditionCache.Count == 0)
        {
            return true;
        }

        QuestConditionGroupType groupType = objective.Data.ConditionGroupType;
        return groupType == QuestConditionGroupType.Or
            ? TryPrepareFirstSatisfiedItemNumCondition(itemNumConditionCache, itemNumConsumeCache)
            : TryPrepareAllSatisfiedItemNumConditions(itemNumConditionCache, itemNumConsumeCache);
    }

    /// <summary>
    /// 准备扣除所有已满足的道具数量条件。
    /// </summary>
    /// <param name="conditions">道具数量条件列表</param>
    /// <param name="consumeConditions">待扣除条件列表</param>
    /// <returns>准备成功返回true</returns>
    private bool TryPrepareAllSatisfiedItemNumConditions(List<ConditionRuntime> conditions,
        List<ConditionRuntime> consumeConditions)
    {
        Dictionary<int, float> requiredCounts = new Dictionary<int, float>();
        for (int i = 0; i < conditions.Count; i++)
        {
            ConditionRuntime condition = conditions[i];
            if (condition == null || !condition.IsCompleted)
            {
                consumeConditions.Clear();
                return false;
            }

            if (!condition.TryGetItemNumPropId(out int propId))
            {
                consumeConditions.Clear();
                return false;
            }

            if (!requiredCounts.ContainsKey(propId))
            {
                requiredCounts.Add(propId, 0);
            }

            requiredCounts[propId] += condition.TargetCount;
            consumeConditions.Add(condition);
        }

        foreach (KeyValuePair<int, float> requiredCount in requiredCounts)
        {
            if (!itemService.HasItem(requiredCount.Key, requiredCount.Value))
            {
                consumeConditions.Clear();
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 准备扣除第一个已满足的道具数量条件。
    /// </summary>
    /// <param name="conditions">道具数量条件列表</param>
    /// <param name="consumeConditions">待扣除条件列表</param>
    /// <returns>准备成功返回true</returns>
    private bool TryPrepareFirstSatisfiedItemNumCondition(List<ConditionRuntime> conditions,
        List<ConditionRuntime> consumeConditions)
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if (!CanConsumeItemNumCondition(conditions[i]))
            {
                continue;
            }

            consumeConditions.Add(conditions[i]);
            return true;
        }

        return true;
    }

    /// <summary>
    /// 判断道具数量条件是否可扣除。
    /// </summary>
    /// <param name="condition">道具数量条件</param>
    /// <returns>可扣除返回true</returns>
    private bool CanConsumeItemNumCondition(ConditionRuntime condition)
    {
        if (condition == null || !condition.IsCompleted)
        {
            return false;
        }

        return condition.TryGetItemNumPropId(out int propId)
               && itemService.HasItem(propId, condition.TargetCount);
    }

    /// <summary>
    /// 扣除道具数量条件对应的背包道具。
    /// </summary>
    /// <param name="condition">道具数量条件</param>
    private void ConsumeItemNumCondition(ConditionRuntime condition)
    {
        if (condition != null && condition.TryGetItemNumPropId(out int propId))
        {
            itemService.TryConsume(propId, condition.TargetCount);
        }
    }

    /// <summary>
    /// 扣除已准备的道具数量条件道具。
    /// </summary>
    private void ConsumePreparedItemNumConditions()
    {
        for (int i = 0; i < itemNumConsumeCache.Count; i++)
        {
            ConsumeItemNumCondition(itemNumConsumeCache[i]);
        }

        itemNumConsumeCache.Clear();
    }

    #endregion

    #region 查询接口

    /// <summary>
    /// 获取任务运行时数据。
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务运行时数据</returns>
    public QuestRuntime GetQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
        {
            return null;
        }

        if (questRuntimes.TryGetValue(questId, out QuestRuntime runtime))
        {
            return runtime;
        }

        if (!questDatas.TryGetValue(questId, out QuestData questData))
        {
            return null;
        }

        runtime = CreateRuntime(questData);
        questRuntimes[questId] = runtime;
        RefreshQuestAvailability(runtime);
        return runtime;
    }

    /// <summary>
    /// 获取任务静态配置
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>任务配置，不存在时返回null</returns>
    public QuestData GetStaticData(string questId)
    {
        if (!string.IsNullOrWhiteSpace(questId)
            && questDatas.TryGetValue(questId, out QuestData questData))
        {
            return questData;
        }

        return null;
    }

    /// <summary>
    /// 获取所有已注册任务。
    /// </summary>
    /// <returns>任务运行时列表</returns>
    public List<QuestRuntime> GetAllQuests()
    {
        return new List<QuestRuntime>(questRuntimes.Values);
    }

    #endregion

    #region 进度输入

    /// <summary>
    /// 直接提交任务进度
    /// </summary>
    /// <param name="progressEvent">任务进度事件</param>
    public void ReportProgress(QuestProgressEvent progressEvent)
    {
        if (progressEvent == null)
        {
            return;
        }

        List<QuestRuntime> runtimes = new List<QuestRuntime>(questRuntimes.Values);
        for (int i = 0; i < runtimes.Count; i++)
        {
            QuestRuntime runtime = runtimes[i];
            if (progressEvent.EventType == QuestProgressEventType.ItemNum)
            {
                EnsureAndRefreshItemNumProgress(runtime);
                continue;
            }

            if (!runtime.HandleProgress(progressEvent))
            {
                continue;
            }

            SaveRuntime(runtime);
            RefreshRuntimeStateAfterProgress(runtime);
        }
    }

    #endregion

    #region 运行时刷新

    /// <summary>
    /// 创建任务运行时实例
    /// </summary>
    /// <param name="questData">任务数据</param>
    /// <returns>返回创建的任务运行时实例</returns>
    private QuestRuntime CreateRuntime(QuestData questData)
    {
        SaveQuestItem saveItem = saveStore.GetQuest(questData.Id);
        bool isNewQuest = saveItem == null;
        saveItem ??= saveStore.GetOrCreateQuest(questData.Id);
        string today = timeProvider.LocalDateKey;
        bool shouldRefreshDaily = !isNewQuest
                                  && questData.TaskType == TaskType.EveryDay
                                  && saveItem.dailyRefreshDate != today;
        bool wasAccepted = saveItem.state >= QuestState.Active;
        QuestRuntime runtime = new QuestRuntime(questData, isNewQuest || shouldRefreshDaily ? null : saveItem,
            SendObjectiveChangedEvent);
        if (shouldRefreshDaily)
        {
            runtime.SetState(wasAccepted ? QuestState.Active : QuestState.Locked);
        }

        if (questData.TaskType == TaskType.EveryDay)
        {
            SaveQuestItem dirtySaveItem = saveStore.GetOrCreateQuest(questData.Id);
            dirtySaveItem.dailyRefreshDate = today;
            saveStore.MarkDirty();
        }

        return runtime;
    }

    /// <summary>
    /// 检查日期变化并刷新每日任务。
    /// </summary>
    private void RefreshDailyQuestsIfNeeded()
    {
        string today = timeProvider.LocalDateKey;
        if (dailyRefreshDate == today)
        {
            return;
        }

        dailyRefreshDate = today;
        List<QuestRuntime> runtimes = new List<QuestRuntime>(questRuntimes.Values);
        for (int i = 0; i < runtimes.Count; i++)
        {
            QuestRuntime runtime = runtimes[i];
            if (runtime?.Data == null || runtime.Data.TaskType != TaskType.EveryDay)
            {
                continue;
            }

            QuestState oldState = runtime.State;
            bool wasAccepted = oldState >= QuestState.Active;
            runtime.ResetForDailyRefresh(wasAccepted);
            SaveQuestItem saveItem = saveStore.GetOrCreateQuest(runtime.Id);
            saveItem.dailyRefreshDate = today;
            saveStore.MarkDirty();
            SaveRuntime(runtime);
            if (!wasAccepted)
            {
                RefreshQuestAvailability(runtime);
            }

            if (oldState != runtime.State)
            {
                SendQuestChangedEvent(runtime, oldState);
            }

            EnsureAndRefreshTimeProgress(runtime);
            EnsureAndRefreshItemNumProgress(runtime);
            SendQuestResetEvent(runtime, true);
        }

        RefreshAllQuestAvailability();
    }

    /// <summary>
    /// 刷新所有任务的时间进度
    /// 该方法会遍历当前所有的运行时任务，并确保每个任务的时间进度都得到更新
    /// </summary>
    private void RefreshAllTimeProgress()
    {
        // 创建一个包含所有运行时任务副本的列表，以避免在遍历过程中修改原始集合
        List<QuestRuntime> runtimes = new List<QuestRuntime>(questRuntimes.Values);
        // 遍历任务列表，对每个任务确保并刷新其时间进度
        for (int i = 0; i < runtimes.Count; i++)
        {
            // 调用EnsureAndRefreshTimeProgress方法处理单个任务的时间进度
            EnsureAndRefreshTimeProgress(runtimes[i]);
        }
    }

    /// <summary>
    /// 刷新所有背包道具数量任务条件。
    /// </summary>
    private void RefreshAllItemNumProgress()
    {
        List<QuestRuntime> runtimes = new List<QuestRuntime>(questRuntimes.Values);
        for (int i = 0; i < runtimes.Count; i++)
        {
            EnsureAndRefreshItemNumProgress(runtimes[i]);
        }
    }

    /// <summary>
    /// 刷新单个任务的背包道具数量条件。
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void EnsureAndRefreshItemNumProgress(QuestRuntime runtime)
    {
        if (runtime == null || runtime.State < QuestState.Active || runtime.State == QuestState.Rewarded)
        {
            return;
        }

        bool changed = runtime.RefreshItemNumProgress(itemService);
        if (!changed)
        {
            return;
        }

        SaveRuntime(runtime);
        RefreshRuntimeStateAfterProgress(runtime);
    }

    /// <summary>
    /// 确保并刷新任务时间进度
    /// </summary>
    /// <param name="runtime">任务运行时对象</param>
    private void EnsureAndRefreshTimeProgress(QuestRuntime runtime)
    {
        // 检查runtime是否为null或任务状态不是"Active"
        if (runtime == null || runtime.State != QuestState.Active)
        {
            return;
        }

        // 获取当前UTC时间的秒数
        long utcSeconds = timeProvider.UtcNowSeconds;
        // 检查并更新任务开始时间条件，返回是否有变化
        bool changed = runtime.StartTimeConditions(utcSeconds);
        // 刷新任务时间进度，并与之前的变化结果进行或运算
        changed |= runtime.RefreshTimeProgress(utcSeconds);
        // 如果没有发生任何变化，则直接返回
        if (!changed)
        {
            return;
        }

        // 保存任务运行时状态
        SaveRuntime(runtime);
        // 刷新任务进度后的状态
        RefreshRuntimeStateAfterProgress(runtime);
    }

    /// <summary>
    /// 根据目标完成情况刷新任务状态。
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void RefreshRuntimeStateAfterProgress(QuestRuntime runtime)
    {
        if (runtime == null || runtime.State != QuestState.Active)
        {
            return;
        }

        for (int i = 0; i < runtime.Objectives.Count; i++)
        {
            ObjectiveRuntime objective = runtime.Objectives[i];
            if (objective != null && objective.Data.AutoComplete
                                  && !objective.IsCompleted && objective.IsConditionsSatisfied)
            {
                objective.TryComplete(runtime.Id);
            }
        }

        if (runtime.IsAllObjectivesCompleted())
        {
            ChangeState(runtime, QuestState.Completed);
            return;
        }
    }

    /// <summary>
    /// 检查是否可以接受任务
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    /// <returns>如果可以接受任务返回true，否则返回false</returns>
    private bool CanAcceptQuest(QuestRuntime runtime)
    {
        if (runtime?.Data?.PrerequisiteQuestIds == null)
        {
            return true;
        }

        for (int i = 0; i < runtime.Data.PrerequisiteQuestIds.Count; i++)
        {
            string prerequisiteId = runtime.Data.PrerequisiteQuestIds[i];
            if (string.IsNullOrWhiteSpace(prerequisiteId))
            {
                continue;
            }

            QuestRuntime prerequisiteQuest = GetQuest(prerequisiteId);
            if (prerequisiteQuest == null || prerequisiteQuest.State < QuestState.Completed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 更改任务状态
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    /// <param name="state">新的任务状态</param>
    private void ChangeState(QuestRuntime runtime, QuestState state)
    {
        if (runtime == null)
        {
            return;
        }

        QuestState oldState = runtime.State;
        if (oldState == state)
        {
            return;
        }

        runtime.SetState(state);
        SaveRuntime(runtime);
        SendQuestChangedEvent(runtime, oldState);

        // 任务完成或领奖后刷新依赖任务状态
        if (oldState >= QuestState.Completed || state >= QuestState.Completed)
        {
            RefreshAllQuestAvailability();
        }
    }

    /// <summary>
    /// 发送任务状态变化事件
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    /// <param name="oldState">旧任务状态</param>
    private void SendQuestChangedEvent(QuestRuntime runtime, QuestState oldState)
    {
        QuestChangedEvent changedEvent = new QuestChangedEvent
        {
            QuestId = runtime.Id,
            OldState = oldState,
            NewState = runtime.State,
            Quest = runtime
        };
        QuestChanged?.Invoke(changedEvent);
    }

    /// <summary>
    /// 转发目标进度变化事件
    /// </summary>
    /// <param name="changedEvent">目标进度变化事件</param>
    private void SendObjectiveChangedEvent(QuestObjectiveChangedEvent changedEvent)
    {
        ObjectiveChanged?.Invoke(changedEvent);
    }

    /// <summary>
    /// 发送任务进度重置事件
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    /// <param name="isDailyReset">是否每日重置</param>
    private void SendQuestResetEvent(QuestRuntime runtime, bool isDailyReset)
    {
        QuestResetEvent resetEvent = new QuestResetEvent
        {
            QuestId = runtime.Id,
            IsDailyReset = isDailyReset,
            Quest = runtime
        };
        QuestReset?.Invoke(resetEvent);
    }

    /// <summary>
    /// 发放任务奖励
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void SendRewards(QuestRuntime runtime)
    {
        if (runtime?.Data?.Rewards == null)
        {
            return;
        }

        for (int i = 0; i < runtime.Data.Rewards.Count; i++)
        {
            QuestRewardData rewardData = runtime.Data.Rewards[i];
            if (rewardData == null || rewardData.Count <= 0)
            {
                continue;
            }

            QuestRewardEvent rewardEvent = new QuestRewardEvent
            {
                QuestId = runtime.Id,
                RewardIndex = i,
                RewardData = rewardData
            };
            RewardIssued?.Invoke(rewardEvent);
        }
    }

    /// <summary>
    /// 保存任务运行时状态
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void SaveRuntime(QuestRuntime runtime)
    {
        if (runtime == null || string.IsNullOrWhiteSpace(runtime.Id))
        {
            return;
        }

        SaveQuestItem saveItem = saveStore.GetOrCreateQuest(runtime.Id);
        runtime.WriteToSave(saveItem);
        saveStore.MarkDirty();
    }

    /// <summary>
    /// 刷新所有任务的可接受状态
    /// </summary>
    private void RefreshAllQuestAvailability()
    {
        List<QuestRuntime> runtimes = new List<QuestRuntime>(questRuntimes.Values);
        for (int i = 0; i < runtimes.Count; i++)
        {
            RefreshQuestAvailability(runtimes[i]);
        }
    }

    /// <summary>
    /// 刷新单个任务的可接受状态
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void RefreshQuestAvailability(QuestRuntime runtime)
    {
        if (runtime == null || runtime.State >= QuestState.Active)
        {
            return;
        }

        QuestState targetState = CanAcceptQuest(runtime) ? QuestState.Available : QuestState.Locked;
        if (runtime.State == targetState)
        {
            return;
        }

        ChangeState(runtime, targetState);
    }

    /// <summary>
    /// 检查任务配置并输出可定位的警告
    /// </summary>
    /// <param name="questData">任务配置</param>
    private static void ValidateQuestConfiguration(QuestData questData)
    {
        if (!Enum.IsDefined(typeof(TaskType), questData.TaskType))
        {
            Debug.LogWarning($"任务 {questData.Id} 的任务类型无效", questData);
        }

        if (questData.Objectives == null || questData.Objectives.Count == 0)
        {
            Debug.LogWarning($"任务 {questData.Id} 未配置任何目标", questData);
        }

        for (int objectiveIndex = 0; objectiveIndex < (questData.Objectives?.Count ?? 0); objectiveIndex++)
        {
            QuestObjectiveData objective = questData.Objectives[objectiveIndex];
            if (objective == null)
            {
                Debug.LogWarning($"任务 {questData.Id} 的目标列表第 {objectiveIndex} 项为空", questData);
                continue;
            }

            if (!Enum.IsDefined(typeof(QuestConditionGroupType), objective.ConditionGroupType))
            {
                Debug.LogWarning($"任务 {questData.Id} 的目标 {objectiveIndex + 1} 条件组合方式无效", questData);
            }

            if (objective.Conditions == null || objective.Conditions.Count == 0)
            {
                Debug.LogWarning($"任务 {questData.Id} 的目标 {objectiveIndex + 1} 未配置任何条件", questData);
            }

            for (int conditionIndex = 0; conditionIndex < (objective.Conditions?.Count ?? 0); conditionIndex++)
            {
                QuestConditionData condition = objective.Conditions[conditionIndex];
                if (condition == null)
                {
                    Debug.LogWarning(
                        $"任务 {questData.Id} 的目标 {objectiveIndex + 1} 条件列表第 {conditionIndex} 项为空", questData);
                    continue;
                }

                if (!Enum.IsDefined(typeof(QuestConditionType), condition.ConditionType)
                    || !Enum.IsDefined(typeof(QuestProgressEventType), condition.EventType))
                {
                    Debug.LogWarning($"任务 {questData.Id} 的条件 {conditionIndex + 1} 类型无效", questData);
                }

                if (condition.TargetCount <= 0)
                {
                    Debug.LogWarning($"任务 {questData.Id} 的条件 {conditionIndex + 1} 目标数量必须大于0", questData);
                }

                if (condition.EventType == QuestProgressEventType.ItemNum
                    && (!int.TryParse(condition.TargetId, out int propId) || propId <= 0))
                {
                    Debug.LogWarning($"任务 {questData.Id} 的道具数量条件 {conditionIndex + 1} 未填写有效道具ID", questData);
                }
            }
        }

        for (int rewardIndex = 0; rewardIndex < (questData.Rewards?.Count ?? 0); rewardIndex++)
        {
            QuestRewardData reward = questData.Rewards[rewardIndex];
            if (reward == null || reward.Count <= 0)
            {
                Debug.LogWarning($"任务 {questData.Id} 的奖励列表第 {rewardIndex} 项为空或数量无效", questData);
                continue;
            }

            if (!Enum.IsDefined(typeof(QuestRewardType), reward.RewardType))
            {
                Debug.LogWarning($"任务 {questData.Id} 的奖励列表第 {rewardIndex} 项类型无效", questData);
            }

            if (reward.RewardType == QuestRewardType.Item
                && (!int.TryParse(reward.TargetKey, out int propId) || propId <= 0))
            {
                Debug.LogWarning($"任务 {questData.Id} 的道具奖励第 {rewardIndex} 项未填写有效道具ID", questData);
            }
        }

        HashSet<string> prerequisiteIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < (questData.PrerequisiteQuestIds?.Count ?? 0); i++)
        {
            string prerequisiteId = questData.PrerequisiteQuestIds[i];
            if (string.IsNullOrWhiteSpace(prerequisiteId))
            {
                continue;
            }

            if (string.Equals(prerequisiteId, questData.Id, StringComparison.Ordinal))
            {
                Debug.LogWarning($"任务 {questData.Id} 不能将自身设为前置任务", questData);
            }
            else if (!prerequisiteIds.Add(prerequisiteId))
            {
                Debug.LogWarning($"任务 {questData.Id} 的前置任务ID重复 {prerequisiteId}", questData);
            }
        }
    }

    #endregion
}
