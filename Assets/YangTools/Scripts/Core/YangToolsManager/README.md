# YangTools 生命周期与模块管理

`YangToolsManager` 是 Core 的生命周期总入口：在场景加载前创建常驻管理物体及对象池父节点，初始化 `GameModuleBase` 模块，并由统一 Unity 循环传入缩放/非缩放时间更新各模块。

## 主要文件

- `YangToolsManager.cs`：模块发现、创建、优先级排序、统一更新及退出清理。
- `UnityLoopScript.cs`：连接 Unity 的 Update/FixedUpdate/LateUpdate；同时驱动自定义协程和 `TimeTool`。
- `UnityLoopScriptExtend.cs`：将扩展脚本注册到统一循环。
- `YangSettingSO.cs`、`SettingInfo.cs`：工具配置数据。
- `YangExtend/`：按游戏、工具、值类型及其他用途拆分的扩展方法。
- `YangToolsPartial/`：应用、数据、功能、泛型和值类型等补充工具模块。

## 使用

```csharp
YangTaskManager taskManager = YangToolsManager.GetModule<YangTaskManager>();
```

`GameModuleBase` 子类由管理器初始化和更新；新增模块应实现 `InitModule`、`Update`、`CloseModule`，并避免自行重复创建全局循环。`DontDestoryObject` 和 `GamePoolObject` 由框架创建，其他系统可将常驻对象或回收对象挂到对应父物体。`UnityLoopScript.AddUpdateAction` 等接口可挂接额外的 Update/FixedUpdate/LateUpdate 回调，注销时需移除对应委托。
