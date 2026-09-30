# 本地存档

`YangSaveDataManager` 使用 `PlayerPrefs` 保存 `DataCenter` JSON，并按 `SaveDataBase` 子类拆分业务数据。数据可按类型取得；修改的数据标记为 dirty 后由管理器写回。

## 主要类型

- `YangSaveDataManager`：单例，在启用时载入存档，定期保存并在销毁时强制保存。
- `DataCenter`：以保存类型名索引各数据块，并管理 dirty 标记。
- `LocalSaveData`：序列化单个数据块。
- `SaveDataBase`：业务存档类型的基类，负责默认数据及反序列化后处理。
- `SaveGameSet`、`SaveGameDataBase`、`Save_QuestData`：当前项目提供的存档示例/数据定义。

## 使用

```csharp
MySaveData save = YangSaveDataManager.Instance.DataCenter.GetLocalSave<MySaveData>(isDirty: true);
save.Coins += 100;
```

`MySaveData` 需由业务实现：标记 `[Serializable]`、继承 `SaveDataBase`、实现 `SetDefaultData`，并遵循 `JsonUtility` 可序列化字段限制。`isDirty: true` 应在修改时设置；无 dirty 标记的改动不会进入增量序列化。该实现使用本地 PlayerPrefs，不提供云存档或加密。
