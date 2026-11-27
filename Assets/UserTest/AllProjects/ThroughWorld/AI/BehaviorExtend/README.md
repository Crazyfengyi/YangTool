# ThroughWorld 行为树扩展

节点可在 Behavior Designer 的 `RoleAI` 分类中添加。旧技能节点和现有调度树共享 `SkillControl` 的执行编号、冷却和结束结果。

| 用途 | 节点 | 配置和结果 |
| --- | --- | --- |
| 目标、生命与技能条件 | `HaveTarget`、`CheckIsHaveTarget`、`CheckDiscContion`、`CheckHpBase`、`CheckIsDie`、`CheckIsInActive`、`NeedUseSkill`、`CheckIsUseSkill`、`CheckIsInCooling`、`CheckTargetInAttckRang` | 血量百分比用 0 到 1。冷却检查当前或最近一次技能及公共间隔。距离条件缺少有效目标时返回失败。 |
| 输入、角色标记与通用条件 | `CheckHaveInput`、`CommonBoolCheck`、`CommonSetBool`、`CommonSetFloat`、`CommonCheck` | 输入节点读取当前角色的移动和跳跃输入。标记节点需要重新选择项目现有的 `RoleFlag`。旧武器、职业、滑步和宠物条件未接入。 |
| 记录和转向 | `RecordPoint`、`RecordAtkPoint`、`LookPoint`、`LookAtkPoint`、`RotateSelf`、`LookTarget` | 先记录挂点或初始朝向，再运行使用记录的节点。记录失败会清除对应的旧值。旧跳跃选点和贝塞尔目标选点需要专用接口。 |
| 寻路与位移 | `AIFindTarget`、`MoveTargetPos`、`PatrolAction`、`JumpToPos`、`AddForceDirection` | 寻路节点使用 `AIPath`，结束时停止寻路。到记录点的位移支持无动态刚体的角色。冲刺节点使用 `AdvancedWalkerController`，仅执行位移时需清空旧命中特效字段。 |
| 技能调度 | `SkillUseAction`、`UseCurrentSkill`、`StartSkillBehaviorTree`、`ThroughWorldBattleSkillAction`、`IsAllReady` | 旧技能启动节点消费已经提交的请求。等待节点跟踪本次请求结果，完成后才返回成功。`IsAllReady` 按顺序执行子节点。 |
| 伤害、Buff 与特效 | `AreaDamage`、`AddBuff`、`PlayEffect`、`PlayHittingEffect`、`CommonAction` | 范围伤害通过角色技能的检测入口执行。Buff 和命中特效须绑定前置节点确认的命中目标。范围检测的独立旋转和自定义命中特效音效目前缺少接口。 |
| 动画、音频与方法 | `PlayAnimation`、`PlayMusic`、`PlayBGMusic`、`ReflectionCallMethod` | 动画需要有效 Animator 状态。事件和方法调用要求参数能唯一匹配。背景音乐支持播放，按名称移除旧音乐栈的操作返回失败。 |

`CheckRoomIsOpen`、`CheckIsState`、`CheckIsDeState`、`CheckIsInSlide` 和三种 `PlayWarning` 节点保留原类型供旧行为树读取。当前项目缺少对应状态接口或专用预警资源，节点返回 `Failure`。通用节点的未适配分支也会在 Console 说明缺项，每个节点实例只提示一次。
