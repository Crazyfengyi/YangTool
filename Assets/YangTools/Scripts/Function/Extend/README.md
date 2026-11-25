# Extend 异步取消扩展

## 功能与入口

[TaskExtend.cs](TaskExtend.cs) 的 `TaskExtensions.WithCancellation` 为 `Task`、`Task<T>`、`UniTask` 和 `UniTask<T>` 提供可取消的等待入口。

## 依赖与配置

需要 .NET 任务与取消令牌类型，以及项目已有的 Cysharp UniTask。无需场景组件或 Inspector 配置。

## 最小接入

```csharp
await LoadAsync().WithCancellation(cancellationToken);
```

调用方传入自己的生命周期或操作取消令牌，并处理 `OperationCanceledException`。原任务成功或异常时按对应结果继续传播。

## 示例与限制

使用片段位于源码末尾，本目录没有独立演示场景。扩展取消的是等待，不保证停止底层下载或加载；需要终止底层工作时还应把令牌传给原操作。
