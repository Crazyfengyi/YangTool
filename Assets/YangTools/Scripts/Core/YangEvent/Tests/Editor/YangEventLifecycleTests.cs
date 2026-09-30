using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YangTools;

/// <summary>
/// 使用真实运行模式验证对象销毁和关闭域重载后的静态清理
/// </summary>
public sealed class YangEventLifecycleTests
{
    /// <summary>
    /// 测试前的运行模式选项
    /// </summary>
    private EnterPlayModeOptions previousOptions;
    /// <summary>
    /// 测试前是否启用运行模式选项
    /// </summary>
    private bool previousOptionsEnabled;

    /// <summary>
    /// 保存编辑器配置以便失败时仍能恢复
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        previousOptions = EditorSettings.enterPlayModeOptions;
        previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
    }

    /// <summary>
    /// 退出运行模式并恢复编辑器选项 原场景由 Test Runner 恢复
    /// </summary>
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        YangEventManager.Instance.RemoveForKey("__YangEventLifecycle");
        EditorSettings.enterPlayModeOptions = previousOptions;
        EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
    }

    /// <summary>
    /// 连续两次进入运行模式不会残留监听 场景卸载后也不触发销毁对象
    /// </summary>
    [UnityTest]
    public IEnumerator EnterPlayModeTwice_ResetsListenersWithoutDomainReload()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        int staleCount = 0;
        YangEventManager manager = YangEventManager.Instance;
        manager.Add(new EventInfo(null, "__YangEventLifecycle", _ => staleCount++));
        yield return new EnterPlayMode(false);
        Assert.AreSame(manager, YangEventManager.Instance);
        manager.Send("__YangEventLifecycle", (object)null);
        Assert.AreEqual(0, staleCount);

        int destroyedCount = 0;
        int globalCount = 0;
        Scene temporaryScene = SceneManager.CreateScene("YangEventRegressionScene");
        GameObject holder = new GameObject("YangEventSceneHolder");
        SceneManager.MoveGameObjectToScene(holder, temporaryScene);
        manager.Add(new EventInfo(holder, "__YangEventLifecycle", _ => destroyedCount++));
        manager.Add(new EventInfo(null, "__YangEventLifecycle", _ => globalCount++));
        yield return SceneManager.UnloadSceneAsync(temporaryScene);
        Assert.IsTrue(holder == null);
        manager.Send("__YangEventLifecycle", (object)null);
        Assert.AreEqual(0, destroyedCount);
        Assert.AreEqual(1, globalCount);
        yield return new ExitPlayMode();

        yield return new EnterPlayMode(false);
        Assert.AreSame(manager, YangEventManager.Instance);
        manager.Send("__YangEventLifecycle", (object)null);
        Assert.AreEqual(0, staleCount);
        Assert.AreEqual(1, globalCount);
        using (YangEventGroup group = new YangEventGroup())
        {
            group.AddListener<LifecycleMessage>(_ => globalCount++);
            YangExtend.SendEvent<LifecycleMessage>(new LifecycleMessage());
            Assert.AreEqual(2, globalCount);
        }
        YangExtend.SendEvent<LifecycleMessage>(new LifecycleMessage());
        Assert.AreEqual(2, globalCount);
        yield return new ExitPlayMode();
    }

    /// <summary>
    /// 生命周期测试使用的普通事件
    /// </summary>
    private sealed class LifecycleMessage { }
}
