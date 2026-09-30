# PowerSystem

该目录提供可恢复的体力/行动力管理。`CostPowerManager` 管理当前值、消耗和增加；`Save_CostPower` 保存状态，目录内 UI 脚本负责显示/恢复交互。恢复规则由存档数据及管理器配置共同决定。

```csharp
if (CostPowerManager.Instance.TryUseCostPower(1))
{
    // 执行消耗体力后的操作
}
```

接入依赖 `YangSaveDataManager`、对应 `Save_CostPower` 注册/初始化及项目用户 ID 等平台上下文；需确保管理器单例和数据中心已就绪。离线恢复依赖保存的时间信息，设备时钟/平台用户变化需要按项目策略处理。目录中的 `CommonLevelCostPower` 仍是示例/占位逻辑，不应视为已接通的通用道具收费流程。
