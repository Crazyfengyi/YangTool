using NUnit.Framework;
using UnityEngine;

/// <summary>
/// QuestSystem 条件和奖励的 EditMode 测试
/// </summary>
public sealed class QuestSystemTests
{
    private int progressCount;
    private int killCount;

    [SetUp]
    public void SetUp()
    {
        progressCount = 0;
        killCount = 0;
        QuestEventBus.OnProgressReported += OnProgressReported;
        QuestEventBus.OnMonsterKilled += OnMonsterKilled;
    }

    [TearDown]
    public void TearDown()
    {
        QuestEventBus.OnMonsterKilled -= OnMonsterKilled;
        QuestEventBus.OnProgressReported -= OnProgressReported;
    }

    /// <summary>
    /// 验证击杀条件只累计匹配的怪物
    /// </summary>
    [Test]
    public void KillCondition_MatchesIdAndCompletesAtTarget()
    {
        KillCondition condition = new KillCondition { MonsterId = "slime", TargetCount = 2 };
        condition.Initialize();

        QuestEventBus.PublishMonsterKilled("goblin");
        Assert.AreEqual(0f, condition.CurrentCount);
        QuestEventBus.PublishMonsterKilled("slime");
        QuestEventBus.PublishMonsterKilled("slime");

        Assert.AreEqual(2f, condition.CurrentCount);
        Assert.IsTrue(condition.IsCompleted);
        condition.Dispose();
    }

    /// <summary>
    /// 验证条件释放后不会继续监听事件
    /// </summary>
    [Test]
    public void Condition_DisposeStopsListening()
    {
        KillCondition condition = new KillCondition { MonsterId = "slime", TargetCount = 2 };
        condition.Initialize();
        QuestEventBus.PublishMonsterKilled("slime");
        condition.Dispose();
        QuestEventBus.PublishMonsterKilled("slime");

        Assert.AreEqual(1f, condition.CurrentCount);
        Assert.IsFalse(condition.IsCompleted);
    }

    /// <summary>
    /// 验证收集、对话、地点、时间和通用进度条件
    /// </summary>
    [Test]
    public void SpecificConditions_ReactToTheirEvents()
    {
        CollectCondition collect = new CollectCondition { ItemId = "herb", TargetCount = 2 };
        TalkCondition talk = new TalkCondition { NpcId = "merchant" };
        ReachLocationCondition reach = new ReachLocationCondition { LocationId = "town" };
        TimeCondition time = new TimeCondition { TargetCount = 3 };
        OnlineTimeCondition online = new OnlineTimeCondition { TargetCount = 2 };
        AdsCondition ads = new AdsCondition { TargetId = "ad", TargetCount = 1 };
        PassNumCondition pass = new PassNumCondition { TargetId = "level", TargetCount = 2 };
        Condition[] conditions = { collect, talk, reach, time, online, ads, pass };
        for (int i = 0; i < conditions.Length; i++) conditions[i].Initialize();

        QuestEventBus.PublishItemAdded("herb", 2);
        QuestEventBus.PublishNpcTalked("merchant");
        QuestEventBus.PublishLocationEntered("town");
        QuestEventBus.Publish(new QuestProgressEvent(QuestProgressEventType.Time, string.Empty, value: 3));
        QuestEventBus.Publish(new QuestProgressEvent(QuestProgressEventType.OnLineTime, string.Empty, value: 2));
        QuestEventBus.Publish(new QuestProgressEvent(QuestProgressEventType.Ads, "ad"));
        QuestEventBus.Publish(new QuestProgressEvent(QuestProgressEventType.PassNum, "level", amount: 2));

        for (int i = 0; i < conditions.Length; i++)
        {
            Assert.IsTrue(conditions[i].IsCompleted, conditions[i].GetType().Name);
            conditions[i].Dispose();
        }
    }

    /// <summary>
    /// 验证道具数量条件使用注入的背包服务
    /// </summary>
    [Test]
    public void ItemNumCondition_UsesItemService()
    {
        QuestData data = ScriptableObject.CreateInstance<QuestData>();
        data.Id = "quest_item_num_test";
        data.Objectives.Add(new QuestObjectiveData
        {
            Condition = new ItemNumCondition { ItemId = "100", TargetCount = 3 }
        });
        QuestRuntime runtime = new QuestRuntime(data,
            new QuestSaveItem { questId = data.Id, state = QuestState.Active },
            itemService: new TestItemService(3));

        runtime.InitializeActiveObjective();

        Assert.AreEqual(3f, runtime.Objectives[0].Condition.CurrentCount);
        Assert.IsTrue(runtime.Objectives[0].Condition.IsCompleted);
        runtime.DisposeConditions();
        Object.DestroyImmediate(data);
    }

    /// <summary>
    /// 验证组合条件支持嵌套并按逻辑计算完成状态
    /// </summary>
    [Test]
    public void CompositeCondition_SupportsAndOrNesting()
    {
        AndCondition and = new AndCondition
        {
            Children = new System.Collections.Generic.List<Condition>
            {
                new KillCondition { MonsterId = "slime", TargetCount = 1 },
                new OrCondition
                {
                    Children = new System.Collections.Generic.List<Condition>
                    {
                        new TalkCondition { NpcId = "merchant" },
                        new ReachLocationCondition { LocationId = "town" }
                    }
                }
            }
        };
        and.Initialize();

        QuestEventBus.PublishMonsterKilled("slime");
        Assert.IsFalse(and.IsCompleted);
        QuestEventBus.PublishLocationEntered("town");

        Assert.IsTrue(and.IsCompleted);
        and.Dispose();
    }

    /// <summary>
    /// 验证运行时条件是配置条件的独立副本
    /// </summary>
    [Test]
    public void QuestRuntime_CopiesConditionWithoutMutatingAsset()
    {
        QuestData data = ScriptableObject.CreateInstance<QuestData>();
        data.Id = "quest_copy_test";
        data.DefaultActive = true;
        data.Objectives.Add(new QuestObjectiveData
        {
            Condition = new KillCondition { MonsterId = "slime", TargetCount = 1 }
        });

        QuestRuntime runtime = new QuestRuntime(data,
            new QuestSaveItem { questId = data.Id, state = QuestState.Active });
        Condition configured = data.Objectives[0].Condition;
        Condition copied = runtime.Objectives[0].Condition.Condition;
        runtime.InitializeActiveObjective();
        QuestEventBus.PublishMonsterKilled("slime");

        Assert.AreNotSame(configured, copied);
        Assert.IsFalse(configured.IsCompleted);
        Assert.IsTrue(copied.IsCompleted);
        runtime.DisposeConditions();
        Object.DestroyImmediate(data);
    }

    /// <summary>
    /// 验证条件进度可以写入并恢复
    /// </summary>
    [Test]
    public void QuestRuntime_RestoresConditionProgress()
    {
        QuestData data = ScriptableObject.CreateInstance<QuestData>();
        data.Id = "quest_save_test";
        data.Objectives.Add(new QuestObjectiveData
        {
            Condition = new KillCondition { MonsterId = "slime", TargetCount = 2 }
        });

        QuestRuntime first = new QuestRuntime(data,
            new QuestSaveItem { questId = data.Id, state = QuestState.Active });
        first.InitializeActiveObjective();
        QuestEventBus.PublishMonsterKilled("slime");
        QuestSaveItem save = new QuestSaveItem();
        first.WriteToSave(save);
        first.DisposeConditions();

        QuestRuntime second = new QuestRuntime(data, save);
        Assert.AreEqual(1f, second.Objectives[0].Condition.CurrentCount);
        second.DisposeConditions();
        Object.DestroyImmediate(data);
    }

    /// <summary>
    /// 验证多目标只激活当前目标的条件监听
    /// </summary>
    [Test]
    public void QuestManager_ActivatesObjectivesSequentially()
    {
        QuestData data = ScriptableObject.CreateInstance<QuestData>();
        data.Id = "quest_sequence_test";
        data.DefaultActive = true;
        data.Objectives.Add(CreateKillObjective("first"));
        data.Objectives.Add(CreateKillObjective("second"));

        GameObject gameObject = new GameObject("QuestSequenceManager");
        QuestManager manager = gameObject.AddComponent<QuestManager>();
        manager.ConfigureServices(new QuestMemorySaveStore(), NullQuestItemService.Instance,
            questRewardService: new QuestMemoryRewardService());
        manager.Initialize(new[] { data });

        QuestEventBus.PublishMonsterKilled("first");
        QuestRuntime runtime = manager.GetQuest(data.Id);
        Assert.AreEqual(1f, runtime.Objectives[0].Condition.CurrentCount);
        Assert.AreEqual(0f, runtime.Objectives[1].Condition.CurrentCount);

        QuestEventBus.PublishMonsterKilled("second");
        Assert.AreEqual(1f, runtime.Objectives[1].Condition.CurrentCount);
        Assert.AreEqual(QuestState.Completed, runtime.State);

        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(data);
    }

    private static QuestObjectiveData CreateKillObjective(string monsterId)
    {
        return new QuestObjectiveData
        {
            Condition = new KillCondition { MonsterId = monsterId, TargetCount = 1 }
        };
    }

    /// <summary>
    /// 验证奖励通过注入服务发放
    /// </summary>
    [Test]
    public void Reward_GiveCallsRewardService()
    {
        TestRewardService service = new TestRewardService();
        bool succeeded = new GoldReward { Gold = 50 }.Give(service);

        Assert.IsTrue(succeeded);
        Assert.AreEqual(50, service.Gold);
    }

    /// <summary>
    /// 验证管理器入口和事件总线使用同一条事件分发链
    /// </summary>
    [Test]
    public void ReportProgress_UsesQuestEventBus()
    {
        GameObject gameObject = new GameObject("QuestSystemTestManager");
        QuestManager manager = gameObject.AddComponent<QuestManager>();
        manager.ReportProgress(new QuestProgressEvent(QuestProgressEventType.Kill, "slime"));

        Assert.AreEqual(1, progressCount);
        Assert.AreEqual(1, killCount);
        Object.DestroyImmediate(gameObject);
    }

    private void OnMonsterKilled(string value)
    {
        killCount++;
    }

    private void OnProgressReported(QuestProgressEvent value)
    {
        progressCount++;
    }

    private sealed class TestRewardService : IQuestRewardService
    {
        public int Gold { get; private set; }

        public bool TryGrantMoney(int amount) => false;
        public bool TryGrantGold(int amount)
        {
            Gold += amount;
            return true;
        }

        public bool TryGrantExp(int amount) => false;
        public bool TryGrantItem(string itemId, int count) => false;
        public bool TryGrantCustom(string rewardKey, int count) => false;
    }

    private sealed class TestItemService : IQuestItemService
    {
        private readonly int count;

        public TestItemService(int count)
        {
            this.count = count;
        }

        public float GetItemCount(int itemId) => count;
        public bool HasItem(int itemId, float amount) => count >= amount;
        public bool TryConsume(int itemId, float amount) => count >= amount;
    }
}
