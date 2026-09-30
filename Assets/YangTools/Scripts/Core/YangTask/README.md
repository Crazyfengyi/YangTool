# 帧驱动任务

`YangTaskManager` 按优先级更新 `TaskBase` 任务，管理任务创建、等待/运行/结束状态及主动取消。它适合由多个生命周期回调组成、需要统一轮询的业务任务。

## 主要文件

- `YangTaskManager.cs`：创建、排序、更新和取消任务。
- `TaskManagerBase.cs`：`TaskBase`、`ITaskManager`、`TaskStatus` 等核心契约。
- `ConditionalTask.cs`：基于条件的任务实现。

## 使用

先派生任务并重写 `OnStart`、`OnUpdate` 等生命周期方法，再注册到模块：

```csharp
MyTask task = YangToolsManager.GetModule<YangTaskManager>().CreateTask<MyTask>();
// 需要取消时
YangToolsManager.GetModule<YangTaskManager>().CancelTask(task, "业务取消");
```

管理器由 `YangToolsManager` 每帧驱动。任务完成或取消后会从活动列表移除；长时间运行的任务应在取消回调中释放自己持有的资源，并避免在已结束状态继续执行副作用。
