using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameMain;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core;
using YangTools.Scripts.Core.YangSaveData;
using YangTools.Scripts.Core.YangUGUI;
using YooAsset;

/// <summary>
/// 通用任务管理器。
/// </summary>
public sealed class QuestManager
{
    private const float TimeQuestRefreshInterval = 1f;
    private const string QuestSoTag = "SO";

    private static QuestManager instance;
    public static QuestManager Instance => instance ??= new QuestManager();

    private readonly Dictionary<string, QuestData> questDatas = new Dictionary<string, QuestData>();
    private readonly Dictionary<string, QuestRuntime> questRuntimes = new Dictionary<string, QuestRuntime>();
    private readonly List<ConditionRuntime> itemNumConditionCache = new List<ConditionRuntime>();
    private readonly List<ConditionRuntime> itemNumConsumeCache = new List<ConditionRuntime>();
    private bool initialized;
    private float timeQuestRefreshTimer;

    private QuestManager()
    {
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
        AddProgressListener();
        LoadAllQuestDataFromSOAsync().Forget();
        RefreshAllTimeProgress();
        RefreshAllItemNumProgress();
    }

    /// <summary>
    /// 重置当前所有任务到初始状态。
    /// </summary>
    public void RestInit()
    {
        itemNumConditionCache.Clear();
        itemNumConsumeCache.Clear();
        timeQuestRefreshTimer = 0f;

        Save_QuestData saveData = GetSaveData(true);
        saveData.quests = new List<SaveQuestItem>();

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
            EnsureAndRefreshTimeProgress(runtime, false);
            EnsureAndRefreshItemNumProgress(runtime, false);
            SaveRuntime(runtime);
        }
    }

    /// <summary>
    /// 关闭任务系统并移除事件监听。
    /// </summary>
    public void Shutdown()
    {
        eventGroup.RemoveAllListener();
        initialized = false;
        timeQuestRefreshTimer = 0f;
    }

    /// <summary>
    /// 更新任务系统。
    /// </summary>
    /// <param name="unscaledDeltaTime">真实流逝时间</param>
    public void Update(float unscaledDeltaTime)
    {
        if (!initialized)
        {
            return;
        }

        timeQuestRefreshTimer += unscaledDeltaTime;
        if (timeQuestRefreshTimer < TimeQuestRefreshInterval)
        {
            return;
        }

        timeQuestRefreshTimer = 0f;
        RefreshAllTimeProgress();
    }

    /// <summary>
    /// 注册任务配置。
    /// </summary>
    /// <param name="questData">任务配置</param>
    public void RegisterQuest(QuestData questData)
    {
        if (questData == null || string.IsNullOrEmpty(questData.Id))
        {
            Debug.LogWarning("任务配置为空或任务ID为空，注册失败");
            return;
        }

        questDatas[questData.Id] = questData;
        QuestRuntime runtime = CreateRuntime(questData);
        questRuntimes[questData.Id] = runtime;
        EnsureAndRefreshTimeProgress(runtime);
        EnsureAndRefreshItemNumProgress(runtime);
        TryShowFirstObjectiveMoneyTip(runtime);
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
            return;
        }

        foreach (QuestData questData in questDataList)
        {
            RegisterQuest(questData);
        }
    }

    /// <summary>
    /// 接取任务。
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <returns>是否接取成功</returns>
    public bool AcceptQuest(string questId)
    {
        QuestRuntime runtime = GetQuest(questId);
        if (runtime == null || runtime.State == QuestState.Active)
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
        TryShowFirstObjectiveMoneyTip(runtime);
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

        TryShowFirstObjectiveMoneyTip(runtime);
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

        SendRewards(runtime);
        ChangeState(runtime, QuestState.Rewarded);
        return true;
    }

    /// <summary>
    /// 尝试准备任务目标物品数量消耗
    /// </summary>
    /// <param name="questId">任务ID</param>
    /// <param name="objective">任务运行时目标</param>
    /// <param name="consumeItem">是否消耗物品</param>
    /// <returns>是否成功准备</returns>
    private bool TryPrepareObjectiveItemNumConsume(string questId, ObjectiveRuntime objective, bool consumeItem)
    {
        // 清空物品数量消耗缓存
        itemNumConsumeCache.Clear();
        // 如果目标为空，返回false
        if (objective == null)
        {
            return false;
        }

        // 刷新任务目标的物品数量进度
        objective.RefreshItemNumProgress(questId);
        // 如果条件不满足，返回false
        if (!objective.IsConditionsSatisfied)
        {
            return false;
        }

    // 清空物品数量条件缓存
        itemNumConditionCache.Clear();
    // 收集物品数量条件
        objective.CollectItemNumConditions(itemNumConditionCache);
    // 如果不需要消耗物品或没有条件，返回true
        if (!consumeItem || itemNumConditionCache.Count == 0)
        {
            return true;
        }

    // 获取条件组合类型，如果目标数据为空则使用And类型
        QuestConditionGroupType groupType = objective.Data != null
            ? objective.Data.ConditionGroupType
            : QuestConditionGroupType.And;
    // 根据条件组合类型选择不同的处理方式
        return groupType == QuestConditionGroupType.Or
            ? TryPrepareFirstSatisfiedItemNumCondition(itemNumConditionCache, itemNumConsumeCache)  // 如果是Or类型，尝试准备第一个满足的条件
            : TryPrepareAllSatisfiedItemNumConditions(itemNumConditionCache, itemNumConsumeCache); // 如果是And类型，尝试准备所有满足的条件
    }

    /// <summary>
    /// 准备扣除所有已满足的道具数量条件。
    /// </summary>
    /// <param name="conditions">道具数量条件列表</param>
    /// <param name="consumeConditions">待扣除条件列表</param>
    /// <returns>准备成功返回true</returns>
    private static bool TryPrepareAllSatisfiedItemNumConditions(List<ConditionRuntime> conditions,
        List<ConditionRuntime> consumeConditions)
    {
        Dictionary<int, float> requiredCounts = new Dictionary<int, float>();
        for (int i = 0; i < conditions.Count; i++)
        {
            ConditionRuntime condition = conditions[i];
            if (condition == null || !condition.IsCompleted )//|| BagMgr.Instance == null)
            {
                return false;
            }

            int propId;
            if (!condition.TryGetItemNumPropId(out propId))
            {
                return false;
            }

            if (!requiredCounts.ContainsKey(propId))
            {
                requiredCounts.Add(propId, 0);
            }

            requiredCounts[propId] += condition.TargetCount;
            consumeConditions.Add(condition);
        }

        // foreach (KeyValuePair<int, float> requiredCount in requiredCounts)
        // {
        //     if (!BagMgr.Instance.BagPropEnough(requiredCount.Key, requiredCount.Value, false))
        //     {
        //         consumeConditions.Clear();
        //         return false;
        //     }
        // }

        return true;
    }

    /// <summary>
    /// 准备扣除第一个已满足的道具数量条件。
    /// </summary>
    /// <param name="conditions">道具数量条件列表</param>
    /// <param name="consumeConditions">待扣除条件列表</param>
    /// <returns>准备成功返回true</returns>
    private static bool TryPrepareFirstSatisfiedItemNumCondition(List<ConditionRuntime> conditions,
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
    private static bool CanConsumeItemNumCondition(ConditionRuntime condition)
    {
        if (condition == null || !condition.IsCompleted )//|| BagMgr.Instance == null)
        {
            return false;
        }

        int propId;
        return condition.TryGetItemNumPropId(out propId);
        // && BagMgr.Instance.BagPropEnough(propId, condition.TargetCount, false);
    }

    /// <summary>
    /// 扣除道具数量条件对应的背包道具。
    /// </summary>
    /// <param name="condition">道具数量条件</param>
    private static void ConsumeItemNumCondition(ConditionRuntime condition)
    {
        int propId;
        if (condition != null && condition.TryGetItemNumPropId(out propId))
        {
            //BagMgr.Instance.RemoveBagProp(propId, condition.TargetCount);
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
        return runtime;
    }

    public QuestData GetStaticData(string questId)
    {
        if(questDatas.TryGetValue(questId, out QuestData questData))
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

    private readonly YangEventGroup eventGroup = new YangEventGroup();

    /// <summary>
    /// 添加任务进度监听器
    /// </summary>
    private void AddProgressListener()
    {
        eventGroup.AddListener<QuestProgressEvent>(OnQuestProgressEvent);
        //eventGroup.AddListener<BagPropChange>(OnBagPropChange);
    }

    /// <summary>
    /// 初始化时加载SO标签下的全部任务配置。
    /// </summary>
    private async UniTaskVoid LoadAllQuestDataFromSOAsync()
    {
        try
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            AssetInfo[] assetInfos = package.GetAssetInfos(QuestSoTag);
            if (assetInfos == null || assetInfos.Length == 0)
            {
                Debug.LogWarning($"未找到任务SO资源标签:{QuestSoTag}");
                return;
            }

            List<AssetInfo> sortedAssetInfos = new List<AssetInfo>(assetInfos);
            sortedAssetInfos.Sort((left, right) => string.CompareOrdinal(left.Address, right.Address));
            for (int i = 0; i < sortedAssetInfos.Count; i++)
            {
                await LoadAndRegisterQuestDataAsync(package, sortedAssetInfos[i]);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"加载任务SO失败:{exception.Message}");
        }
    }

    /// <summary>
    /// 加载并注册单个任务配置。
    /// </summary>
    /// <param name="package">默认资源包</param>
    /// <param name="assetInfo">资源信息</param>
    private async UniTask LoadAndRegisterQuestDataAsync(ResourcePackage package, AssetInfo assetInfo)
    {
        AssetHandle handle = package.LoadAssetAsync<ScriptableObject>(assetInfo.Address);
        await handle.ToUniTask();
        if (handle.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"加载任务SO失败:{assetInfo.Address} {handle.LastError}");
            return;
        }

        QuestData questData = handle.AssetObject as QuestData;
        if (questData == null)
        {
            return;
        }

        RegisterQuest(questData);
    }

    /// <summary>
    /// 处理任务进度事件
    /// </summary>
    /// <param name="eventData">事件数据</param>
    private void OnQuestProgressEvent(EventData eventData)
    {
        // 将事件数据转换为任务进度事件
        QuestProgressEvent progressEvent = eventData?.Args as QuestProgressEvent;
        // 如果事件数据无效则直接返回
        if (progressEvent == null)
        {
            return;
        }

        // 创建任务运行时列表的副本
        List<QuestRuntime> runtimes = new List<QuestRuntime>(questRuntimes.Values);
        // 遍历所有任务运行时
        for (int i = 0; i < runtimes.Count; i++)
        {
            QuestRuntime runtime = runtimes[i];
            if (progressEvent.EventType == QuestProgressEventType.ItemNum)
            {
                EnsureAndRefreshItemNumProgress(runtime);
                continue;
            }

            // 处理任务进度，如果返回false则继续下一个任务
            if (!runtime.HandleProgress(progressEvent))
            {
                continue;
            }

            // 保存任务运行时状态
            SaveRuntime(runtime);
            // 检查任务是否所有目标已完成
            RefreshRuntimeStateAfterProgress(runtime);
            TryShowFirstObjectiveMoneyTip(runtime);
        }
    }

    /// <summary>
    /// 处理背包道具变化事件。
    /// </summary>
    /// <param name="eventData">事件数据</param>
    private void OnBagPropChange(EventData eventData)
    {
        // BagPropChange bagPropChange = eventData?.Args as BagPropChange;
        // if (bagPropChange == null)
        // {
        //     return;
        // }

        RefreshAllItemNumProgress();
    }

    /// <summary>
    /// 创建任务运行时实例
    /// </summary>
    /// <param name="questData">任务数据</param>
    /// <returns>返回创建的任务运行时实例</returns>
    private QuestRuntime CreateRuntime(QuestData questData)
    {
        // 获取存档数据
        Save_QuestData saveData = GetSaveData();
        // 获取或创建任务存档项
        SaveQuestItem saveItem = saveData.GetOrCreateQuest(questData.Id);
        // 创建并返回任务运行时实例
        QuestRuntime runtime = new QuestRuntime(questData, saveItem);
        if (runtime.State < QuestState.Active)
        {
            runtime.SetState(QuestState.Active);
        }

        return runtime;
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
    private void EnsureAndRefreshItemNumProgress(QuestRuntime runtime, bool showMoneyTip = true)
    {
        if (runtime == null || runtime.State < QuestState.Active || runtime.State == QuestState.Rewarded)
        {
            return;
        }

        bool changed = runtime.RefreshItemNumProgress();
        if (!changed)
        {
            if (showMoneyTip)
            {
                TryShowFirstObjectiveMoneyTip(runtime);
            }

            return;
        }

        SaveRuntime(runtime);
        RefreshRuntimeStateAfterProgress(runtime);
        if (showMoneyTip)
        {
            TryShowFirstObjectiveMoneyTip(runtime);
        }
    }

    /// <summary>
    /// 确保并刷新任务时间进度
    /// </summary>
    /// <param name="runtime">任务运行时对象</param>
    private void EnsureAndRefreshTimeProgress(QuestRuntime runtime, bool showMoneyTip = true)
    {
        // 检查runtime是否为null或任务状态不是"Active"
        if (runtime == null || runtime.State != QuestState.Active)
        {
            return;
        }

        // 获取当前UTC时间的秒数
        long utcSeconds = GetCurrentUtcSeconds();
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
        // 尝试显示第一个目标的金钱提示
        if (showMoneyTip)
        {
            TryShowFirstObjectiveMoneyTip(runtime);
        }
    }

    /// <summary>
    /// 根据目标完成情况刷新任务状态。
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void RefreshRuntimeStateAfterProgress(QuestRuntime runtime)
    {
        if (runtime == null)
        {
            return;
        }

        if (runtime.State == QuestState.Active && runtime.IsAllObjectivesCompleted())
        {
            ChangeState(runtime, QuestState.Completed);
            return;
        }
    }

    /// <summary>
    /// 尝试显示提现任务第一个目标满足条件引导。
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void TryShowFirstObjectiveMoneyTip(QuestRuntime runtime)
    {
        if (runtime == null || runtime.State < QuestState.Active || runtime.State == QuestState.Rewarded)
        {
            return;
        }

        if (runtime.Data == null || runtime.Data.GetRewardCount(QuestRewardType.Money) <= 0)
        {
            return;
        }

        if (runtime.Objectives == null || runtime.Objectives.Count == 0)
        {
            return;
        }

        ObjectiveRuntime firstObjective = runtime.Objectives[0];
        if (firstObjective == null || (!firstObjective.IsCompleted && !firstObjective.IsConditionsSatisfied))
        {
            return;
        }

        SaveQuestItem saveItem = GetSaveData().GetOrCreateQuest(runtime.Id);
        if (saveItem.firstObjectiveMoneyTipShown)
        {
            return;
        }

        saveItem = GetSaveData(true).GetOrCreateQuest(runtime.Id);
        saveItem.firstObjectiveMoneyTipShown = true;
        //UIMonoInstance.OpenPanel<MoneyTipWindow>(GroupType.弹窗1, userData: new MoneyTipWindowData(runtime.Id));
    }

    /// <summary>
    /// 检查是否可以接受任务
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    /// <returns>如果可以接受任务返回true，否则返回false</returns>
    private bool CanAcceptQuest(QuestRuntime runtime)
    {
        // 如果没有前置任务要求，则可以直接接受
        if (runtime?.Data?.PrerequisiteQuestIds == null)
        {
            return true;
        }

        // 检查所有前置任务是否已完成
        for (int i = 0; i < runtime.Data.PrerequisiteQuestIds.Count; i++)
        {
            string prerequisiteId = runtime.Data.PrerequisiteQuestIds[i];
            // 跳过空的前置任务ID
            if (string.IsNullOrEmpty(prerequisiteId))
            {
                continue;
            }

            // 获取前置任务实例
            QuestRuntime prerequisiteQuest = GetQuest(prerequisiteId);
            // 如果前置任务不存在或未完成，则不能接受当前任务
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
        // 获取旧的任务状态
        QuestState oldState = runtime.State;
        // 如果状态没有变化，则直接返回
        if (oldState == state)
        {
            return;
        }

        // 设置新的任务状态
        runtime.SetState(state);
        // 保存任务运行时状态
        SaveRuntime(runtime);
        // 发送任务状态变更事件
        new QuestChangedEvent
        {
            QuestId = runtime.Id,
            OldState = oldState,
            NewState = state,
            Quest = runtime
        }.SendEvent();

        // 任务完成或领奖后，刷新依赖该任务的可接受状态。
        if (oldState >= QuestState.Completed || state >= QuestState.Completed)
        {
            RefreshAllQuestAvailability();
        }
    }

    /// <summary>
    /// 发放任务奖励
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void SendRewards(QuestRuntime runtime)
    {
        // 如果没有奖励，则直接返回
        if (runtime?.Data?.Rewards == null)
        {
            return;
        }

        // 遍历所有奖励并发送奖励事件
        for (int i = 0; i < runtime.Data.Rewards.Count; i++)
        {
            new QuestRewardEvent
            {
                QuestId = runtime.Id,
                RewardData = runtime.Data.Rewards[i]
            }.SendEvent();
        }
    }

    /// <summary>
    /// 保存任务运行时状态
    /// </summary>
    /// <param name="runtime">任务运行时实例</param>
    private void SaveRuntime(QuestRuntime runtime)
    {
        // 如果任务运行时为空，则直接返回
        if (runtime == null)
        {
            return;
        }

        // 获取存档数据，标记为已修改
        Save_QuestData saveData = GetSaveData(true);
        // 获取或创建任务存档项
        SaveQuestItem saveItem = saveData.GetOrCreateQuest(runtime.Id);
        // 将任务运行时状态写入存档
        runtime.WriteToSave(saveItem);
    }

    /// <summary>
    /// 获取任务存档数据
    /// </summary>
    /// <param name="isDirty">是否标记为已修改</param>
    /// <returns>返回任务存档数据</returns>
    private static Save_QuestData GetSaveData(bool isDirty = false)
    {
        // 从数据中心获取本地存档
        return YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_QuestData>(isDirty);
    }

    /// <summary>
    /// 获取当前UTC时间自Unix纪元（1970年1月1日）以来的秒数
    /// </summary>
    /// <returns>返回自Unix纪元以来的秒数</returns>
    private static long GetCurrentUtcSeconds()
    {
        // 创建一个表示Unix纪元（1970年1月1日00:00:00 UTC）的DateTime对象
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // 计算当前UTC时间与Unix纪元时间之间的差值，并转换为总秒数
        return (long) (DateTime.UtcNow - epoch).TotalSeconds;
    }

    /// <summary>
    /// 刷新所有任务的可接受状态
    /// </summary>
    private void RefreshAllQuestAvailability()
    {
        // 创建任务运行时列表的副本
        List<QuestRuntime> runtimes = new List<QuestRuntime>(questRuntimes.Values);
        // 遍历所有任务运行时，刷新其可接受状态
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
        // 如果任务为空或状态已经是活跃或更高，则直接返回
        if (runtime == null || runtime.State >= QuestState.Active)
        {
            return;
        }

        // 根据前置任务完成情况确定目标状态
        QuestState targetState = CanAcceptQuest(runtime) ? QuestState.Available : QuestState.Locked;
        // 如果当前状态已经是目标状态，则直接返回
        if (runtime.State == targetState)
        {
            return;
        }

        // 更改任务状态
        ChangeState(runtime, targetState);
    }
}
