# Extend

[TaskExtend.cs](TaskExtend.cs) 为 `Task`、`Task<T>` 和 Cysharp `UniTask` 提供 `WithCancellation` 扩展，使等待任务时可以响应 `CancellationToken`。

```csharp
await LoadAsync().WithCancellation(cancellationToken);
```

调用前应确保项目已引用 UniTask；取消等待是否会取消底层工作取决于扩展实现与原任务本身，不能把它当作自动中止网络或资源操作的保证。令牌已取消、任务异常等情况遵循扩展实现传播的取消/异常结果。
