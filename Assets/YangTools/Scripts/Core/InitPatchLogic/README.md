# 初始化与热更新流程

本目录负责游戏启动时的 YooAsset 初始化、资源包补丁检查、下载和进入游戏流程。入口示例位于 `GameInit.cs`，补丁状态机由 `PatchOperation` 和 `FsmNode` 中的节点推进。

## 主要文件

- `GameInit.cs`：启动 YooAsset、创建补丁操作、设置默认资源包，并在操作完成后进入游戏场景。
- `PatchOperation.cs`：将补丁流程封装为 `GameAsyncOperation`，通过 UniMachine 状态机依次执行初始化、版本与清单检查、创建下载器、下载、清理缓存和完成步骤。
- `FsmNode/`：各补丁步骤的具体节点。
- `EventDefine.cs`、`GameInitWindow.cs`：定义流程交互事件，并提供更新进度、错误提示和确认重试的 UI 示例。
- `UniMachine/`：随项目提供的状态机实现，目录内 README 介绍其自身用法。

## 接入方式

场景启动组件可按 `GameInit.Start` 的顺序初始化资源系统并启动操作：

```csharp
YooAssets.Initialize();
var operation = new PatchOperation("DefaultPackage", "", playMode);
YooAssets.StartOperation(operation);
yield return operation;
YooAssets.SetDefaultPackage(YooAssets.GetPackage("DefaultPackage"));
```

实际项目需先完成 YooAsset 的运行模式、资源包和构建配置，并将 `GameInit` 放在启动场景。`PatchOperation` 依赖 YangEvent 事件驱动重试和用户确认；退出或中止操作时会释放本次注册的监听。
