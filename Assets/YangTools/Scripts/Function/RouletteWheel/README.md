# RouletteWheel

该目录实现 UGUI 转盘及条目数据。`RouletteWheel` 负责设置奖项、指定索引/ID 旋转、随机旋转和立即停止；`WheelItemData` 包含条目 ID、名称、图标、权重和业务载荷。目录内 Prefab 用于展示转盘层级和视觉配置。

```csharp
wheel.SetItems(items);
wheel.SpinToId(targetId);
// 也可调用 SpinToIndex(index) 或 SpinRandom()
```

确保转盘 Prefab 的指针、条目容器、动画/回调引用完整；随机结果受条目权重配置影响。若在旋转期间重设数据或调用 `StopImmediately`，调用方需按组件回调约定处理结果，不要重复结算奖励。
