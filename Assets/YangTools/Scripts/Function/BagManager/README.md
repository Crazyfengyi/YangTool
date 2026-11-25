# BagManager 道具数量与存档

## 功能与入口

[BagMgr.cs](BagMgr.cs) 中的 `GameMain.BagMgr` 管理道具缓存，`Save_BagProp` 与 `SaveBagPropItem` 保存本地数量。[ItemData_BagProp.cs](ItemData_BagProp.cs) 提供单个道具的运行时数量运算。

## 依赖与配置

依赖 `MonoSingleton`、`YangSaveDataManager`、存档数据中心及项目的 `cfg` 表类型。先完成数据中心与背包存档初始化，再使用单例；首次存档默认创建空道具列表。

## 最小接入

```csharp
BagMgr.Instance.AddBagProp(propId: 1001, addCount: 3);
bool enough = BagMgr.Instance.BagPropEnough(1001, 2);
float count = BagMgr.Instance.GetBagPropCount(1001);
BagMgr.Instance.RemoveBagProp(1001, 1);
```

`SetBagProp` 设置绝对数量，`AddBagProp` 增加数量；提供 `saveToLocal` 的入口默认同步存档。`TryGetBagProp` 不存在时返回空，数量查询不存在时返回零。

## 示例与限制

本目录没有独立演示场景。优先通过 `BagMgr` 操作以保持缓存与存档一致。直接调用 `ItemData_BagProp.RemoveProp` 不校验余额；底层存档的数量扣除在不足时记录日志后仍继续计算，不能代替上层校验。
