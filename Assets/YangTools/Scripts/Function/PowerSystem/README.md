# PowerSystem 体力与恢复

## 功能与入口

[CostPowerManager.cs](CostPowerManager.cs) 提供体力增加、消耗与恢复查询。[Save.CostPower.cs](Save.CostPower.cs) 中的 `Save_CostPower` 保存体力状态和时间记录，[CostPowerUI.cs](CostPowerUI.cs) 定期刷新数量与倒计时。

## 依赖与配置

依赖 `YangSaveDataManager`、已注册的 `Save_CostPower`、[BagManager](../BagManager/README.md)、项目表类型与 `PlatformMgr.UserId`。UI 使用 TMP，需在 Inspector 绑定数量和时间文本。先完成存档及单例初始化。

## 最小接入

```csharp
if (CostPowerManager.Instance.TryUseCostPower(1))
{
    // 执行成功扣费后的玩法
}
```

无限体力期间消耗入口直接返回成功。数量查询与恢复使用现存存档实现，需同时核对背包中的体力道具和体力存档字段。

## 示例与限制

本目录没有独立演示场景。恢复和每日次数依赖设备时间与用户信息，不代表服务端校时结果。

`CommonLevelCostPower` 是管理器内的模板方法，其费用字典当前为空，会直接回调成功。`ShowCostPowerAniTips` 的资源加载和动画调用仍被注释，非零数量不会执行完成回调，不能视为已完成的通用扣费和表现链路。
