# 自定义 IEnumerator 调度器

`YangCoroutineManager` 用一个轻量队列推进 `IEnumerator`，无需将每个迭代器交给 MonoBehaviour。它只识别普通 `yield` 和 `IWait`，不等同于 Unity 原生协程调度器。

## 主要文件

- `YangCoroutineManager.cs`：单例队列、启动/停止及每轮推进。
- `IWait`、`YangWaitSecond`：等待条件接口与按 `Time.deltaTime` 计时的实现。

## 使用

```csharp
YangCoroutineManager.Instance.StartCoroutine(RunSequence());

IEnumerator RunSequence()
{
    Debug.Log("开始");
    yield return new YangWaitSecond(2f);
    Debug.Log("两秒后");
}
```

`UnityLoopScript.Update` 会自动调用 `UpdateCoroutine()`，通常无需额外接入更新。`YangWaitSecond` 使用缩放时间，暂停游戏时等待也会暂停；结束对象关联的流程时调用 `StopCoroutine`。
