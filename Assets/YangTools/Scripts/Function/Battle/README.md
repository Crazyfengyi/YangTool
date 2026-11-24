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



/*
连招系统可以设计成：**每一招是独立技能，连招负责决定“什么条件下能接哪一招”**。技能处理伤害、位移和效果，连招处理输入、衔接时机和分支。

你的项目已经有基础结构，可以在它上面扩展：

| 现有组件 | 职责 |
|---|---|
| [SkillComboDefinition](D:/UnityProject/YangTool/Assets/YangTools/Scripts/Function/Battle/SkillBase/Combo/SkillComboDefinition.cs) | 配置每段技能、动画、衔接时间和命中条件 |
| [SkillComboController](D:/UnityProject/YangTool/Assets/YangTools/Scripts/Function/Battle/SkillBase/Combo/SkillComboController.cs) | 推进连招、缓存输入、处理命中和超时 |
| [SkillComboKeyInputExample](D:/UnityProject/YangTool/Assets/YangTools/Scripts/Function/Battle/SkillBase/Example/SkillComboKeyInputExample.cs) | 鼠标左键触发连招的输入示例 |

当前实现偏向“连续点击，依次释放技能”的线性连招。动作游戏可以进一步这样设计。

**1. 把输入缓冲和衔接窗口分开**

例如第一刀持续 0.6 秒：

| 时间 | 行为 |
|---|---|
| 0～0.15 秒 | 前摇，准备攻击 |
| 0.15～0.25 秒 | 攻击生效，开启伤害判定 |
| 0.25～0.45 秒 | 后摇，允许接第二刀 |
| 0.45～0.6 秒 | 收招，结束后回到待机 |

玩家提前按攻击，可以缓存约 0.2 秒；进入衔接窗口时再消费输入。**按一次最多推进一段**，避免连续乱按提前排满整个连招。

你当前的控制器在技能施放完成后打开延续窗口。如果希望提前衔接、取消部分后摇，需要让技能执行过程提供“允许接招”的时机。

**2. 简单连击用列表，轻重攻击分支用图**

如果只有三连击，现有步骤列表就够了。需要轻重组合时，再改成节点和转换关系：

```text
待机 → 轻击1 → 轻击2 → 轻击3
          ↓重击     ↓重击
         挑空       重斩
```

每条转换配置输入类型、允许时间、是否要求命中、资源消耗和目标技能。多个条件同时成立时，用明确的优先级选择一条。

**3. 让技能时序成为统一依据**

动画、伤害判定和接招窗口应跟随同一套技能时间推进，避免各自使用独立计时器而逐渐错位。

连招只发起技能请求；技能系统返回开始、命中、可衔接、完成和被打断事件。动画负责表现，伤害仍由战斗逻辑确认。

**4. 先确定打断规则**

至少明确这些行为：

- 普通受击是否打断，霸体期间是否例外。
- 闪避能在哪个阶段取消攻击。
- 超时后从第一段重新开始。
- 切武器、死亡时清理进度和输入缓存。
- 旧技能延迟命中时，通过施放会话识别来源，避免推进新的连招。

对你这个项目，建议先完善**三段普攻＋输入缓冲＋后摇衔接＋受击打断**，确认操作手感，再增加轻重分支、挑空和空中连招。现有控制器已经具备步骤推进、输入缓存和会话关联，适合作为起点。
*/

## 弹幕排列与子弹链

[ProjectilePatternDemo](ProjectilePatternDemo/README.md) 提供 7 种排列、10 种轨迹、命中换目标、消亡散射和沿途派生的独立演示。打开 [ProjectilePatternDemo.unity](ProjectilePatternDemo/ProjectilePatternDemo.unity) 运行，在左侧面板切换模式并发射。配置位于 ProjectileSkillEffect 的中文 Odin 面板。
