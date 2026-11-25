# Battle 角色 技能与武器

## 功能与入口

战斗模块由角色属性、Buff、技能、连招与武器组成，数据主要通过 ScriptableObject 配置。目录使用以下实际路径：

| 目录 | 内容 |
| --- | --- |
| [1_RoleBase](1_RoleBase) | CharacterStats 属性计算 IDamageable 接口与 SimpleHealth |
| [2_BuffBase](2_BuffBase) | BuffDefinition 配置与 BuffController 持续和移除 |
| [3_SkillBase](3_SkillBase) | SkillCaster 施放 资源 冷却 上下文与效果 |
| [3_SkillBase/Combo](3_SkillBase/Combo) | 连招步骤 输入缓冲 命中确认与接招窗口 |
| [3_SkillBase/Projectile](3_SkillBase/Projectile) | 发射排列 飞行轨迹 生命周期派生与预算 |
| [4_Weapon](4_Weapon) | 武器配置 库存 装备 弹药 装填与射击控制 |
| [5_SO](5_SO) | 技能 效果 弹体 Buff 与武器的示例资产 |
| [Tests](Tests) | Demmo 预制体与 PlayMode 回归测试 |

## 依赖与配置

需要 Unity 物理组件，配置面板使用 Odin。角色按用途配置 `SkillCaster`、可选的 `SkillResource`、属性和生命组件；将效果资产填入技能定义，并为投射物配置 Prefab、目标层与阻挡层。武器、动画和 UI 不会由此目录自动装配。

## 最小接入

```csharp
skillCaster.AddSkill(skillDefinition);
bool accepted = skillCaster.TryCast(skillDefinition, target);
```

返回 `true` 表示请求通过校验并开始施放，不表示已经命中。技能可在等待施法时间或动画事件后生效；命中由具体伤害效果上报到 `SkillCastSession`。

连招为线性步骤列表。配置 `SkillComboDefinition` 并关联 `SkillComboController` 后，每次输入提交一次 `TryTrigger`。前一段结束后仍需在接招窗口内收到有效输入；提前输入只在缓冲时间内有效。开启 `requireHit` 时还需该段实际命中，仅有开始日志不能满足条件。步骤数由资产的列表长度决定。

## 示例与限制

- [Tests/Demmo.prefab](Tests/Demmo.prefab)：连招对象示例，输入脚本为 [SkillComboKeyInputExample](3_SkillBase/Example/SkillComboKeyInputExample.cs)，默认使用旧输入系统的鼠标左键。配置资产位于 [5_SO/1_Skills/ComboSkill1](5_SO/1_Skills/ComboSkill1)。放入场景后仍需可命中的目标和正确的物理层设置。
- [打击感演示](HitFeelDemo/README.md)：独立场景及命中反馈操作说明。
- [投射物模式演示](ProjectilePatternDemo/README.md)：7 种发射排列、10 种轨迹以及命中、消亡、飞行节点派生的参数说明。所有分支共享一次根施法的生成数量和深度预算。
- [Tests/PlayMode](Tests/PlayMode)：连招和投射物回归测试源码，测试代码受测试编译条件控制。

本目录提供框架部件和演示，不包含完整关卡、AI 或游戏结算。独立法杖系统见 [NoitaStyleSpells](../NoitaStyleSpells/README.md)，其配置和 Bullet 类型不能直接替换本模块资产。
