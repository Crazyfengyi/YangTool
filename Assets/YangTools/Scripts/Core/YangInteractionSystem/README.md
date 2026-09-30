# 物体交互系统

本目录提供鼠标/指针驱动的聚焦、选择和拖拽交互流程。交互目标实现 `ICanFocus`、`ICanSelect` 或 `ICanDrag`，交互情景实现 `IInteractBase`，由 `YangInteractSystem` 统一判断当前情景并分发回调。

## 主要文件

- `YangInteractSystem.cs`：跟踪当前焦点、选择和拖拽对象，扫描并运行已声明的交互情景。
- `IInteract.cs`、`IInteractBase.cs`：目标能力和交互情景接口/基类。
- `ObjectInteractAttribute.cs`：声明情景对应的主体、目标类型及默认启用状态。
- `InteractUIBase.cs`、`DragTargetInteractBase.cs`：UI 指针事件和拖拽情景辅助基类。
- `Example/`：场景物体与 UI 之间拖拽的示例实现；`Example/QuickOutline/` 是示例视觉效果依赖。

## 接入

目标组件实现所需能力，并返回稳定的 `InteractTag` 类型标识；自定义交互情景继承 `InteractBase` 并标注 `ObjectInteractAttribute`。系统在运行时扫描其程序集中的情景类型，并由 `UnityLoopScript` 驱动更新。

场景交互依赖摄像机/射线设置；UI 交互依赖 EventSystem 和合适的 GraphicRaycaster。`InteractRelationMapper` 描述注册情景中的类型关系。
