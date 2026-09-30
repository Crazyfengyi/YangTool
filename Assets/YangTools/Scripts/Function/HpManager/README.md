# HpManager

该模块将世界空间生命/护盾/状态数据显示到 UGUI 血条。`HealthBarManager` 管理目标注册、刷新与血条复用；`HealthBarTarget` 是可直接使用的数据源；`HealthBarUI` 和 Buff 项组件负责视图，目录内 Prefab 为对应 UI 模板。

```csharp
healthBarManager.RegisterTarget(target);
target.TakeDamage(10f);
healthBarManager.UnregisterTarget(target);
```

接入时在场景配置 `HealthBarManager` 的血条 Prefab、Canvas/Root、目标相机，以及初始池容量和显示策略；Canvas 模式需与相机设置一致。自定义角色可实现 `IHealthBarDataSource` 并在状态变化后通知管理器刷新。确保 Prefab 的血条组件和引用完整，销毁/卸载目标前取消注册。
