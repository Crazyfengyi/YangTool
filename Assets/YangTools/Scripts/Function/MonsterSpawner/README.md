# MonsterSpawner

该目录提供场景怪物生成器、生成点与配置数据，并含示例 Prefab。`MonsterSpawner` 支持连续生成和波次生成，可设置最大存活数量、生成间隔、对象池参数、波次内容及是否循环；通过事件通知生成和波次状态。

```csharp
spawner.StartSpawn();
// 停止继续生成；需要时清理当前存活列表
spawner.StopSpawn();
spawner.ClearAliveList();
```

在 Inspector 配置生成点、生成配置及怪物 Prefab，确认生成对象生命周期/销毁能正确通知生成器。`autoStart` 控制启用后自动开刷；连续与波次配置字段按所选模式填写。切场景或停止玩法时应显式停止协程并按需要清场。
