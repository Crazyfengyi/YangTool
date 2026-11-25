# RouletteWheel UGUI 转盘

## 功能与入口

[RouletteWheel.cs](RouletteWheel.cs) 提供按索引、ID 或权重随机旋转以及立即停止。[WheelItemData.cs](WheelItemData.cs) 保存条目名称、图标、权重和业务载荷，[RouletteWheelItemUI.cs](RouletteWheelItemUI.cs) 负责布局与图文展示。

## 依赖与配置

依赖 UGUI、TMP、DOTween 和 Odin。配置转盘根节点、指针、条目容器与模板；可选按钮绑定在 Awake 完成。

## 最小接入

```csharp
wheel.SetItems(items);
wheel.SpinToId(targetId);
// 也可使用 SpinToIndex(index) 或 SpinRandom()
```

## 示例与限制

[Demo.prefab](Demo.prefab) 与 [Are.prefab](Are.prefab) 为参考资源，[RouletteWheelDemo.cs](RouletteWheelDemo.cs) 负责初始化演示条目和记录结果。组件停用或销毁会停止旋转；运行中重设列表和提前停止时，业务应按实际回调处理，避免重复结算。动画表现不负责服务端奖励裁定或存档。
