using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using GameMain;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YangTools;
using EventInfo = YangTools.EventInfo;
using Object = UnityEngine.Object;

/// <summary>
/// 验证真实事件管理器及直接调用方的注册和派发行为
/// </summary>
public sealed class YangEventTests
{
    /// <summary>
    /// 独立管理器 避免核心测试影响单例
    /// </summary>
    private YangEventManager manager;
    /// <summary>
    /// 本用例创建的 Unity 对象
    /// </summary>
    private readonly List<Object> objects = new List<Object>();
    /// <summary>
    /// 本用例持有的分组
    /// </summary>
    private readonly List<YangEventGroup> groups = new List<YangEventGroup>();

    #region 注册与释放

    /// <summary>
    /// 创建独立管理器
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        manager = new YangEventManager();
    }

    /// <summary>
    /// 清理本用例的监听及临时对象
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        foreach (YangEventGroup group in groups) group.Dispose();
        groups.Clear();
        manager.Clear();
        foreach (Object item in objects) if (item) Object.DestroyImmediate(item);
        objects.Clear();
    }

    /// <summary>
    /// 全局监听支持空参数且空对象注销不会删除全局监听
    /// </summary>
    [Test]
    public void GlobalListener_AcceptsNullPayloadAndSurvivesNullHolderRemoval()
    {
        int count = 0;
        manager.Add(new EventInfo(null, "event", data => { Assert.IsNull(data.Args); count++; }));
        manager.Remove((Object)null);
        manager.Send("event", (object)null);
        Assert.AreEqual(1, count);
    }

    /// <summary>
    /// 对象注销删除全部类型与优先级的监听并保留其他对象
    /// </summary>
    [Test]
    public void RemoveHolder_RemovesEveryMatchingListener()
    {
        GameObject holder = Root();
        GameObject other = Root();
        int removed = 0;
        int kept = 0;
        manager.Add(new EventInfo(holder, "first", _ => removed++, -1));
        manager.Add(new EventInfo(holder, "first", _ => removed++, 1));
        manager.Add(new EventInfo(holder, "second", _ => removed++));
        manager.Add(new EventInfo(other, "first", _ => kept++));
        manager.Remove(holder);
        manager.Send("first", (object)null);
        manager.Send("second", (object)null);
        Assert.AreEqual(0, removed);
        Assert.AreEqual(1, kept);
    }

    /// <summary>
    /// 重复句柄仅注册一次且移除后可以重新添加
    /// </summary>
    [Test]
    public void DuplicateHandle_IsIdempotentAndCanBeRegisteredAgain()
    {
        int count = 0;
        EventInfo listener = new EventInfo(null, "event", _ => count++);
        manager.Add(listener);
        manager.Add(listener);
        manager.Send("event", (object)null);
        Assert.AreEqual(1, count);
        manager.Remove(listener);
        manager.Remove(listener);
        manager.Add(listener);
        manager.Send("event", (object)null);
        Assert.AreEqual(2, count);
    }

    /// <summary>
    /// 不同分组相同事件不互相清理且本组重复回调不会重复执行
    /// </summary>
    [Test]
    public void GroupDispose_PreservesOtherGroupAndAllowsReuse()
    {
        YangEventGroup first = Group();
        YangEventGroup second = Group();
        int a = 0;
        int b = 0;
        Action<EventData> callback = _ => a++;
        first.AddListener<Message>(callback);
        first.AddListener<Message>(callback);
        second.AddListener<Message>(_ => b++);
        YangExtend.SendEvent<Message>(new Message());
        Assert.AreEqual(1, a);
        Assert.AreEqual(1, b);
        first.Dispose();
        first.Dispose();
        YangExtend.SendEvent<Message>(new Message());
        Assert.AreEqual(1, a);
        Assert.AreEqual(2, b);
        first.AddListener<Message>(callback);
        YangExtend.SendEvent<Message>(new Message());
        Assert.AreEqual(2, a);
        Assert.AreEqual(3, b);
    }

    /// <summary>
    /// 外部按键清理后分组相同回调可以恢复注册
    /// </summary>
    [Test]
    public void Group_CanRestoreRegistrationAfterExternalKeyRemoval()
    {
        YangEventGroup group = Group();
        int count = 0;
        Action<EventData> callback = _ => count++;
        group.AddListener<Message>(callback);
        YangEventManager.Instance.RemoveForKey(typeof(Message).FullName);
        group.AddListener<Message>(callback);
        YangExtend.SendEvent<Message>(new Message());
        Assert.AreEqual(1, count);
    }

    /// <summary>
    /// 已销毁对象不能成为全局监听并在派发时清理禁用记录
    /// </summary>
    [Test]
    public void DestroyedHolder_IsPrunedEvenWhenDisabled()
    {
        GameObject holder = Root();
        int count = 0;
        EventInfo listener = new EventInfo(holder, "event", _ => count++) { isEnabled = false };
        manager.Add(listener);
        Object.DestroyImmediate(holder);
        manager.Send("event", (object)null);
        Assert.AreEqual(0, count);
        Assert.AreEqual(0, EventKeyCount());
        EventInfo destroyed = new EventInfo(holder, "event", _ => count++);
        Assert.IsFalse(destroyed.CanUse);
        manager.Add(destroyed);
        manager.Remove(holder);
        Assert.AreEqual(0, EventKeyCount());
    }

    #endregion

    #region 派发顺序与变更

    /// <summary>
    /// 同优先级按顺序执行并正确处理禁用与重新启用
    /// </summary>
    [Test]
    public void Priority_AscendingAndStableAcrossEnableChanges()
    {
        List<int> order = new List<int>();
        manager.Add(new EventInfo(null, "event", _ => order.Add(3), 3));
        manager.Add(new EventInfo(null, "event", _ => order.Add(1), -1));
        EventInfo enabled = new EventInfo(null, "event", _ => order.Add(2), -1) { isEnabled = false };
        manager.Add(enabled);
        manager.Send("event", (object)null);
        CollectionAssert.AreEqual(new[] { 1, 3 }, order);
        order.Clear();
        enabled.isEnabled = true;
        manager.Send("event", (object)null);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, order);
    }

    /// <summary>
    /// 各注销入口立即使当前派发中未执行的监听失效
    /// </summary>
    [TestCase("handle")]
    [TestCase("holder")]
    [TestCase("key")]
    [TestCase("clear")]
    public void RemovalDuringSend_SkipsRemainingListeners(string mode)
    {
        GameObject holder = Root();
        int count = 0;
        EventInfo remaining = new EventInfo(holder, "event", _ => count++);
        manager.Add(new EventInfo(null, "event", _ =>
        {
            if (mode == "handle") manager.Remove(remaining);
            else if (mode == "holder") manager.Remove(holder);
            else if (mode == "key") manager.RemoveForKey("event");
            else manager.Clear();
        }));
        manager.Add(remaining);
        manager.Send("event", (object)null);
        Assert.AreEqual(0, count);
    }

    /// <summary>
    /// 新增及重新注册不进入外层快照但可被嵌套派发看到
    /// </summary>
    [TestCase(false)]
    [TestCase(true)]
    public void RegistrationDuringSend_UsesNewVersionAndNestedSnapshot(bool reRegister)
    {
        List<string> order = new List<string>();
        bool nested = false;
        EventInfo listener = new EventInfo(null, "event", data => order.Add((string)data.Args + "-listener"));
        manager.Add(new EventInfo(null, "event", data =>
        {
            order.Add((string)data.Args + "-first");
            if (nested) return;
            nested = true;
            if (reRegister) manager.Remove(listener);
            manager.Add(listener);
            manager.Send("event", "inner");
        }));
        if (reRegister) manager.Add(listener);
        manager.Send("event", "outer");
        CollectionAssert.AreEqual(new[] { "outer-first", "inner-first", "inner-listener" }, order);
        order.Clear();
        manager.Send("event", "next");
        CollectionAssert.AreEqual(new[] { "next-first", "next-listener" }, order);
    }

    /// <summary>
    /// 清空再添加原句柄也不会执行旧版本快照
    /// </summary>
    [Test]
    public void ClearAndReRegisterDuringSend_SkipsOldVersion()
    {
        int count = 0;
        EventInfo listener = new EventInfo(null, "event", _ => count++);
        manager.Add(new EventInfo(null, "event", _ => { manager.Clear(); manager.Add(listener); }));
        manager.Add(listener);
        manager.Send("event", (object)null);
        Assert.AreEqual(0, count);
        manager.Send("event", (object)null);
        Assert.AreEqual(1, count);
    }

    /// <summary>
    /// 自注销和嵌套其他事件不会破坏本轮剩余回调
    /// </summary>
    [Test]
    public void SelfRemovalAndNestedOtherEvent_PreserveDispatchOrder()
    {
        List<int> order = new List<int>();
        EventInfo self = null;
        self = new EventInfo(null, "event", _ => { order.Add(1); manager.Remove(self); manager.Send("other", (object)null); });
        manager.Add(self);
        manager.Add(new EventInfo(null, "event", _ => order.Add(3)));
        manager.Add(new EventInfo(null, "other", _ => order.Add(2)));
        manager.Send("event", (object)null);
        manager.Send("event", (object)null);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 3 }, order);
    }

    /// <summary>
    /// 异常保留原始信息并允许后续监听继续执行
    /// </summary>
    [Test]
    public void ThrowingCallback_IsLoggedAndDoesNotStopDispatch()
    {
        int count = 0;
        manager.Add(new EventInfo(null, "event", _ => throw new InvalidOperationException("regression")));
        manager.Add(new EventInfo(null, "event", _ => count++));
        LogAssert.Expect(LogType.Error, new Regex(@"事件回调异常 \[event\][\s\S]*InvalidOperationException: regression"));
        Assert.DoesNotThrow(() => manager.Send("event", (object)null));
        Assert.AreEqual(1, count);
    }

    /// <summary>
    /// 快照在监听不变时复用且每轮消息数据独立
    /// </summary>
    [Test]
    public void Snapshot_IsReusedButEventDataRemainsIndependent()
    {
        List<EventData> data = new List<EventData>();
        manager.Add(new EventInfo(null, "event", data.Add));
        manager.Send("event", "first");
        object first = Snapshot();
        manager.Send("event", "second");
        Assert.AreSame(first, Snapshot());
        Assert.AreNotSame(data[0], data[1]);
        Assert.AreEqual("first", data[0].Args);
        manager.Add(new EventInfo(null, "event", _ => { }));
        manager.Send("event", "third");
        Assert.AreNotSame(first, Snapshot());
    }

    /// <summary>
    /// 路由只匹配指定类型并兼容旧事件发送接口
    /// </summary>
    [Test]
    public void Routing_IsExactAndSupportsLegacyAndCustomKeys()
    {
        YangEventGroup group = Group();
        int derived = 0;
        int baseCount = 0;
        group.AddListener<BaseMessage>(_ => baseCount++);
        group.AddListener<DerivedMessage>(_ => derived++);
        new DerivedMessage().SendEvent();
        YangExtend.SendEvent<DerivedMessage>(new DerivedMessage());
        Assert.AreEqual(0, baseCount);
        Assert.AreEqual(2, derived);
        int custom = 0;
        string key = Guid.NewGuid().ToString("N");
        EventInfo listener = YangExtend.AddEventListener<Message>(null, _ => custom++, key);
        try
        {
            YangExtend.SendEvent<Message>(new Message(), key);
            Assert.AreEqual(1, custom);
        }
        finally { YangExtend.RemoveEventListener(listener); }
    }

    /// <summary>
    /// 非法注册参数在入口失败而不污染事件表
    /// </summary>
    [Test]
    public void InvalidArguments_AreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => manager.Add(null));
        Assert.Throws<ArgumentNullException>(() => new EventInfo(null, "event", null));
        Assert.Throws<ArgumentException>(() => new EventInfo(null, " ", _ => { }));
        Assert.Throws<ArgumentException>(() => manager.Send(" ", (object)null));
        Assert.Throws<ArgumentNullException>(() => YangExtend.SendEvent((Type)null, new object()));
        Assert.AreEqual(0, EventKeyCount());
    }

    #endregion

    #region 调用方集成

    /// <summary>
    /// 更新完成或中止仅释放本次操作 不删除相同事件的业务监听
    /// </summary>
    [TestCase(false)]
    [TestCase(true)]
    public void PatchOperation_CompletionAndAbortPreserveBusinessListeners(bool abort)
    {
        GameInit previous = GameInit.Instance;
        GameObject holder = Root();
        holder.SetActive(false);
        GameInit.Instance = holder.AddComponent<GameInit>();
        Type operationType = typeof(YangEventManager).Assembly.GetType("GameMain.PatchOperation", true);
        object operation = null;
        YangEventGroup business = Group();
        int count = 0;
        business.AddListener<UserTryInitialize>(_ => count++);
        try
        {
            Type playModeType = operationType.GetConstructors()[0].GetParameters()[2].ParameterType;
            operation = Activator.CreateInstance(operationType, "event_regression", "", Enum.Parse(playModeType, "EditorSimulateMode"));
            if (abort) Invoke(operation, "OnAbort");
            else
            {
                operationType.GetField("stepsType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(operation, StepsType.Update);
                object machine = operationType.GetField("machine", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(operation);
                machine.GetType().GetMethod("Run", new[] { typeof(string) }).Invoke(machine, new object[] { "GameMain.FsmLoadDone" });
                Invoke(operation, "OnUpdate");
            }
            new UserTryInitialize().SendEvent();
            Assert.AreEqual(1, count);
            Assert.AreEqual(0, ((ICollection)operationType.GetField("listeners", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(operation)).Count);
        }
        finally
        {
            if (operation != null) Invoke(operation, "OnAbort");
            GameInit.Instance = previous;
        }
    }

    /// <summary>
    /// 登录监听匹配具体类型且平台重复注销能安全释放监听
    /// </summary>
    [Test]
    public void Login_ReceivesConcreteEventAndUnInitializeUnsubscribes()
    {
        GameObject holder = Root();
        holder.SetActive(false);
        Type platformType = typeof(YangEventManager).Assembly.GetType("GameMain.PlatformMgr", true);
        Component platform = holder.AddComponent(platformType);
        platformType.GetProperty("Platform").SetValue(platform, new PlatformDefault { OpenId = "regression_user" });
        YangEventGroup group = (YangEventGroup)platformType.GetProperty("EventGroup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(platform);
        groups.Add(group);
        FieldInfo login = platformType.GetField("IsLoginSuccess");
        bool previousLogin = (bool)login.GetValue(null);
        string previousId = (string)platformType.GetProperty("UserId").GetValue(null);
        string previousName = (string)platformType.GetProperty("NickName").GetValue(null);
        string previousIcon = (string)platformType.GetProperty("IconUrl").GetValue(null);
        try
        {
            login.SetValue(null, false);
            Invoke(platform, "Login", new object[] { null });
            new GetOpenIdSuccess { openId = "regression_user" }.SendEvent();
            Assert.IsTrue((bool)login.GetValue(null));
            Assert.AreEqual("regression_user".GetHashCode().ToString(), platformType.GetProperty("UserId").GetValue(null));
            Invoke(platform, "UnInitialize");
            Invoke(platform, "UnInitialize");
            login.SetValue(null, false);
            new GetOpenIdSuccess().SendEvent();
            Assert.IsFalse((bool)login.GetValue(null));
        }
        finally
        {
            group.Dispose();
            login.SetValue(null, previousLogin);
            platformType.GetProperty("UserId").SetValue(null, previousId);
            platformType.GetProperty("NickName").SetValue(null, previousName);
            platformType.GetProperty("IconUrl").SetValue(null, previousIcon);
        }
    }

#if YANGTOOLS_QUEST_INTEGRATION
    /// <summary>
    /// 任务桥接具体进度类型且释放后不误删其他业务监听
    /// </summary>
    [Test]
    public void QuestBridge_ForwardsProgressAndDisposesOnlyOwnListeners()
    {
        GameObject holder = Root();
        holder.SetActive(false);
        QuestManager quests = holder.AddComponent<QuestManager>();
        QuestData config = ScriptableObject.CreateInstance<QuestData>();
        objects.Add(config);
        config.Id = "yang_event_bridge_regression";
        config.DefaultActive = true;
        config.Objectives.Add(new QuestObjectiveData { Condition = new KillCondition { MonsterId = "slime", TargetCount = 2 } });
        quests.Initialize(new[] { config });
        var previous = QuestEventMessageBase.Sender;
        YangEventGroup business = Group();
        int count = 0;
        business.AddListener<QuestProgressEvent>(_ => count++);
        YangQuestEventBridge bridge = new YangQuestEventBridge(quests);
        try
        {
            new QuestProgressEvent(QuestProgressEventType.Kill, "slime").SendEvent();
            Assert.AreEqual(1f, quests.GetQuest(config.Id).Objectives[0].Condition.CurrentCount);
            bridge.Dispose();
            new QuestProgressEvent(QuestProgressEventType.Kill, "slime").SendEvent();
            Assert.AreEqual(1f, quests.GetQuest(config.Id).Objectives[0].Condition.CurrentCount);
            YangExtend.SendEvent<QuestProgressEvent>(new QuestProgressEvent(QuestProgressEventType.Kill, "slime"));
            Assert.AreEqual(2, count);
            Assert.AreEqual(1f, quests.GetQuest(config.Id).Objectives[0].Condition.CurrentCount);
        }
        finally
        {
            bridge.Dispose();
            QuestEventMessageBase.Sender = previous;
            quests.Shutdown();
        }
    }
#endif

    #endregion

    #region 测试辅助

    /// <summary>
    /// 创建本用例负责清理的对象
    /// </summary>
    private GameObject Root()
    {
        GameObject item = new GameObject("YangEventRegression");
        objects.Add(item);
        return item;
    }

    /// <summary>
    /// 创建本用例负责释放的分组
    /// </summary>
    private YangEventGroup Group()
    {
        YangEventGroup group = new YangEventGroup();
        groups.Add(group);
        return group;
    }

    /// <summary>
    /// 读取事件表以验证空容器已清理
    /// </summary>
    private int EventKeyCount()
    {
        return ((IDictionary)typeof(YangEventManager).GetField("eventDic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager)).Count;
    }

    /// <summary>
    /// 读取实际快照以验证缓存复用
    /// </summary>
    private object Snapshot()
    {
        IDictionary events = (IDictionary)typeof(YangEventManager).GetField("eventDic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
        object bucket = events["event"];
        return bucket.GetType().GetProperty("Snapshot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bucket);
    }

    /// <summary>
    /// 调用受保护的操作生命周期入口
    /// </summary>
    private static void Invoke(object target, string method, params object[] args)
    {
        try
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(target, args);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException;
        }
    }

    /// <summary>
    /// 无基类事件
    /// </summary>
    private sealed class Message { }
    /// <summary>
    /// 精确路由测试基类
    /// </summary>
    private class BaseMessage : EventMessageBase { }
    /// <summary>
    /// 精确路由测试派生类
    /// </summary>
    private sealed class DerivedMessage : BaseMessage { }

    #endregion
}
