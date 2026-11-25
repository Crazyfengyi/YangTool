# MonsterSpawner 怪物生成

## 功能与入口

[MonsterSpawner.cs](MonsterSpawner.cs) 支持连续生成和波次生成，提供生成、波次开始与完成事件。[MonsterSpawnTypes.cs](MonsterSpawnTypes.cs) 保存权重池和波次参数，[MonsterSpawnPoint.cs](MonsterSpawnPoint.cs) 提供位置锚点。

## 依赖与配置

配置生成点、怪物 Prefab、最大存活数以及所选模式的规则。Inspector 使用 Odin；`autoStart` 在 Start 自动开始，连续模式配置生成间隔与权重列表，波次模式配置数量、延迟和是否等待全部销毁。

## 最小接入

```csharp
spawner.StartSpawn();
spawner.StopSpawn();
```

`AliveCount` 会清除已销毁的 Unity 对象引用。需要重新计数时可调用 `ClearAliveList()`，但该方法只清空记录，不销毁场上怪物。

## 示例与限制

[Manager.prefab](Manager.prefab) 与 [Monster.prefab](Monster.prefab) 为配置参考；`MonsterAutoDespawn` 在首次 Start 后按时销毁。生成器直接 Instantiate，没有实现对象池。仅 SetActive(false) 不算已销毁，仍会占用存活数；切场景或结束玩法时应显式停止生成并自行处理存量对象。
