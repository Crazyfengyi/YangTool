using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用任务管理器
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("YangTools/Quest/Quest Manager")]
public sealed class QuestManager : MonoBehaviour
{
    /// <summary>
    /// 在线时长条件的进度更新间隔
    /// </summary>
    private const float OnlineTimeTickIntervalSeconds = 1f;

    /// <summary>
    /// 任务管理器单例引用
    /// </summary>
    private static QuestManager instance;

    /// <summary>
    /// 是否跨场景保留任务管理器
    /// </summary>
    [SerializeField]
    [InspectorName("跨场景保留")]
    private bool persistAcrossScenes = true;

    /// <summary>
    /// 已注册的任务配置
    /// </summary>
    private readonly Dictionary<string, QuestData> questDatas = new Dictionary<string, QuestData>(StringComparer.Ordinal);

    /// <summary>
    /// 已创建的任务运行时数据
    /// </summary>
    private readonly Dictionary<string, QuestRuntime> questRuntimes = new Dictionary<string, QuestRuntime>(StringComparer.Ordinal);

    /// <summary>
    /// 当前任务中的道具数量条件缓存
    /// </summary>
    private readonly List<ItemNumCondition> itemNumConditionCache = new List<ItemNumCondition>();

    /// <summary>
    /// 待消耗的道具数量条件缓存
    /// </summary>
    private readonly List<ItemNumCondition> itemNumConsumeCache = new List<ItemNumCondition>();

    /// <summary>
    /// 任务存档服务
    /// </summary>
    private IQuestSaveStore saveStore = new QuestMemorySaveStore();

    /// <summary>
    /// 任务道具服务
    /// </summary>
    private IQuestItemService itemService = NullQuestItemService.Instance;

    /// <summary>
    /// 任务奖励服务
    /// </summary>
    private IQuestRewardService rewardService = NullQuestRewardService.Instance;

    /// <summary>
    /// 任务时间服务
    /// </summary>
    private IQuestTimeProvider timeProvider = SystemQuestTimeProvider.Instance;

    /// <summary>
    /// 是否已经完成服务注入
    /// </summary>
    private bool servicesConfigured;

    /// <summary>
    /// 是否已经完成任务系统初始化
    /// </summary>
    private bool initialized;

    /// <summary>
    /// 是否已经完成当前批次任务加载
    /// </summary>
    private bool dataLoaded;

    /// <summary>
    /// 应用是否处于前台
    /// </summary>
    private bool isApplicationForeground = true;

    /// <summary>
    /// 真实时间条件的累计计时器
    /// </summary>
    private float timeQuestTimer;

    /// <summary>
    /// 在线时长条件的累计计时器
    /// </summary>
    private float onlineTimeTimer;

    /// <summary>
    /// 上一次每日任务刷新的日期键
    /// </summary>
    private string dailyRefreshDate;

    /// <summary>
    /// 任务系统是否已经初始化
    /// </summary>
    public bool IsInitialized => initialized;

    /// <summary>
    /// 任务配置是否已经完成加载
    /// </summary>
    public bool IsDataLoaded => dataLoaded;

    /// <summary>
    /// 是否已经注入任务系统服务
    /// </summary>
    public bool IsServicesConfigured => servicesConfigured;

    /// <summary>
    /// 当前场景中的任务管理器单例
    /// </summary>
    public static QuestManager Instance => instance;

    /// <summary>
    /// 任务状态发生变化时触发
    /// </summary>
    public event Action<QuestChangedEvent> QuestChanged;

    /// <summary>
    /// 任务目标进度发生变化时触发
    /// </summary>
    public event Action<QuestObjectiveChangedEvent> ObjectiveChanged;

    /// <summary>
    /// 在线时长发生变化时触发
    /// </summary>
    public event Action<float> OnlineTimeProgressed;

    /// <summary>
    /// 单项奖励发放完成后触发
    /// </summary>
    public event Action<QuestRewardEvent> RewardIssued;

    /// <summary>
    /// 任务重置后触发
    /// </summary>
    public event Action<QuestResetEvent> QuestReset;

    /// <summary>
    /// 任务配置加载完成后触发
    /// </summary>
    public event Action<QuestDataLoadedEvent> DataLoaded;

    /// <summary>
    /// 注册场景任务管理器单例
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
    /// 启动任务系统
    /// </summary>
    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 驱动每日刷新和在线时长事件
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
    private void OnApplicationFocus(bool hasFocus)
    {
        SetApplicationForeground(hasFocus);
    }

    /// <summary>
    /// 响应应用暂停和恢复
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        SetApplicationForeground(!pauseStatus);
    }

    /// <summary>
    /// 销毁任务系统并释放条件监听
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

    /// <summary>
    /// 注入任务系统依赖服务
    /// </summary>
    public void ConfigureServices(IQuestSaveStore questSaveStore, IQuestItemService questItemService,
        IQuestTimeProvider questTimeProvider = null, IQuestRewardService questRewardService = null)
    {
        if (questSaveStore == null)
        {
            throw new ArgumentNullException(nameof(questSaveStore));
        }

        if (dataLoaded || questDatas.Count > 0 || questRuntimes.Count > 0)
        {
            throw new InvalidOperationException("任务数据加载后不能替换依赖服务");
        }

        saveStore = questSaveStore;
        itemService = questItemService ?? NullQuestItemService.Instance;
        timeProvider = questTimeProvider ?? SystemQuestTimeProvider.Instance;
        rewardService = questRewardService ?? NullQuestRewardService.Instance;
        servicesConfigured = true;
    }

    /// <summary>
    /// 初始化任务系统
    /// </summary>
    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        dataLoaded = false;
        timeQuestTimer = 0f;
        onlineTimeTimer = 0f;
        dailyRefreshDate = timeProvider.LocalDateKey;
    }

    /// <summary>
    /// 使用任务配置初始化系统
    /// </summary>
    public void Initialize(IEnumerable<QuestData> questDataList)
    {
        Initialize();
        RegisterQuests(questDataList);
    }

    /// <summary>
    /// 重置全部任务
    /// </summary>
    public void ResetAll()
    {
        saveStore.Clear();
        foreach (QuestRuntime runtime in questRuntimes.Values)
        {
            runtime.DisposeConditions();
        }

        List<QuestData> registeredData = new List<QuestData>(questDatas.Values);
        questRuntimes.Clear();
        for (int i = 0; i < registeredData.Count; i++)
        {
            QuestData data = registeredData[i];
            if (data == null || string.IsNullOrWhiteSpace(data.Id))
            {
                continue;
            }

            QuestRuntime runtime = CreateRuntime(data);
            questRuntimes[data.Id] = runtime;
            RefreshQuestAvailability(runtime);
            if (runtime.State == QuestState.Active)
            {
                runtime.InitializeActiveObjective();
            }

            SaveRuntime(runtime);
            SendQuestResetEvent(runtime, false);
        }
    }

    /// <summary>
    /// 兼容旧接口并重置全部任务
    /// </summary>
    [Obsolete("请使用 ResetAll")]
    public void RestInit()
    {
        ResetAll();
    }

    /// <summary>
    /// 关闭任务系统
    /// </summary>
    public void Shutdown()
    {
        foreach (QuestRuntime runtime in questRuntimes.Values)
        {
            runtime.DisposeConditions();
        }

        questDatas.Clear();
        questRuntimes.Clear();
        itemNumConditionCache.Clear();
        itemNumConsumeCache.Clear();
        initialized = false;
        dataLoaded = false;
        servicesConfigured = false;
        dailyRefreshDate = string.Empty;
        timeQuestTimer = 0f;
        onlineTimeTimer = 0f;
        QuestChanged = null;
        ObjectiveChanged = null;
        OnlineTimeProgressed = null;
        RewardIssued = null;
        QuestReset = null;
        DataLoaded = null;
    }

    /// <summary>
    /// 设置任务系统是否接收在线时间
    /// </summary>
    public void SetApplicationForeground(bool isForeground)
    {
        isApplicationForeground = isForeground;
        if (!isForeground)
        {
            onlineTimeTimer = 0f;
        }
    }

    /// <summary>
    /// 手动驱动任务计时
    /// </summary>
    public void Tick(float unscaledDeltaTime)
    {
        if (!initialized || unscaledDeltaTime <= 0f)
        {
            return;
        }

        RefreshDailyQuestsIfNeeded();
        timeQuestTimer += unscaledDeltaTime;
        if (timeQuestTimer >= OnlineTimeTickIntervalSeconds)
        {
            float timeElapsedSeconds = timeQuestTimer;
            timeQuestTimer %= OnlineTimeTickIntervalSeconds;
            ReportProgress(new QuestProgressEvent(QuestProgressEventType.Time, string.Empty,
                value: timeElapsedSeconds));
        }

        if (!isApplicationForeground)
        {
            return;
        }

        onlineTimeTimer += unscaledDeltaTime;
        if (onlineTimeTimer < OnlineTimeTickIntervalSeconds)
        {
            return;
        }

        float elapsedSeconds = onlineTimeTimer;
        onlineTimeTimer %= OnlineTimeTickIntervalSeconds;
        ReportProgress(new QuestProgressEvent(QuestProgressEventType.OnLineTime, string.Empty,
            value: elapsedSeconds));
        OnlineTimeProgressed?.Invoke(elapsedSeconds);
    }

    /// <summary>
    /// 注册单个任务
    /// </summary>
    public void RegisterQuest(QuestData questData)
    {
        if (questData == null || string.IsNullOrWhiteSpace(questData.Id))
        {
            Debug.LogWarning("任务配置为空或任务ID为空 注册失败");
            return;
        }

        dataLoaded = false;
        ValidateQuestConfiguration(questData);
        if (questDatas.ContainsKey(questData.Id))
        {
            Debug.LogWarning($"任务ID重复 {questData.Id} 后注册的配置将覆盖先前配置");
            questRuntimes[questData.Id]?.DisposeConditions();
        }

        questDatas[questData.Id] = questData;
        QuestRuntime runtime = CreateRuntime(questData);
        questRuntimes[questData.Id] = runtime;
        RefreshQuestAvailability(runtime);
        if (runtime.State == QuestState.Active)
        {
            runtime.InitializeActiveObjective();
        }

        SaveRuntime(runtime);
    }

    /// <summary>
    /// 注册任务配置列表
    /// </summary>
    public void RegisterQuests(IEnumerable<QuestData> questDataList)
    {
        if (questDataList != null)
        {
            foreach (QuestData questData in questDataList)
            {
                RegisterQuest(questData);
            }
        }

        CompleteRegistration();
    }

    /// <summary>
    /// 完成当前批次任务注册
    /// </summary>
    public void CompleteRegistration()
    {
        Initialize();
        RefreshAllQuestAvailability();
        dataLoaded = true;
        DataLoaded?.Invoke(new QuestDataLoadedEvent { QuestCount = questRuntimes.Count });
    }

    /// <summary>
    /// 接取任务
    /// </summary>
    public bool AcceptQuest(string questId)
    {
        QuestRuntime runtime = GetQuest(questId);
        if (runtime == null || runtime.State != QuestState.Available || !CanAcceptQuest(runtime))
        {
            return false;
        }

        ChangeState(runtime, QuestState.Active);
        EnsureAndRefreshItemNumProgress(runtime);
        return true;
    }

    /// <summary>
    /// 手动确认完成当前任务目标
    /// </summary>
    public bool CompleteQuest(string questId, bool consumeItem = true)
    {
        QuestRuntime runtime = GetQuest(questId);
        if (runtime == null || runtime.State != QuestState.Active)
        {
            return false;
        }

        ObjectiveRuntime objective = runtime.GetActiveObjective();
        if (objective == null || !TryPrepareObjectiveItemNumConsume(objective, consumeItem)
            || !runtime.TryCompleteActiveObjective())
        {
            return false;
        }

        ConsumePreparedItemNumConditions();
        SaveRuntime(runtime);
        RefreshRuntimeStateAfterProgress(runtime);
        return true;
    }

    /// <summary>
    /// 领取任务奖励
    /// </summary>
    public bool ClaimReward(string questId)
    {
        QuestRuntime runtime = GetQuest(questId);
        if (runtime == null || runtime.State != QuestState.Completed)
        {
            return false;
        }

        ChangeState(runtime, QuestState.Rewarded);
        SendRewards(runtime);
        return true;
    }

    /// <summary>
    /// 获取任务运行时数据
    /// </summary>
    public QuestRuntime GetQuest(string questId)
    {
        return !string.IsNullOrEmpty(questId) && questRuntimes.TryGetValue(questId, out QuestRuntime runtime)
            ? runtime
            : null;
    }

    /// <summary>
    /// 获取任务静态配置
    /// </summary>
    public QuestData GetStaticData(string questId)
    {
        return !string.IsNullOrEmpty(questId) && questDatas.TryGetValue(questId, out QuestData data) ? data : null;
    }

    /// <summary>
    /// 获取全部任务
    /// </summary>
    public List<QuestRuntime> GetAllQuests()
    {
        return new List<QuestRuntime>(questRuntimes.Values);
    }

    /// <summary>
    /// 发布任务进度事件
    /// </summary>
    public void ReportProgress(QuestProgressEvent progressEvent)
    {
        QuestEventBus.Publish(progressEvent);
    }

    /// <summary>
    /// 根据任务配置和存档创建运行时任务
    /// </summary>
    /// <param name="questData">任务配置</param>
    /// <returns>任务运行时数据</returns>
    private QuestRuntime CreateRuntime(QuestData questData)
    {
        QuestSaveItem saveItem = saveStore.GetQuest(questData.Id);
        bool isNewQuest = saveItem == null;
        saveItem ??= saveStore.GetOrCreateQuest(questData.Id);
        bool dailyRefresh = !isNewQuest && questData.TaskType == TaskType.EveryDay
                            && saveItem.dailyRefreshDate != timeProvider.LocalDateKey;
        bool wasAccepted = saveItem.state >= QuestState.Active;
        QuestRuntime runtime = new QuestRuntime(questData, dailyRefresh ? null : saveItem,
            SendObjectiveChangedEvent, itemService);
        if (dailyRefresh)
        {
            runtime.SetState(wasAccepted ? QuestState.Active : QuestState.Locked);
        }

        if (questData.TaskType == TaskType.EveryDay)
        {
            saveItem.dailyRefreshDate = timeProvider.LocalDateKey;
            saveStore.MarkDirty();
        }

        return runtime;
    }

    /// <summary>
    /// 检查日期变化并刷新每日任务
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
            bool activate = oldState >= QuestState.Active;
            runtime.ResetForDailyRefresh(activate);
            QuestSaveItem saveItem = saveStore.GetOrCreateQuest(runtime.Id);
            saveItem.dailyRefreshDate = today;
            SaveRuntime(runtime);
            if (!activate)
            {
                RefreshQuestAvailability(runtime);
            }

            if (oldState != runtime.State)
            {
                SendQuestChangedEvent(runtime, oldState);
            }

            SendQuestResetEvent(runtime, true);
        }

        RefreshAllQuestAvailability();
    }

    /// <summary>
    /// 检查目标完成条件并准备需要消耗的道具
    /// </summary>
    /// <param name="objective">当前目标</param>
    /// <param name="consumeItem">是否需要消耗道具</param>
    /// <returns>可以完成目标返回true</returns>
    private bool TryPrepareObjectiveItemNumConsume(ObjectiveRuntime objective, bool consumeItem)
    {
        itemNumConsumeCache.Clear();
        objective.RefreshItemNumProgress(string.Empty, itemService);
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

        if (objective.Condition != null && objective.Condition.IsOrComposite)
        {
            for (int i = 0; i < itemNumConditionCache.Count; i++)
            {
                if (CanConsumeItemNumCondition(itemNumConditionCache[i]))
                {
                    itemNumConsumeCache.Add(itemNumConditionCache[i]);
                    return true;
                }
            }

            return true;
        }

        Dictionary<int, float> requiredCounts = new Dictionary<int, float>();
        for (int i = 0; i < itemNumConditionCache.Count; i++)
        {
            ItemNumCondition condition = itemNumConditionCache[i];
            if (!condition.IsCompleted || !TryGetItemNumPropId(condition, out int propId))
            {
                itemNumConsumeCache.Clear();
                return false;
            }

            requiredCounts[propId] = requiredCounts.TryGetValue(propId, out float count)
                ? count + condition.TargetCount
                : condition.TargetCount;
            itemNumConsumeCache.Add(condition);
        }

        foreach (KeyValuePair<int, float> required in requiredCounts)
        {
            if (!itemService.HasItem(required.Key, required.Value))
            {
                itemNumConsumeCache.Clear();
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 检查单个道具数量条件是否可以消耗
    /// </summary>
    /// <param name="condition">道具数量条件</param>
    /// <returns>道具足够并且条件完成返回true</returns>
    private bool CanConsumeItemNumCondition(ItemNumCondition condition)
    {
        return condition != null && condition.IsCompleted
               && TryGetItemNumPropId(condition, out int propId)
               && itemService.HasItem(propId, condition.TargetCount);
    }

    /// <summary>
    /// 消耗预先准备好的道具数量条件
    /// </summary>
    private void ConsumePreparedItemNumConditions()
    {
        for (int i = 0; i < itemNumConsumeCache.Count; i++)
        {
            ItemNumCondition condition = itemNumConsumeCache[i];
            if (TryGetItemNumPropId(condition, out int propId))
            {
                itemService.TryConsume(propId, condition.TargetCount);
            }
        }

        itemNumConsumeCache.Clear();
    }

    /// <summary>
    /// 将任务道具ID解析为项目背包道具ID
    /// </summary>
    /// <param name="condition">道具数量条件</param>
    /// <param name="propId">解析后的背包道具ID</param>
    /// <returns>解析成功返回true</returns>
    private static bool TryGetItemNumPropId(ItemNumCondition condition, out int propId)
    {
        propId = 0;
        return condition != null && int.TryParse(condition.ItemId, out propId) && propId > 0;
    }

    /// <summary>
    /// 刷新任务中的背包道具数量条件
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    private void EnsureAndRefreshItemNumProgress(QuestRuntime runtime)
    {
        if (runtime == null || runtime.State < QuestState.Active || runtime.State == QuestState.Rewarded)
        {
            return;
        }

        runtime.RefreshItemNumProgress(itemService);
    }

    /// <summary>
    /// 根据条件变化推进目标和任务状态
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    /// <param name="changedObjective">发生变化的目标</param>
    private void RefreshRuntimeStateAfterProgress(QuestRuntime runtime, ObjectiveRuntime changedObjective = null)
    {
        if (runtime == null || runtime.State != QuestState.Active)
        {
            return;
        }

        ObjectiveRuntime activeObjective = runtime.GetActiveObjective();
        if (changedObjective != null && changedObjective.IsCompleted
            && activeObjective != null && activeObjective != changedObjective)
        {
            runtime.DisposeConditions();
            runtime.InitializeActiveObjective();
            SaveRuntime(runtime);
            return;
        }

        ObjectiveRuntime objective = activeObjective;
        bool objectiveCompleted = false;
        if (objective != null && objective.Data.AutoComplete && objective.IsConditionsSatisfied)
        {
            objectiveCompleted = objective.TryComplete(runtime.Id);
        }

        if (runtime.IsAllObjectivesCompleted())
        {
            ChangeState(runtime, QuestState.Completed);
            return;
        }

        if (objectiveCompleted && runtime.GetActiveObjective() == objective)
        {
            runtime.DisposeConditions();
            runtime.InitializeActiveObjective();
            SaveRuntime(runtime);
        }
    }

    /// <summary>
    /// 检查任务前置条件是否满足
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    /// <returns>可以接取返回true</returns>
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

            QuestRuntime prerequisite = GetQuest(prerequisiteId);
            if (prerequisite == null || prerequisite.State < QuestState.Completed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 修改任务状态并处理对应生命周期
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    /// <param name="state">目标状态</param>
    private void ChangeState(QuestRuntime runtime, QuestState state)
    {
        if (runtime == null || runtime.State == state)
        {
            return;
        }

        QuestState oldState = runtime.State;
        if (oldState == QuestState.Active && state != QuestState.Active)
        {
            runtime.DisposeConditions();
        }

        runtime.SetState(state);
        if (state == QuestState.Active)
        {
            runtime.InitializeActiveObjective();
        }

        SaveRuntime(runtime);
        SendQuestChangedEvent(runtime, oldState);
        if (oldState >= QuestState.Completed || state >= QuestState.Completed)
        {
            RefreshAllQuestAvailability();
        }
    }

    /// <summary>
    /// 发布任务状态变化事件
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    /// <param name="oldState">变化前的状态</param>
    private void SendQuestChangedEvent(QuestRuntime runtime, QuestState oldState)
    {
        QuestChanged?.Invoke(new QuestChangedEvent
        {
            QuestId = runtime.Id,
            OldState = oldState,
            NewState = runtime.State,
            Quest = runtime
        });
    }

    /// <summary>
    /// 转发目标变化事件并保存任务状态
    /// </summary>
    /// <param name="changedEvent">目标变化事件</param>
    private void SendObjectiveChangedEvent(QuestObjectiveChangedEvent changedEvent)
    {
        if (changedEvent == null)
        {
            return;
        }

        ObjectiveChanged?.Invoke(changedEvent);
        QuestRuntime runtime = GetQuest(changedEvent.QuestId);
        if (runtime == null)
        {
            return;
        }

        SaveRuntime(runtime);
        RefreshRuntimeStateAfterProgress(runtime, changedEvent.Objective);
    }

    /// <summary>
    /// 发布任务重置事件
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    /// <param name="isDailyReset">是否为每日刷新</param>
    private void SendQuestResetEvent(QuestRuntime runtime, bool isDailyReset)
    {
        QuestReset?.Invoke(new QuestResetEvent
        {
            QuestId = runtime.Id,
            IsDailyReset = isDailyReset,
            Quest = runtime
        });
    }

    /// <summary>
    /// 依次发放任务奖励并上报结果
    /// </summary>
    /// <param name="runtime">已完成的任务运行时数据</param>
    private void SendRewards(QuestRuntime runtime)
    {
        if (runtime?.Data?.Rewards == null)
        {
            return;
        }

        for (int i = 0; i < runtime.Data.Rewards.Count; i++)
        {
            Reward reward = runtime.Data.Rewards[i];
            if (reward == null)
            {
                continue;
            }

            bool succeeded = false;
            string error = string.Empty;
            try
            {
                succeeded = reward.Give(rewardService);
                if (!succeeded)
                {
                    error = "奖励服务未能发放奖励";
                    Debug.LogError($"任务 {runtime.Id} 奖励发放失败 {reward.DisplayText}");
                }
            }
            catch (Exception exception)
            {
                error = exception.Message;
                Debug.LogException(exception);
            }

            RewardIssued?.Invoke(new QuestRewardEvent
            {
                QuestId = runtime.Id,
                RewardIndex = i,
                Reward = reward,
                Succeeded = succeeded,
                Error = error
            });
        }
    }

    /// <summary>
    /// 保存任务运行时状态
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    private void SaveRuntime(QuestRuntime runtime)
    {
        if (runtime == null || string.IsNullOrWhiteSpace(runtime.Id))
        {
            return;
        }

        QuestSaveItem saveItem = saveStore.GetOrCreateQuest(runtime.Id);
        runtime.WriteToSave(saveItem);
        saveStore.MarkDirty();
    }

    /// <summary>
    /// 刷新全部任务的可接取状态
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
    /// 刷新单个任务的可接取状态
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    private void RefreshQuestAvailability(QuestRuntime runtime)
    {
        if (runtime == null || runtime.State >= QuestState.Active)
        {
            return;
        }

        bool canAccept = CanAcceptQuest(runtime);
        QuestState targetState = canAccept && runtime.Data.DefaultActive
            ? QuestState.Active
            : canAccept ? QuestState.Available : QuestState.Locked;
        if (targetState != runtime.State)
        {
            ChangeState(runtime, targetState);
        }
    }

    /// <summary>
    /// 检查任务配置中的空引用和缺失条件
    /// </summary>
    /// <param name="questData">待检查的任务配置</param>
    private void ValidateQuestConfiguration(QuestData questData)
    {
        if (questData.Objectives == null || questData.Objectives.Count == 0)
        {
            Debug.LogWarning($"任务 {questData.Id} 未配置任何目标", questData);
        }

        for (int i = 0; i < (questData.Objectives?.Count ?? 0); i++)
        {
            QuestObjectiveData objective = questData.Objectives[i];
            if (objective == null)
            {
                continue;
            }

            if (objective.Condition == null)
            {
                Debug.LogWarning($"任务 {questData.Id} 的目标 {i + 1} 未配置任何条件", questData);
            }
        }

        for (int i = 0; i < (questData.Rewards?.Count ?? 0); i++)
        {
            if (questData.Rewards[i] == null)
            {
                Debug.LogWarning($"任务 {questData.Id} 的奖励列表第 {i + 1} 项为空", questData);
            }
        }
    }

    #region 使用示例

    /*
     * 任务系统使用流程
     *
     * 1  将 QuestManager 和 QuestManagerBootstrap 挂到场景对象上
     * 2  在 QuestData 中配置任务目标和奖励
     * 3  在业务层注入项目存档 道具 时间和奖励服务
     * 4  使用 RegisterQuest 或 Initialize 注册任务配置
     * 5  业务事件通过 ReportProgress 或 QuestEventBus 发布
     * 6  任务完成后调用 ClaimReward 领取奖励
     *
     * 示例代码
     *
     * QuestManager manager = QuestManager.Instance;
     * manager.ConfigureServices(saveStore, itemService, timeProvider, rewardService);
     * manager.Initialize(new[] { questData });
     * manager.AcceptQuest(questData.Id);
     * manager.ReportProgress(new QuestProgressEvent(
     *     QuestProgressEventType.Kill, "slime", amount: 1f));
     * manager.ClaimReward(questData.Id);
     *
     * 也可以直接发布强类型事件
     *
     * QuestEventBus.PublishMonsterKilled("slime");
     */

    #endregion
}
