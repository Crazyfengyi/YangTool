# 泛型有限状态机

`YangFSM` 提供与业务对象绑定的有限状态机。状态由 `FsmStateBase<T>` 派生，状态机切换时负责调用状态初始化、进入、更新和结束回调。

## 主要文件

- `YangFsmManager.cs`：`YangFsmManager` 模块管理命名状态机；`YangFsm<T>` 保存宿主并执行状态切换；`FsmStateBase<T>` 是业务状态基类。

## 使用

```csharp
var fsm = YangToolsManager.GetModule<YangFsmManager>()
    .CreateFsm("player", player, states);
fsm.ChangeStateTo<IdleState>();
```

`T` 是状态访问的业务宿主类型，`states` 需由业务创建并传入；状态类型必须属于传入状态列表，并在首次更新前切换到起始状态。状态机由 YangTools 模块持续更新。当前管理器没有单个状态机的移除接口，因此应在创建时控制其生命周期，并在业务结束后切换到不再执行工作的状态；`Close()` 目前不负责从管理器注销。
