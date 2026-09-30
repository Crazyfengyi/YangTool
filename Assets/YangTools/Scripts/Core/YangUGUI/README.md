# UGUI 界面管理

`YangUGUI` 通过资源地址异步加载页面、按组管理层级与逻辑深度、复用页面实例，并提供打开/关闭生命周期、事件和动画基础类。

## 主要文件

- `YangUIManager.cs`：页面组、打开/关闭、回收队列及 Canvas 排序布局。
- `UIMonoInstance.cs`：场景入口，初始化创建辅助器与组，并提供类型安全查询和打开/关闭调用。
- `UIPanel.cs`：页面运行时元数据与池生命周期。
- `UGUIHelper.cs`：页面实例化和页面组辅助器。
- `UGUIPanelBase.cs`：页面逻辑基类、通用开关动画和状态重置。
- `UIScriptDefine.cs`：组配置、公共接口和常量；`UISortingLayout.cs` 负责实际 Canvas 排序。

## 使用

页面逻辑继承 `UGUIPanelBase<TData>`，在场景 `UIMonoInstance` 上配置 Canvas、辅助器和 UI 组，然后异步打开：

```csharp
var result = await UIMonoInstance.OpenPanel<MyPanel>(GroupType.Top);
```

资源名须与 ResourceManager/YooAsset 地址一致，页面预制体须包含 `UIPanel` 和实现 `IUGUIPanel` 的逻辑组件。管理器按组深度和页面打开顺序写入 Canvas 的实际 `sortingOrder`；跨 Sorting Layer 的覆盖关系仍由 Unity 配置决定。
