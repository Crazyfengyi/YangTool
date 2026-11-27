# ThroughWorld 技能流程测试

## 启动与操作

直接打开 [SkillFlowDemo.unity](SkillFlowDemo.unity) 并进入 Play Mode，无需从 Init 启动，也不需要加入构建场景列表。建议 Game 视图使用 16:9，便于同时查看左侧面板和右侧角色。

| 按钮 | 行为 |
| --- | --- |
| Timeline 技能 | 提交时间线技能，经 AI 调度节点启动 Director |
| 行为树 A / B | 交替提交两套行为树技能，经同一调度节点启动独立技能树 |
| 表驱动整合技能 | 行为树提交 Battle 会话，由 Timeline 在两个时间点触发伤害 |
| 打断技能 | 停止本次实际执行器与本轮调度，保留已产生的命中结果 |
| 重置测试 | 停止技能，恢复角色位置、靶子生命和计数 |

白色史莱姆是施法者，红色胶囊是固定靶子。旧流程的 Timeline 技能和行为树 A 在约 0.8 秒后检测，行为树 B 在约 0.3 秒后检测，命中时各扣 20 点生命。靶子初始生命为 200，生命归零后仍保留，可重置后继续测试。施法期间再次点击施法按钮会被拦截。

测试旧流程时，施法开始后、伤害检测前将 `Target` 移出命中范围，可以观察到技能执行结束但靶子生命不变。提交前超距会触发距离校验。点击「重置测试」可恢复原位。

## 技能系统流程

### 旧 ThroughWorld 流程

「Timeline 技能」和「行为树 A / B」都将 `RunTimeSkillData` 提交给 `SkillControl`。`SkillUseAction` 启动对应的执行器，两条路径在 `RoleBase.DamageCheck` 汇合。

```text
按钮提交 RunTimeSkillData
  → SkillControl.TryUseSkill 校验并进入等待状态
  → DemoSkillScheduler / Sequence
      → NeedUseSkill 条件
      → SkillUseAction
          ├─ Timeline → PlayableDirector.Play
          │             → DamageCheckClip / DamageCheckBehaviour
          └─ 技能树 → DemoSkillExecution
                       → 攻击动画 → Wait → SkillFlowDamageAction → Wait
  → RoleBase.DamageCheck
  → PhysicsCheckManager → RoleBase.Atk → GameBattleManager.HitProcess
  → 测试靶子 BeHit 记录生命与次数
  → Director.stopped 或 OnBehaviorEnd
  → SkillControl.SkillEnd
```

控制器的 `IsExecuting` 表示执行节点已经启动；请求被接受不等于已经命中。Timeline 使用实际停止事件完成，行为树使用执行结束事件完成。打断会先清除请求，再停止实际执行器，避免被误当作正常完成。

### Battle 整合流程

```text
「表驱动整合技能」按钮 → SkillFlowBattleDemo → 启动 IntegratedScheduler
  → ThroughWorldBattleSkillAction 提交映射与目标
  → ThroughWorldBattleAdapter 按映射 ID 读取 Luban 技能表
  → SkillCaster.TryCast 校验占用、资源、冷却与目标距离
  → 创建 SkillCastSession 和 SkillContext
  → SkillTimelinePlayer 播放 IntegratedTimeline
      → 每个 SkillTimelineClip 在本次会话中触发一次 Battle SkillEffect
      → Battle 计算伤害 → 目标的 ThroughWorldBattleAdapter.TakeDamage
      → RoleBase.BeHit 扣除生命 → Battle 效果向会话上报实际命中
  → Timeline 播放结束 → SkillCaster 记录冷却并通知会话结束
  → ThroughWorldBattleSkillAction 读取会话结果
```

行为树节点持有本次请求返回的 `SkillCastSession`。会话进行中返回 `Running`，自然结束返回 `Success`，请求被拒绝或会话被打断时返回 `Failure`。如果行为树节点提前退出，会打断尚未结束的会话。打断会停止后续 Timeline 片段，已扣除的生命保留。

### 配置职责与排查

| 配置位置 | 决定什么 | 何时查看 |
| --- | --- | --- |
| Luban 技能表和 `IntegratedMapping` | 技能 ID、固定伤害、攻击倍率、施法距离、冷却，以及对应的 `SkillDefinition` 和 Timeline | 请求被拒绝，或需要调整技能数值 |
| Behavior Designer 的 `IntegratedScheduler` | 传给施法节点的映射和目标、等待会话结果的时机 | 点按钮后没有提交施法，或行为树提前结束 |
| `IntegratedTimeline` | 何时播放动画、何时触发哪个 Battle 效果 | 有动画但效果触发时机或次数不符 |
| `IntegratedDamage` 等 Battle 效果资产 | 效果类型、命中方式，以及伤害来自技能表还是效果资产 | 有效果触发但靶子未扣血，或伤害不符 |

重置后运行技能 ID `1002`，两个伤害片段应各扣约 `66.67` 点生命，靶子从 `200` 降到约 `66.67`。旧流程在效果触发时做物理范围检测。整合示例在请求时检查施法距离，直接伤害片段触发时使用已选目标，因此靶子在施法开始后移出距离仍可能受击。

旧流程在执行器启动时开始技能冷却，已启动的技能结束或打断后再进入公共间隔。整合示例在 Timeline 自然结束时由 `SkillCaster` 开始表配置的冷却，拒绝或打断不会启动本次冷却。

## 资源与 Inspector 配置

- `DemoManager`：显式绑定施法者、靶子、三个技能配置、四个 Button 和两个 TMP 状态文本；只放置测试需要的战斗与调试绘制管理器。
- `Caster`：复用现有怪物 Prefab 的模型与 Animator，在本场景中移除正式怪物 AI、移动和寻路组件，替换为 `SkillFlowDemoRole`。`AI调度行为树` 与 `技能执行行为树` 分别绑定两个 BehaviorTree，禁止自动启动、自动循环及停用后保留暂停状态。
- `DemoSkillScheduler.asset`：手动启动的一轮 AI 技能调度。`Resources/Skills/DemoSkillExecution.asset` 和 `DemoSkillExecutionB.asset`：不同前摇时长的技能序列，两棵树都可用 Behavior Designer 打开。
- `TimelineSkill.asset`、`BehaviorSkill.asset` 与 `BehaviorSkillB.asset`：独立测试定义，创建与正式入口兼容的临时 `cfg.skill.Skill`，不读取或修改全局技能表。Timeline 按 `Resources/Skills` 下的资源名加载；行为树按 `ResName` 从同一路径加载，仅资源名为空时回退至原绑定树。
- `Resources/Skills/SkillFlowDemoTimeline.playable`：现有 `Skill_1001` 的独立副本，保留动画，移除不参与对照的特效轨道，将伤害检测设为单次矩形。两种方式的范围参数均为宽 3、高 3、长 3。
- `DemoUIFont.asset`：使用项目已有 NotoSans 字体源生成的专用 TMP 字体，预填面板中文字与动态状态，避免影响其他界面的字体设置。

## 测试边界

本演示复用 ThroughWorld 的技能调度、物理检测和战斗命中入口。测试角色只替代依赖正式属性表、存档和 HUD 的初始化与生命显示；不启动 GameActorManager 更新循环，不演示正式怪物的自动选技或关卡加载。

本次共享路径修复保留原 `UseSkill`、`BreakSkill` 等入口，并兼容未显式绑定技能树的旧双行为树 Prefab。正式伤害公式未改；测试技能以固定值提供 20 点伤害。

已在 Play Mode 核对两条路径正常命中、重复提交拦截、范围外不命中、两种前摇打断和打断后重用。中文面板、按钮射线命中与事件链也已检查。上述结果不代表正式关卡的 AI、存档或 HUD 已完成回归测试。

旧流程在等待调度时取消或校验失败不会启动冷却。等待调度默认超时 5 秒，公共间隔默认 2 秒，可在角色 Inspector 配置。

## Battle 与 ThroughWorld 整合示例

点击“表驱动整合技能”运行第三条路径。`IntegratedScheduler` 的 `ThroughWorldBattleSkillAction` 提交请求并等待对应的 Battle 施法会话；`IntegratedTimeline` 保留动画，并在两个 Battle 效果片段触发 `IntegratedDamage`。示例读取项目 JSON 技能表中 ID 1002，固定伤害为 50，攻击倍率为 1。施法者攻击为 30、靶子防御为 20 时，Battle 比例减防后每次约扣 66.67 生命，两次后靶子剩余约 66.67 生命。点击重置可恢复生命并清除示例施法器冷却。

接入其他技能时：

1. 创建 `ThroughWorldBattleSkill`，配置技能表 ID、Battle `SkillDefinition` 和 Timeline。角色上显式绑定 `ThroughWorldBattleAdapter`、`SkillCaster`、`SkillTimelinePlayer` 及其 Director 和 Animator。
2. 在行为树中配置 `ThroughWorldBattleSkillAction` 和目标。正式运行使用 `GameTableManager` 的技能表；节点的演示表文件仅用于本场景脱离 Init 独立运行。
3. 在 Timeline 添加 Battle 效果轨道与片段，指定伤害、投射物或其他 Battle 效果资产。伤害来源设为技能表时使用本次施法读取的固定伤害和攻击倍率；选择效果资产时使用资产中的数值。已有资产保持原有资产数值来源，迁移时需显式选择技能表。
4. ThroughWorld 属性经适配器提供给 Battle 公式，已结算伤害直接进入角色受击入口，避免再次使用旧减防公式。需要 Battle 属性或 Buff 修正时在角色上配置 `CharacterStats`；Buff 效果应明确选择一种目标系统，避免同时重复施加。

需要飞行碰撞或命中时范围检测的技能应配置对应 Battle 效果。投射物携带本次施法的表数据快照。

这条示例不迁移正式怪物技能，也不覆盖正式关卡初始化、选目标或完整 Buff 流程。旧 ThroughWorld 技能仍使用原有执行路径。
