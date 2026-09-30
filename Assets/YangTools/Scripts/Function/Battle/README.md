# Battle

战斗目录提供可组合的角色数值、Buff、技能和武器基础模块，并带示例资源及 PlayMode 测试。当前代码是可扩展的战斗框架部件，不包含完整游戏战斗流程。

## 子模块

- `RoleBase`：`CharacterStats`、属性类型与修正，以及 `IDamageable`、`SimpleHealth` 等角色/生命接口实现。
- `BuffBase`：`BuffDefinition` ScriptableObject 描述持续时间和属性修正；`BuffController` 管理应用与移除。
- `SkillBase`：`SkillCaster`、`SkillDefinition`、消耗/冷却/施法上下文及 `SkillEffect`；`SkillEffect` 子目录有伤害、范围伤害、Buff、近战、投射物和生成 Prefab 等效果。
- `SkillBase/Combo`：连招定义与 `SkillComboController`，包括输入缓冲和步骤窗口。
- `Weapon`：武器定义、运行时实例/库存及武器控制器，用于与技能施放衔接。
- `SO`：战斗相关 ScriptableObject 辅助定义；`Tests/PlayMode` 含连招控制器回归测试。
- 根目录的 `Bullet.prefab`、`Demmo.prefab` 为示例对象，不是运行时自动加载入口。

典型技能配置是在 Project 中创建 `SkillDefinition` 及其效果资产，在角色对象添加并配置 `SkillCaster`，再通过代码施放：

```csharp
skillCaster.AddSkill(skillDefinition);
bool accepted = skillCaster.TryCast(skillDefinition, target);
```

技能效果引用、动画触发、资源/属性组件与投射物 Prefab 均需按具体定义在 Inspector 配置。Buff 和武器同样由 ScriptableObject 数据驱动。修改或扩展效果时检查目标实现了所需接口；示例与测试资源请勿当作完整生产配置。
