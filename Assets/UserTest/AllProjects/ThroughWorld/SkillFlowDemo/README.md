# ThroughWorld 技能流程测试

## 启动与操作

直接打开 [SkillFlowDemo.unity](SkillFlowDemo.unity) 并进入 Play Mode，无需从 Init 启动，也不需要加入构建场景列表。建议 Game 视图使用 16:9，便于同时查看左侧面板和右侧角色。

| 按钮 | 行为 |
| --- | --- |
| Timeline 技能 | 提交时间线技能，经 AI 调度节点启动 Director |
| 行为树 A / B | 交替提交两套行为树技能，经同一调度节点启动独立技能树 |
| 打断技能 | 停止本次实际执行器与本轮调度，保留已产生的命中结果 |
| 重置测试 | 停止技能，恢复角色位置、靶子生命和计数 |

白色史莱姆是施法者，红色胶囊是固定靶子。Timeline 和行为树 A 在约 0.8 秒后检测，行为树 B 在约 0.3 秒后检测，每次伤害为 20，靶子初始生命为 200。靶子不会在零血量时销毁，可重复测试。施放期间两个技能按钮不可重复提交。

在技能开始执行后，将 Target 移到范围之外，可观察技能正常结束但没有命中和扣血；开始前超距会取消请求；点击重置后恢复原位。

## 调用流程

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

技能冷却从执行开始计时，执行结束或打断后进入公共间隔；等待阶段取消和校验失败不扣冷却。等待调度默认超时 5 秒，公共间隔默认 2 秒，可在角色 Inspector 配置。
