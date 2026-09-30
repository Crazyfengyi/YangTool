# NoitaStyleSpells

该目录实现以定义资产组合投射物行为的法术系统。`BulletConfig` 保存基础弹体参数；`SpellDefinition`、`ProjectileSpellDefinition` 与各类 Modifier Spell ScriptableObject 描述生成、速度、伤害、数量、散射、生命周期和 Prefab 修改；`WandCaster` 负责施法，`BulletFactory` 与 `Bullet` 管理弹体生成、运行及回收。

场景中配置 `WandCaster` 所需的法杖/子弹配置、生成点及 `BulletFactory`，并在 Project 中创建并关联法术定义资产。调用入口：

```csharp
wandCaster.Cast();
```

具体字段以 Inspector 为准。弹体 Prefab 需具备脚本依赖的 Rigidbody/Collider；碰撞层、速度、生命周期与对象池回收条件应一并检查。生命周期法术可能在生成、Tick、命中或死亡时执行，注意避免递归生成失控。
