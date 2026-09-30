# BagManager

本目录提供轻量的道具数量管理和本地存档接入。入口 [BagMgr](BagMgr.cs) 是 `GameMain` 命名空间中的 `MonoSingleton<BagMgr>`；[ItemData_BagProp](ItemData_BagProp.cs) 表示道具 ID 与数量，`Save_BagProp` / `SaveBagPropItem` 保存本地数据。

```csharp
BagMgr.Instance.AddBagProp(propId: 1001, addCount: 3);
bool enough = BagMgr.Instance.BagPropEnough(1001, 2);
float count = BagMgr.Instance.GetBagPropCount(1001);
BagMgr.Instance.RemoveBagProp(1001, 1);
```

`saveToLocal` 默认为 `true`；初始化与存取依赖 `YangSaveDataManager` 已正确配置，并依赖项目的表数据类型。首次接入请确认场景中单例生命周期、存档中心及 `Save_BagProp` 默认数据可用。`SetBagProp` 的第二个参数表示设置后的数量，而 `AddBagProp` 表示增量。
