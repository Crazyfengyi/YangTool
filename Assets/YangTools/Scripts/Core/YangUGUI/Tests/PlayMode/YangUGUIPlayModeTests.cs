using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// 使用真实 GameObject 和框架生命周期验证 UGUI 的失败回滚和复用
/// </summary>
public sealed class YangUGUIPlayModeTests
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private readonly List<GameObject> objects = new(); //测试创建的对象
    private readonly Dictionary<string, Object> previousAssets = new(); //临时资源地址的旧缓存
    private object manager; //被测管理器
    private IDictionary assets; //真实资源缓存
    private Type panelType; //真实测试页面类型
    private string prefix; //本次资源地址前缀
    private float originalTimeScale; //原时间缩放

    /// <summary>
    /// 建立独立管理器并复用实际资源缓存入口
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        prefix = "__UGUIRegression_" + Guid.NewGuid().ToString("N");
        panelType = RuntimeType("YangTools.Scripts.Core.YangUGUI.YangUGUITestPanel");
        ClearHooks();
        manager = Activator.CreateInstance(RuntimeType("YangTools.Scripts.Core.YangUGUI.YangUIManager"));
        Call(manager, "InitModule");
        Component helper = Root("CreateHelper").AddComponent(RuntimeType("YangTools.Scripts.Core.YangUGUI.UGUIPanelCreateHelper")); //真实辅助器
        Call(manager, "SetUIPanelHelper", helper);
        assets = (IDictionary)RuntimeType("YangTools.Scripts.Core.ResourceManager.ResourceManager").GetField("AssetCacheDict", Flags).GetValue(null);
        AddGroup("test", 0);
    }

    /// <summary>
    /// 清理所有测试对象并恢复资源缓存和全局时间
    /// </summary>
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        ClearHooks();
        if (manager != null) Call(manager, "CloseModule");
        foreach (var entry in previousAssets)
        {
            if (entry.Value) assets[entry.Key] = entry.Value;
            else assets.Remove(entry.Key);
        }
        previousAssets.Clear();
        foreach (GameObject item in objects)
        {
            if (item) Object.Destroy(item);
        }
        objects.Clear();
        Time.timeScale = originalTimeScale;
        yield return null;
    }

    /// <summary>
    /// 缺少逻辑组件及初始化和打开异常均不得留下注册或借出实例
    /// </summary>
    [UnityTest]
    public IEnumerator FailedOpen_RollsBackRegistrationAndPoolCounts()
    {
        int failed = 0; //失败事件数
        int succeeded = 0; //成功事件数
        Subscribe("OpenUIPanelFailure", _ => failed++);
        Subscribe("OpenUIPanelSuccess", _ => succeeded++);
        for (int stage = 0; stage < 3; stage++)
        {
            string location = Cache(Prefab("Failure" + stage, stage == 0 ? null : panelType)); //本次资源
            if (stage == 1) Hook("Initialized", _ => throw new InvalidOperationException("init failed"));
            if (stage == 2) Hook("Opened", _ => throw new InvalidOperationException("open failed"));
            Task task = Open(location); //打开任务
            yield return Wait(task);
            Assert.IsTrue(task.IsFaulted);
            Assert.AreEqual(0, ((Array)Call(manager, "GetAllLoadedPanels")).Length);
            Assert.AreEqual(0, PoolCount("AllCount"));
            Assert.AreEqual(0, PoolCount("InactiveCount"));
            ClearHooks();
            yield return null;
        }
        Assert.AreEqual(3, failed);
        Assert.AreEqual(0, succeeded);
    }

    /// <summary>
    /// 创建期间关闭管理器使等待后的请求取消而非报告失败
    /// </summary>
    [UnityTest]
    public IEnumerator ShutdownDuringCreation_CancelsAndDestroysLease()
    {
        string location = Cache(Prefab("Shutdown", panelType)); //测试资源
        int events = 0; //打开事件数
        Subscribe("OpenUIPanelFailure", _ => events++);
        Subscribe("OpenUIPanelSuccess", _ => events++);
        Hook("Created", _ => Call(manager, "CloseModule"));
        Task task = Open(location); //被取消的任务
        yield return Wait(task);
        Assert.Throws<OperationCanceledException>(() => task.GetAwaiter().GetResult());
        Assert.AreEqual(0, events);
        Assert.AreEqual(0, PoolCount("AllCount"));
        Assert.AreEqual(0, Property<int>(manager, "UIGroupCount"));
    }

    /// <summary>
    /// 打开回调内关闭自身使用已绑定句柄且不发送成功事件
    /// </summary>
    [UnityTest]
    public IEnumerator CloseInsideOpen_CancelsWithoutStaleRecycleEntry()
    {
        string location = Cache(Prefab("CloseInsideOpen", panelType)); //测试资源
        int events = 0; //打开事件数
        Subscribe("OpenUIPanelSuccess", _ => events++);
        Subscribe("OpenUIPanelFailure", _ => events++);
        Hook("Opened", panel => Call(panel, "CloseSelfPanel"));
        Task task = Open(location); //被取消的任务
        yield return Wait(task);
        Assert.Throws<OperationCanceledException>(() => task.GetAwaiter().GetResult());
        Tick();
        Assert.AreEqual(0, events);
        Assert.AreEqual(0, PoolCount("AllCount"));
        Assert.AreEqual(0, PoolCount("InactiveCount"));
    }

    /// <summary>
    /// 打开回调关闭并重开同一实例时旧请求不得回滚新的借出版本
    /// </summary>
    [UnityTest]
    public IEnumerator ReopenInsideOpen_ProtectsNewLeaseFromOldRollback()
    {
        string location = Cache(Prefab("ReentrantReuse", panelType)); //复用资源
        bool first = true; //是否为旧请求
        Task reopened = null; //新请求
        Hook("Opened", panel =>
        {
            if (!first) return;
            first = false;
            Call(panel, "CloseSelfPanel");
            reopened = Open(location);
        });
        Task original = Open(location); //原请求
        yield return Wait(original);
        yield return Wait(reopened);
        Assert.Throws<OperationCanceledException>(() => original.GetAwaiter().GetResult());
        var result = Result(reopened);
        Assert.AreEqual(1, ((Array)Call(manager, "GetAllLoadedPanels")).Length);
        Assert.AreEqual(1, PoolCount("AllCount"));
        Assert.IsTrue(result.panel.gameObject.activeSelf);
        Component framework = result.panel.GetComponent(RuntimeType("YangTools.Scripts.Core.YangUGUI.UIPanel")); //当前框架组件
        Assert.AreEqual(result.id, Property<int>(framework, "SerialId"));
    }

    /// <summary>
    /// 监听器异常不会阻断后续订阅者且重复关闭和重新打开正确复用
    /// </summary>
    [UnityTest]
    public IEnumerator ListenerExceptions_KeepCommittedStateAndAllowReuse()
    {
        string location = Cache(Prefab("Reuse", panelType)); //测试资源
        int opened = 0; //成功事件数
        int closed = 0; //关闭事件数
        Subscribe("OpenUIPanelSuccess", _ => throw new InvalidOperationException("subscriber failed"));
        Subscribe("OpenUIPanelSuccess", _ => opened++);
        Subscribe("CloseUIPanelComplete", _ => throw new InvalidOperationException("close subscriber failed"));
        Subscribe("CloseUIPanelComplete", _ => closed++);
        LogAssert.Expect(LogType.Exception, new Regex("subscriber failed"));
        Task firstTask = Open(location); //首次打开
        yield return Wait(firstTask);
        var first = Result(firstTask); //首次结果
        LogAssert.Expect(LogType.Exception, new Regex("close subscriber failed"));
        Close(first.id);
        Close(first.id);
        Tick();
        Assert.AreEqual(1, closed);
        Assert.AreEqual(1, PoolCount("InactiveCount"));
        Assert.IsFalse(first.panel.gameObject.activeSelf);
        Assert.IsNull(Property<object>(first.panel.GetComponent(RuntimeType("YangTools.Scripts.Core.YangUGUI.UIPanel")), "Handle"));
        LogAssert.Expect(LogType.Exception, new Regex("subscriber failed"));
        Task secondTask = Open(location); //再次打开
        yield return Wait(secondTask);
        var second = Result(secondTask); //再次结果
        Assert.AreSame(first.panel, second.panel);
        Assert.AreNotEqual(first.id, second.id);
        Assert.IsTrue(second.panel.gameObject.activeSelf);
        Assert.AreEqual(2, opened);
        LogAssert.Expect(LogType.Exception, new Regex("close subscriber failed"));
        Call(manager, "CloseModule");
        yield return null;
        Assert.IsFalse(second.panel);
        Assert.AreEqual(0, PoolCount("AllCount"));
    }

    /// <summary>
    /// 多组和复用页面保持分层关系并捕获动态 Canvas 的原始顺序
    /// </summary>
    [UnityTest]
    public IEnumerator Sorting_SeparatesGroupsPanelsAndLocalCanvasRanks()
    {
        AddGroup("high", 1);
        string location = Cache(Prefab("Sorting", panelType, true)); //包含非零原始排序的资源
        Task highTask = Open(location, "high");
        yield return Wait(highTask);
        Task lowTask = Open(location);
        yield return Wait(lowTask);
        Task topTask = Open(location);
        yield return Wait(topTask);
        var high = Result(highTask);
        var low = Result(lowTask);
        var top = Result(topTask);
        Assert.Less(MaxOrder(low.panel), MinOrder(top.panel));
        Assert.Less(MaxOrder(top.panel), MinOrder(high.panel));
        Canvas root = top.panel.GetComponent<Canvas>(); //页面根 Canvas
        Canvas below = top.panel.transform.Find("Below").GetComponent<Canvas>(); //低于根的 Canvas
        Canvas above = top.panel.transform.Find("Above").GetComponent<Canvas>(); //高于根的 Canvas
        Canvas equal = top.panel.transform.Find("Equal").GetComponent<Canvas>(); //相同原始排序的 Canvas
        Assert.Less(below.sortingOrder, root.sortingOrder);
        Assert.Greater(above.sortingOrder, root.sortingOrder);
        Assert.AreEqual(above.sortingOrder, equal.sortingOrder);
        Canvas dynamicCanvas = Child(top.panel.transform, "Dynamic").AddComponent<Canvas>(); //动态 Canvas
        dynamicCanvas.overrideSorting = true;
        dynamicCanvas.sortingOrder = -10;
        Call(manager, "RefreshGroups");
        Assert.Less(dynamicCanvas.sortingOrder, below.sortingOrder);
        object highGroup = Call(manager, "GetGroup", "high"); //高组
        highGroup.GetType().GetProperty("Depth").SetValue(highGroup, -1);
        Assert.Less(MaxOrder(high.panel), MinOrder(low.panel));
        Close(top.id);
        Tick();
        Task reusedTask = Open(location);
        yield return Wait(reusedTask);
        Assert.AreSame(top.panel, Result(reusedTask).panel);
        Assert.Less(dynamicCanvas.sortingOrder, below.sortingOrder);
    }

    /// <summary>
    /// 使用较小内部容量验证真实打开失败不会部分覆盖旧排序
    /// </summary>
    [UnityTest]
    public IEnumerator SortingOverflow_LeavesExistingLayoutAndDepthUnchanged()
    {
        object layout = Activator.CreateInstance(RuntimeType("YangTools.Scripts.Core.YangUGUI.UISortingLayout"), Flags,
            null, new object[] { 3 }, null); //受限布局
        manager.GetType().GetField("sortingLayout", Flags).SetValue(manager, layout);
        string location = Cache(Prefab("Capacity", panelType, true)); //占满内部容量的资源
        Task firstTask = Open(location);
        yield return Wait(firstTask);
        var first = Result(firstTask);
        int originalOrder = first.panel.GetComponent<Canvas>().sortingOrder; //已提交排序
        Task overflow = Open(location);
        yield return Wait(overflow);
        Assert.IsTrue(overflow.IsFaulted);
        Assert.AreEqual(originalOrder, first.panel.GetComponent<Canvas>().sortingOrder);
        Assert.AreEqual(1, PoolCount("AllCount"));
        Assert.AreEqual(1, ((Array)Call(manager, "GetAllLoadedPanels")).Length);
        Canvas extra = Child(first.panel.transform, "Overflow").AddComponent<Canvas>(); //导致布局变更超限的 Canvas
        extra.overrideSorting = true;
        extra.sortingOrder = 99;
        object group = Call(manager, "GetGroup", "test"); //被修改的组
        Assert.Throws<TargetInvocationException>(() => group.GetType().GetProperty("Depth").SetValue(group, 100));
        Assert.AreEqual(0, Property<int>(group, "Depth"));
        Assert.AreEqual(originalOrder, first.panel.GetComponent<Canvas>().sortingOrder);
    }

    /// <summary>
    /// 暂停游戏时动画仍完成且旧关闭动画不会关闭复用后的页面
    /// </summary>
    [UnityTest]
    public IEnumerator Animation_PauseAndReuseDoNotRunOldCloseCallbacks()
    {
        GameObject prefab = Prefab("Animation", panelType); //动画资源
        Child(prefab.transform, "WindowRoot").transform.localPosition = new Vector3(3f, 4f, 0f);
        Child(prefab.transform, "bgMask").AddComponent<CanvasGroup>();
        Component logic = prefab.GetComponent(panelType); //资源逻辑
        Field(logic, "needOpenAni", true);
        Field(logic, "needCloseAni", true);
        Field(logic, "aniTime", 0.05f);
        Field(logic, "aniTime2", 0.1f);
        string location = Cache(prefab); //测试地址
        int closed = 0; //关闭事件数
        Subscribe("CloseUIPanelComplete", _ => closed++);
        Time.timeScale = 0f;
        Task firstTask = Open(location);
        yield return Wait(firstTask);
        var first = Result(firstTask);
        yield return WaitUntil(() => first.panel.transform.Find("WindowRoot").localPosition == new Vector3(3f, 4f, 0f));
        Call(first.panel, "CloseSelfPanel");
        Call(first.panel, "CloseSelfPanel");
        yield return WaitUntil(() => closed == 1);
        Tick();
        Task secondTask = Open(location);
        yield return Wait(secondTask);
        var second = Result(secondTask);
        Call(second.panel, "CloseSelfPanel");
        Close(second.id);
        Tick();
        Task thirdTask = Open(location);
        yield return Wait(thirdTask);
        var third = Result(thirdTask);
        float until = Time.realtimeSinceStartup + 0.2f; //跨越旧关闭动画完成时间
        while (Time.realtimeSinceStartup < until) yield return null;
        Assert.AreEqual(2, closed);
        Assert.AreSame(first.panel, third.panel);
        Assert.IsTrue(Property<bool>(third.panel, "Available"));
        Assert.IsTrue(third.panel.gameObject.activeSelf);
        Assert.AreEqual(1f, third.panel.transform.Find("bgMask").GetComponent<CanvasGroup>().alpha);
    }

    /// <summary>
    /// 缺少动画节点时直接关闭并正常回收
    /// </summary>
    [UnityTest]
    public IEnumerator MissingAnimationNodes_FallsBackToImmediateClose()
    {
        GameObject prefab = Prefab("MissingAnimation", panelType); //缺少动画节点的资源
        Field(prefab.GetComponent(panelType), "needCloseAni", true);
        Task task = Open(Cache(prefab));
        yield return Wait(task);
        LogAssert.Expect(LogType.Warning, new Regex("UI动画节点不完整"));
        Call(Result(task).panel, "CloseSelfPanel");
        Tick();
        Assert.AreEqual(0, ((Array)Call(manager, "GetAllLoadedPanels")).Length);
        Assert.AreEqual(1, PoolCount("InactiveCount"));
    }

    /// <summary>
    /// 确认弹窗复用时刷新文案和回调并只执行一次自动选择
    /// </summary>
    [UnityTest]
    public IEnumerator Confirmation_ReusesInstanceWithFreshDataAndSingleCallback()
    {
        GameObject prefab = ConfirmationPrefab(); //确认弹窗资源
        string location = Cache(prefab);
        int confirmed = 0; //确认次数
        int cancelled = 0; //取消次数
        Task firstTask = Open(location, "test", ConfirmationData("first", () => confirmed++, null));
        yield return Wait(firstTask);
        var first = Result(firstTask);
        Call(first.panel, "OK_OnClick");
        Call(first.panel, "OK_OnClick");
        Tick();
        Assert.AreEqual(1, confirmed);
        object data = ConfirmationData("second", null, () => cancelled++); //新数据
        Field(data, "isCountDownSelect", true);
        Field(data, "countDownTime", 0.01f);
        Field(data, "autoSelectOk", false);
        Task secondTask = Open(location, "test", data);
        yield return Wait(secondTask);
        var second = Result(secondTask);
        Assert.AreSame(first.panel, second.panel);
        Assert.AreEqual("second", ((TMP_Text)second.panel.GetType().GetField("text").GetValue(second.panel)).text);
        Component framework = second.panel.GetComponent(RuntimeType("YangTools.Scripts.Core.YangUGUI.UIPanel")); //框架组件
        Call(framework, "OnUpdate", 0f, 0.02f);
        Call(framework, "OnUpdate", 0f, 0.02f);
        Assert.AreEqual(1, cancelled);
        Assert.AreEqual(1, confirmed);
        Tick();
        Assert.IsNull(second.panel.GetType().GetField("confirmData", Flags).GetValue(second.panel));
    }

    /// <summary>
    /// 真实 Start 完成前打开等待且泛型查询和两种关闭入口正确工作
    /// </summary>
    [UnityTest]
    public IEnumerator EntryPoint_WaitsForStartAndSupportsSafeQueriesAndGenericClose()
    {
        Type entryType = RuntimeType("YangTools.Scripts.Core.YangUGUI.UIMonoInstance"); //入口类型
        object group = Enum.Parse(RuntimeType("YangTools.Scripts.Core.YangUGUI.GroupType"), "Top"); //打开组
        AddGroup(group.ToString(), 2);
        string location = Cache(Prefab("EntryPoint", panelType)); //自定义地址
        Component entry = Root("UIEntry").AddComponent(entryType); //真实入口
        Field(entry, "uiManager", manager);
        Task task = ToTask(Call(entry, "OpenPanel", location, group, 0, false, null)); //等待 Start 的打开请求
        Assert.IsFalse(task.IsCompleted);
        yield return Wait(task);
        var opened = Result(task);
        MethodInfo query = GenericMethod(entryType, "GetUIPanel", false, 1, typeof(string)); //字符串查询
        Assert.IsNull(query.MakeGenericMethod(panelType).Invoke(entry, new object[] { "missing" }));
        Assert.AreSame(opened.panel, query.MakeGenericMethod(panelType).Invoke(entry, new object[] { location }));
        Assert.IsNull(query.MakeGenericMethod(RuntimeType("CommonConfirmPanel")).Invoke(entry, new object[] { location }));
        GenericMethod(entryType, "ClosePanel", true, 1, typeof(string)).MakeGenericMethod(panelType)
            .Invoke(null, new object[] { location });
        Tick();
        Assert.IsFalse(opened.panel.gameObject.activeSelf);

        Cache(Prefab("DefaultAddress", panelType), panelType.Name);
        Task defaultTask = ToTask(Call(entry, "OpenPanel", panelType.Name, group, 0, false, null));
        yield return Wait(defaultTask);
        GenericMethod(entryType, "ClosePanel", true, 0, null).MakeGenericMethod(panelType).Invoke(null, null);
        Tick();
        Assert.IsFalse(Result(defaultTask).panel.gameObject.activeSelf);

        LogAssert.Expect(LogType.Error, "UIMonoInstance有重复");
        Component duplicate = Root("DuplicateUIEntry").AddComponent(entryType); //重复入口
        yield return null;
        Assert.IsFalse(duplicate);
        Assert.AreSame(entry, entryType.GetProperty("Instance", Flags).GetValue(null));
        Object.Destroy(entry.gameObject);
        yield return null;
        Assert.IsNull(entryType.GetProperty("Instance", Flags).GetValue(null));
    }

    /// <summary>
    /// 初始化等待期间的连续业务请求共享页面且加载显示以最后请求为准
    /// </summary>
    [UnityTest]
    public IEnumerator CommonTool_CoalescesPendingOpensAndUsesLatestLoadingState()
    {
        Type entryType = RuntimeType("YangTools.Scripts.Core.YangUGUI.UIMonoInstance"); //入口类型
        object group = Enum.Parse(RuntimeType("YangTools.Scripts.Core.YangUGUI.GroupType"), "顶部"); //业务组
        AddGroup(group.ToString(), 2);
        Component entry = Root("CommonUIEntry").AddComponent(entryType); //初始化尚未完成的入口
        Field(entry, "uiManager", manager);

        GameObject tipPrefab = Prefab("Tips", RuntimeType("TipsPanel")); //提示页面
        GameObject itemPrefab = Root("TipItem"); //提示物品原型
        itemPrefab.AddComponent<TextMeshProUGUI>();
        itemPrefab.SetActive(false);
        Field(tipPrefab.GetComponent(RuntimeType("TipsPanel")), "prefab", itemPrefab);
        Cache(tipPrefab, "TipsPanel");
        GameObject loadingPrefab = Prefab("Loading", RuntimeType("LoadingPanel")); //加载页面
        Field(loadingPrefab.GetComponent(RuntimeType("LoadingPanel")), "loading", Child(loadingPrefab.transform, "LoadingContent"));
        Cache(loadingPrefab, "LoadingPanel");
        Cache(ConfirmationPrefab(), "CommonConfirmPanel");
        Component input = Root("Input").AddComponent(RuntimeType("GameInputManager")); //真实输入组件
        object tool = Activator.CreateInstance(RuntimeType("UICommonTool")); //业务辅助器
        Call(tool, "ShowTip", "first");
        Call(tool, "ShowTip", "second");
        Call(tool, "SetLoadingShow", true);
        Call(tool, "SetLoadingShow", false);
        Call(tool, "ShowConfirmPanel", "confirmation", "OK", "Cancel", (Action)(() => { }), null, false, 1f, true);
        yield return WaitUntil(() => ((Array)Call(manager, "GetAllLoadedPanels")).Length == 3);
        yield return null;
        Component tips = Logic("TipsPanel"); //实际提示页面
        Component loading = Logic("LoadingPanel"); //实际加载页面
        Component confirm = Logic("CommonConfirmPanel"); //实际弹窗
        Assert.AreEqual(1, ((Array)Call(manager, "GetPanels", "TipsPanel")).Length);
        Assert.AreEqual(2, tips.transform.childCount);
        Assert.IsFalse(((GameObject)loading.GetType().GetField("loading").GetValue(loading)).activeSelf);
        Assert.AreEqual("confirmation", ((TMP_Text)confirm.GetType().GetField("text").GetValue(confirm)).text);
        // 等待业务层自行创建的提示序列结束 避免测试销毁对象后污染其他用例
        yield return WaitUntil(() => tips.transform.childCount == 0);
        ((IDisposable)Property<object>(input, "GameInput")).Dispose();
    }

    #region 测试辅助

    /// <summary>
    /// 从项目实际加载的程序集中查找运行时类型
    /// </summary>
    private static Type RuntimeType(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(name, false); //实际已加载类型
            if (type != null) return type;
        }
        throw new TypeLoadException(name);
    }

    /// <summary>
    /// 按参数类型选择框架方法并保留原始异常
    /// </summary>
    private static object Call(object target, string name, params object[] args)
    {
        foreach (MethodInfo method in target.GetType().GetMethods(Flags))
        {
            if (method.Name != name || method.IsGenericMethod) continue;
            ParameterInfo[] parameters = method.GetParameters(); //方法参数
            if (parameters.Length != args.Length) continue;
            bool matches = true; //参数是否匹配
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null && !parameters[i].ParameterType.IsInstanceOfType(args[i])) matches = false;
            }
            if (!matches) continue;
            try { return method.Invoke(target, args); }
            catch (TargetInvocationException exception) { throw exception.InnerException; }
        }
        throw new MissingMethodException(target.GetType().FullName, name);
    }

    /// <summary>
    /// 读取公开属性
    /// </summary>
    private static T Property<T>(object target, string name)
    {
        return (T)target.GetType().GetProperty(name, Flags).GetValue(target);
    }

    /// <summary>
    /// 选择兼容入口的泛型重载
    /// </summary>
    private static MethodInfo GenericMethod(Type type, string name, bool isStatic, int count, Type parameterType)
    {
        foreach (MethodInfo method in type.GetMethods(Flags))
        {
            if (method.Name != name || !method.IsGenericMethodDefinition || method.IsStatic != isStatic) continue;
            ParameterInfo[] parameters = method.GetParameters(); //重载参数
            if (parameters.Length == count && (count == 0 || parameters[0].ParameterType == parameterType)) return method;
        }
        throw new MissingMethodException(type.FullName, name);
    }

    /// <summary>
    /// 从实际已加载页面获取逻辑组件
    /// </summary>
    private Component Logic(string location)
    {
        return (Component)Property<object>(Call(manager, "GetPanel", location), "UGUIPanel");
    }

    /// <summary>
    /// 设置运行时字段
    /// </summary>
    private static void Field(object target, string name, object value)
    {
        target.GetType().GetField(name, Flags).SetValue(target, value);
    }

    /// <summary>
    /// 创建由本用例负责销毁的根对象
    /// </summary>
    private GameObject Root(string name)
    {
        GameObject item = new GameObject(prefix + name, typeof(RectTransform)); //测试对象
        objects.Add(item);
        return item;
    }

    /// <summary>
    /// 创建真实子节点
    /// </summary>
    private GameObject Child(Transform parent, string name)
    {
        GameObject child = new GameObject(name, typeof(RectTransform)); //测试节点
        child.transform.SetParent(parent, false);
        return child;
    }

    /// <summary>
    /// 添加真实 UGUI 组辅助器
    /// </summary>
    private void AddGroup(string name, int depth)
    {
        Component helper = Root(name).AddComponent(RuntimeType("YangTools.Scripts.Core.YangUGUI.UGUIGroupHelper")); //组辅助器
        Call(manager, "AddGroup", name, depth, helper);
    }

    /// <summary>
    /// 创建可由真实资源加载入口获取的界面原型
    /// </summary>
    private GameObject Prefab(string name, Type type, bool nestedCanvases = false)
    {
        GameObject prefab = Root(name); //原型对象
        Canvas canvas = prefab.AddComponent<Canvas>(); //根 Canvas
        canvas.sortingOrder = 25;
        if (type != null) prefab.AddComponent(type);
        if (nestedCanvases)
        {
            foreach (string childName in new[] { "Below", "Above", "Equal" })
            {
                Canvas child = Child(prefab.transform, childName).AddComponent<Canvas>(); //子 Canvas
                child.overrideSorting = true;
                child.sortingOrder = childName == "Below" ? 10 : 40;
            }
        }
        return prefab;
    }

    /// <summary>
    /// 临时缓存原型并在用例结束时恢复原值
    /// </summary>
    private string Cache(GameObject prefab, string location = null)
    {
        location ??= prefab.name;
        if (!previousAssets.ContainsKey(location)) previousAssets.Add(location, assets[location] as Object);
        assets[location] = prefab;
        return location;
    }

    /// <summary>
    /// 通过真实 UniTask API 返回可等待的任务
    /// </summary>
    private Task Open(string location, string group = "test", object data = null)
    {
        object task = Call(manager, "OpenPanel", location, group, 0, false, data); //真实 UniTask
        return ToTask(task);
    }

    /// <summary>
    /// 调用 UniTask 扩展转换方法而不改变运行时程序集边界
    /// </summary>
    private static Task ToTask(object task)
    {
        Type extensions = task.GetType().Assembly.GetType("Cysharp.Threading.Tasks.UniTaskExtensions", true); //扩展类型
        foreach (MethodInfo method in extensions.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.Name == "AsTask" && method.IsGenericMethodDefinition)
            {
                return (Task)method.MakeGenericMethod(task.GetType().GenericTypeArguments[0]).Invoke(null, new[] { task });
            }
        }
        throw new MissingMethodException("UniTask AsTask");
    }

    /// <summary>
    /// 读取成功打开结果
    /// </summary>
    private static (int id, Component panel) Result(Task task)
    {
        Assert.IsFalse(task.IsFaulted, task.Exception?.ToString());
        Assert.IsFalse(task.IsCanceled);
        object result = task.GetType().GetProperty("Result").GetValue(task); //实际 ValueTuple
        return ((int)result.GetType().GetField("Item1").GetValue(result), (Component)result.GetType().GetField("Item2").GetValue(result));
    }

    /// <summary>
    /// 等待任务完成并限制最长等待时间
    /// </summary>
    private static IEnumerator Wait(Task task)
    {
        return WaitUntil(() => task.IsCompleted);
    }

    /// <summary>
    /// 使用非缩放时间等待真实状态变化
    /// </summary>
    private static IEnumerator WaitUntil(Func<bool> condition)
    {
        float deadline = Time.realtimeSinceStartup + 5f; //等待截止时间
        while (!condition())
        {
            Assert.Less(Time.realtimeSinceStartup, deadline, "等待 UGUI 状态变化超时");
            yield return null;
        }
    }

    /// <summary>
    /// 推进被测管理器的真实更新
    /// </summary>
    private void Tick()
    {
        Call(manager, "Update", 0.02f, 0.02f);
    }

    /// <summary>
    /// 按序列号关闭页面
    /// </summary>
    private void Close(int id)
    {
        Call(manager, "ClosePanel", id, null);
    }

    /// <summary>
    /// 读取真实对象池计数
    /// </summary>
    private int PoolCount(string property)
    {
        return Property<int>(manager.GetType().GetField("instancePool", Flags).GetValue(manager), property);
    }

    /// <summary>
    /// 添加具有真实事件签名的订阅者
    /// </summary>
    private void Subscribe(string name, Action<object> action)
    {
        EventInfo info = manager.GetType().GetEvent(name); //真实事件
        Type argument = info.EventHandlerType.GenericTypeArguments[0]; //事件参数类型
        ParameterExpression sender = Expression.Parameter(typeof(object)); //发送者参数
        ParameterExpression args = Expression.Parameter(argument); //事件数据参数
        Delegate handler = Expression.Lambda(info.EventHandlerType,
            Expression.Invoke(Expression.Constant(action), Expression.Convert(args, typeof(object))), sender, args).Compile(); //适配订阅者
        info.AddEventHandler(manager, handler);
    }

    /// <summary>
    /// 设置测试页面的生命周期回调
    /// </summary>
    private void Hook(string name, Action<Component> action)
    {
        panelType.GetField(name, Flags).SetValue(null, action);
    }

    /// <summary>
    /// 清除用例之间的静态回调
    /// </summary>
    private void ClearHooks()
    {
        if (panelType == null) return;
        foreach (string name in new[] { "Created", "Initialized", "Opened", "Updated" }) Hook(name, null);
    }

    /// <summary>
    /// 获取页面 Canvas 的最小排序值
    /// </summary>
    private static int MinOrder(Component panel)
    {
        int value = int.MaxValue; //最小值
        foreach (Canvas canvas in panel.GetComponentsInChildren<Canvas>(true)) value = Math.Min(value, canvas.sortingOrder);
        return value;
    }

    /// <summary>
    /// 获取页面 Canvas 的最大排序值
    /// </summary>
    private static int MaxOrder(Component panel)
    {
        int value = int.MinValue; //最大值
        foreach (Canvas canvas in panel.GetComponentsInChildren<Canvas>(true)) value = Math.Max(value, canvas.sortingOrder);
        return value;
    }

    /// <summary>
    /// 在 Awake 执行前配置确认弹窗的真实按钮和文字引用
    /// </summary>
    private GameObject ConfirmationPrefab()
    {
        GameObject prefab = Root("Confirm"); //弹窗原型
        prefab.SetActive(false);
        Component panel = prefab.AddComponent(RuntimeType("CommonConfirmPanel")); //弹窗组件
        Field(panel, "okBtn", Child(prefab.transform, "OK").AddComponent<Button>());
        Field(panel, "cancelBtn", Child(prefab.transform, "Cancel").AddComponent<Button>());
        Field(panel, "text", Child(prefab.transform, "Message").AddComponent<TextMeshProUGUI>());
        Field(panel, "okBtnText", Child(prefab.transform, "OKText").AddComponent<TextMeshProUGUI>());
        Field(panel, "cancelBtnText", Child(prefab.transform, "CancelText").AddComponent<TextMeshProUGUI>());
        prefab.SetActive(true);
        return prefab;
    }

    /// <summary>
    /// 构造实际确认数据
    /// </summary>
    private static object ConfirmationData(string text, Action ok, Action cancel)
    {
        object data = Activator.CreateInstance(RuntimeType("ConfirmData")); //确认数据
        Field(data, "showMsg", text);
        Field(data, "okCallBack", ok);
        Field(data, "cancelCallBack", cancel);
        return data;
    }

    #endregion
}
