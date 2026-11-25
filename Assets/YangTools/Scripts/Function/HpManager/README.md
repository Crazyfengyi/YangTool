# HpManager 世界目标血条

## 功能与入口

[HealthBarManager.cs](HealthBarManager.cs) 管理目标注册、屏幕跟随与视图池。[HealthBarTarget.cs](HealthBarTarget.cs) 提供生命、护盾、状态和增益数据；[HealthBarUI.cs](HealthBarUI.cs) 负责平滑填充与图标刷新。

## 依赖与配置

依赖 UGUI、TMP，管理器配置标签使用 Odin。配置血条模板、Canvas、血条容器和世界相机；各视图字段在 Prefab Inspector 中绑定。Canvas 的坐标模式与相机必须匹配。

## 最小接入

```csharp
healthBarManager.RegisterTarget(target);
target.TakeDamage(10f);
healthBarManager.UnregisterTarget(target);
```

自定义数据源实现 `IHealthBarDataSource`，提供跟随锚点、显示数据和数据变化事件。管理器复制数据供视图使用，目标结束使用时应取消注册。

## 示例与限制

[HealthBarManager.prefab](HealthBarManager.prefab)、[HealthBarUI.prefab](HealthBarUI.prefab) 和增益条目为配置参考。[TestHealthBarRegister.cs](TestHealthBarRegister.cs) 在 Start 注册目标，按 A 设置护盾与生命，按 S 造成伤害。

屏幕外或相机背后的视图会隐藏。当前 `hideWhenFullHealth` 的实际判定只使用临时显示截止时间，未按当前生命是否满值判断；不要把字段名当作完整的满血检测。该模块不自动接管 Battle 的伤害与增益流程。
