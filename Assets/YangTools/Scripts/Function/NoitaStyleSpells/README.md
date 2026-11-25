# NoitaStyleSpells 独立法杖法术

## 功能与入口

[WandCaster.cs](WandCaster.cs) 创建施法上下文，[SpellRunner.cs](SpellRunner.cs) 顺序执行法术链表。修饰法术先累积到上下文，随后由 `ProjectileSpellDefinition` 消费并生成弹体。

| 配置 | 职责 |
| --- | --- |
| [BulletConfig](BulletConfig.cs) | 基础弹体 速度 伤害 寿命 数量 散射和碰撞层 |
| 各 ModifierSpellDefinition | 修改数量 速度 伤害 散射 预制体或追加生命周期法术 |
| [BulletLifecycleSpellLists](BulletLifecycleSpellLists.cs) | 出生 周期 命中 死亡四组法术 |
| [BulletFactory](BulletFactory.cs) 与 [Bullet](Bullet.cs) | 按预制体复用弹体 刚体移动与生命周期触发 |

## 依赖与配置

使用 `NoitaStyleSpells` 命名空间。弹体 Prefab 需要本模块 Bullet、Rigidbody 和 Collider；配置物理层与触发器规则。在场景配置 BulletFactory，并在 WandCaster 绑定工厂、可选发射点与按顺序排列的法术资产。

## 最小接入

先添加修饰法术，再添加引用 BulletConfig 的投射物法术，最后调用 `wandCaster.Cast()`。修饰器在生成时消费；子生命周期上下文共享来源和工厂，但不继承未消费的修饰器。

## 示例与限制

本目录没有独立演示场景或 Prefab。弹体以刚体速度前进，在命中指定层或寿命结束时回收。Damage 目前是运行时参数，本模块未直接调用 Battle 的 `IDamageable` 伤害接口。

这是独立法杖方案，与 [Battle](../Battle/README.md) 的 SkillProjectile 不互换。生命周期法术可以再次生成弹体，当前没有共享生成预算或最大递归深度，配置时应避免出生或死亡法术形成自循环。
