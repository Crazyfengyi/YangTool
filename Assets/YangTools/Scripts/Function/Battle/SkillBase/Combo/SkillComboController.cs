using System;
using UnityEngine;

/// <summary>
/// 线性技能连击控制器，负责步骤推进、输入缓存和命中确认。
/// </summary>
public class SkillComboController : MonoBehaviour
{
    //连招状态枚举，定义了角色在连招过程中的不同状态
    private enum ComboState
    {
        Idle, // 空闲状态，角色未执行连招
        Casting, // 施法状态，角色正在执行连招中的某个动作
        WaitingForContinuation, // 等待连招继续状态，等待玩家输入继续连招
        FinalRecovery // 最终恢复状态，连招结束后的恢复阶段
    }

    //连招输入模式枚举，定义了不同的连招输入方式
    private enum ComboInputMode
    {
        Target, // 目标模式，以游戏对象为目标进行连招
        Point, // 点位模式，以世界中的某个点为目标进行连招
        Direction // 方向模式，以某个方向为目标进行连招
    }

    //连招输入结构体，存储连招所需的输入信息
    private struct ComboInput
    {
        public ComboInputMode Mode; // 输入模式，使用上述枚举定义
        public GameObject Target; // 目标游戏对象，当Mode为Target时使用
        public Vector3 Point; // 目标点位，当Mode为Point时使用
        public Vector3 Direction; // 目标方向，当Mode为Direction时使用
    }

    /// <summary>
    /// 技能连招系统的相关变量和事件定义
    /// </summary>
    [SerializeField] private SkillCaster skillCaster; //设置技能施放器

    [SerializeField] private SkillComboDefinition comboDefinition; //设置技能连招的定义
    [SerializeField] private Animator animator; //设置动画控制器

    //连招系统事件
    public event Action<SkillComboDefinition> OnComboStarted; // 连招开始时触发的事件
    public event Action<int, SkillDefinition> OnStepStarted; // 连招步骤开始时触发的事件，参数为步骤索引和技能定义
    public event Action<int, GameObject> OnStepHit; // 连招步骤命中时触发的事件，参数为步骤索引和命中的游戏对象
    public event Action<SkillComboDefinition> OnComboCompleted; // 连招完成时触发的事件
    public event Action<SkillComboDefinition> OnComboReset; // 连招重置时触发的事件

    public SkillComboDefinition ComboDefinition => comboDefinition; // 获取连招定义的属性
    public int CurrentStepIndex => currentStepIndex; // 获取当前步骤索引的属性
    public bool IsComboActive => state != ComboState.Idle; // 判断连招是否处于激活状态的属性
    public bool HasBufferedInput => hasBufferedInput && Time.time <= bufferedInputExpireTime; // 判断是否有缓冲输入的属性

    private ComboState state; // 连招状态
    private SkillCastSession activeSession; // 当前激活的技能施放会话
    private int currentStepIndex = -1; // 当前步骤索引，初始值为-1
    private int submittingStepIndex = -1; // 正在提交的步骤索引，初始值为-1
    private bool submittingStep; // 是否正在提交步骤
    private bool activeCastCompleted; // 当前激活的施放是否已完成
    private bool activeStepHit; // 当前激活的步骤是否已命中
    private float continuationExpireTime; // 连招延续过期时间
    private bool hasBufferedInput; // 是否有缓冲输入
    private ComboInput bufferedInput; // 缓冲的输入
    private float bufferedInputExpireTime; // 缓冲输入的过期时间

    /// <summary>
    /// 重置组件，获取必要的组件引用
    /// </summary>
    private void Reset()
    {
        // 获取技能施放器组件
        skillCaster = GetComponent<SkillCaster>();
        // 获取子对象中的动画器组件
        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 游戏对象初始化时调用，解决依赖关系并注册连招技能
    /// </summary>
    private void Awake()
    {
        // 解决组件依赖关系
        ResolveDependencies();
        // 注册连招技能
        RegisterComboSkills();
    }

    /// <summary>
    /// 当游戏对象启用时调用，解决依赖关系、订阅施放器事件并注册连招技能
    /// </summary>
    private void OnEnable()
    {
        // 解决组件依赖关系
        ResolveDependencies();
        // 订阅技能施放器事件
        SubscribeCasterEvents();
        // 注册连招技能
        RegisterComboSkills();
    }

    /// <summary>
    /// 当游戏对象禁用时调用，取消订阅施放器事件并清除连招状态
    /// </summary>
    private void OnDisable()
    {
        // 取消订阅技能施放器事件
        UnsubscribeCasterEvents();
        // 清除连招状态，传入是否处于连招中的状态
        ClearComboState(IsComboActive);
    }

    /// <summary>
    /// 每帧更新调用，处理缓冲输入和连招超时逻辑
    /// </summary>
    private void Update()
    {
        // 检查缓冲输入是否过期，如果过期则清除缓冲输入
        if (hasBufferedInput && Time.time > bufferedInputExpireTime)
        {
            ClearBufferedInput();
        }

        // 检查是否处于等待继续或最终恢复状态，并且是否超时
        if ((state == ComboState.WaitingForContinuation || state == ComboState.FinalRecovery)
            && Time.time >= continuationExpireTime)
        {
            // 判断是否被中断，如果不是最终恢复状态则视为中断
            bool interrupted = state != ComboState.FinalRecovery;
            // 清除连招状态，传入中断状态
            ClearComboState(interrupted);
        }
    }

    /// <summary>
    /// 使用目标触发当前连击。
    /// </summary>
    public bool TryTrigger(GameObject target = null)
    {
        return HandleInput(new ComboInput
        {
            Mode = ComboInputMode.Target,
            Target = target
        });
    }

    /// <summary>
    /// 使用指定位置触发当前连击。
    /// </summary>
    public bool TryTriggerAtPoint(Vector3 point)
    {
        return HandleInput(new ComboInput
        {
            Mode = ComboInputMode.Point,
            Point = point
        });
    }

    /// <summary>
    /// 使用指定方向触发当前连击。
    /// </summary>
    public bool TryTriggerInDirection(Vector3 direction)
    {
        return HandleInput(new ComboInput
        {
            Mode = ComboInputMode.Direction,
            Direction = direction
        });
    }

    /// <summary>
    /// 更换当前连击配置并清空进度。
    /// </summary>
    public void SetCombo(SkillComboDefinition combo)
    {
        ClearComboState(IsComboActive);
        comboDefinition = combo;
        RegisterComboSkills();
    }

    /// <summary>
    /// 手动重置当前连击进度。
    /// </summary>
    public void ResetCombo()
    {
        ClearComboState(IsComboActive);
    }

    /// <summary>
    /// 处理连招输入，检查连招状态并执行相应的连招步骤
    /// </summary>
    /// <param name="input">连招输入信息</param>
    /// <returns>是否成功处理输入并开始连招步骤</returns>
    private bool HandleInput(ComboInput input)
    {
        // 检查是否可以使用连招
        if (!CanUseCombo())
        {
            return false;
        }

        // 如果处于最终恢复状态，则无法处理输入
        if (state == ComboState.FinalRecovery)
        {
            return false;
        }

        // 如果处于施法状态，则缓冲输入
        if (state == ComboState.Casting)
        {
            return BufferInput(input);
        }

        // 如果处于等待连招继续状态
        if (state == ComboState.WaitingForContinuation)
        {
            // 检查是否超过连招继续时间窗口
            if (Time.time >= continuationExpireTime)
            {
                ClearComboState(true);
                return StartStep(0, input);
            }

            // 获取当前连招步骤
            SkillComboStep currentStep = comboDefinition.GetStep(currentStepIndex);
            // 如果当前步骤需要命中但未命中，且不是最后一步，则缓冲输入
            if (currentStep != null && currentStep.RequireHit && !activeStepHit)
            {
                return IsFinalStep(currentStepIndex) ? false : BufferInput(input);
            }

            // 开始下一步连招
            return StartStep(currentStepIndex + 1, input);
        }

        // 默认从第一步开始连招
        return StartStep(0, input);
    }

    /// <summary>
    /// 开始执行连招步骤
    /// </summary>
    /// <param name="stepIndex">步骤索引</param>
    /// <param name="input">连招输入信息</param>
    /// <returns>是否成功开始连招步骤</returns>
    private bool StartStep(int stepIndex, ComboInput input)
    {
        // 获取指定步骤
        SkillComboStep step = comboDefinition.GetStep(stepIndex);
        // 如果步骤无效或技能无效，则清除连招状态
        if (step == null || step.Skill == null)
        {
            ClearComboState(IsComboActive);
            return false;
        }

        // 清除缓冲的输入
        ClearBufferedInput();
        submittingStep = true;
        submittingStepIndex = stepIndex;

        // 尝试施放技能
        bool castStarted = skillCaster.TryCast(CreateCastRequest(step.Skill, input), out SkillCastSession session);

        // 重置提交状态
        submittingStep = false;
        submittingStepIndex = -1;

        // 如果未成功开始施法，则清除连招状态
        if (!castStarted)
        {
            ClearComboState(IsComboActive);
            return false;
        }

        // 如果当前会话不是活动会话，则开始步骤
        if (activeSession != session)
        {
            BeginStep(stepIndex, step, session);
        }

        return true;
    }

    /// <summary>
    /// 开始执行连招步骤的具体实现
    /// </summary>
    /// <param name="stepIndex">步骤索引</param>
    /// <param name="step">连招步骤</param>
    /// <param name="session">技能施放会话</param>
    private void BeginStep(int stepIndex, SkillComboStep step, SkillCastSession session)
    {
        // 检查是否是连招开始（初始状态且第一步）
        bool comboStarted = state == ComboState.Idle && stepIndex == 0;

        // 更新活动会话和当前步骤索引
        activeSession = session;
        currentStepIndex = stepIndex;
        state = ComboState.Casting;
        activeCastCompleted = session.IsCompleted;
        activeStepHit = session.HasHit;
        continuationExpireTime = 0f;

        // 触发动画器
        if (animator != null && !string.IsNullOrWhiteSpace(step.AnimatorTrigger))
        {
            animator.SetTrigger(Animator.StringToHash(step.AnimatorTrigger));
        }

        // 如果是连招开始，触发连招开始事件
        if (comboStarted)
        {
            OnComboStarted?.Invoke(comboDefinition);
        }

        // 触发步骤开始事件
        OnStepStarted?.Invoke(stepIndex, step.Skill);

        // 如果施法已完成，打开连招继续时间窗口
        if (activeCastCompleted)
        {
            OpenContinuationWindow();
        }
    }

    /// <summary>
    /// 打开连续技能窗口，处理连招的下一步操作
    /// </summary>
    private void OpenContinuationWindow()
    {
        // 获取当前步骤
        SkillComboStep step = comboDefinition != null ? comboDefinition.GetStep(currentStepIndex) : null;
        if (step == null)
        {
            // 如果没有步骤，清除连招状态
            ClearComboState(true);
            return;
        }

        // 设置连招过期时间
        if (continuationExpireTime <= 0f)
        {
            continuationExpireTime = Time.time + step.NextStepWindow;
        }

        // 设置为等待连续状态
        state = ComboState.WaitingForContinuation;
        // 检查是否需要命中且未命中
        if (step.RequireHit && !activeStepHit)
        {
            // 如果没有连续窗口，清除连招状态
            if (step.NextStepWindow <= 0f)
            {
                ClearComboState(true);
            }

            return;
        }

        // 检查是否为最后一步
        if (IsFinalStep(currentStepIndex))
        {
            ClearBufferedInput();
            state = ComboState.FinalRecovery;
            // 触发连招完成事件
            OnComboCompleted?.Invoke(comboDefinition);

            // 如果没有连续窗口，清除连招状态
            if (step.NextStepWindow <= 0f)
            {
                ClearComboState(false);
            }

            return;
        }

        // 尝试使用缓冲的输入
        if (TryConsumeBufferedInput())
        {
            return;
        }

        // 如果没有连续窗口，清除连招状态
        if (step.NextStepWindow <= 0f)
        {
            ClearComboState(true);
        }
    }

    /// <summary>
    /// 缓冲输入
    /// </summary>
    /// <param name="input">要缓冲的输入</param>
    /// <returns>是否成功缓冲输入</returns>
    private bool BufferInput(ComboInput input)
    {
        float duration = comboDefinition != null ? comboDefinition.InputBufferDuration : 0f;
        if (duration <= 0f)
        {
            return false;
        }

        // 设置缓冲输入及其过期时间
        bufferedInput = input;
        bufferedInputExpireTime = Time.time + duration;
        hasBufferedInput = true;
        return true;
    }

    /// <summary>
    /// 尝试消耗缓冲的输入
    /// </summary>
    /// <returns>是否成功消耗并开始下一步</returns>
    private bool TryConsumeBufferedInput()
    {
        if (!HasBufferedInput)
        {
            ClearBufferedInput();
            return false;
        }

        // 获取并清除缓冲输入
        ComboInput input = bufferedInput;
        ClearBufferedInput();
        return StartStep(currentStepIndex + 1, input);
    }

    /// <summary>
    /// 清除缓冲的输入
    /// </summary>
    private void ClearBufferedInput()
    {
        hasBufferedInput = false;
        bufferedInputExpireTime = 0f;
        bufferedInput = default;
    }

    /// <summary>
    /// 创建技能施放请求
    /// </summary>
    /// <param name="skill">技能定义</param>
    /// <param name="input">连招输入</param>
    /// <returns>技能施放请求对象</returns>
    private SkillCastRequest CreateCastRequest(SkillDefinition skill, ComboInput input)
    {
        Vector3 origin = skillCaster.GetOriginPosition();
        Vector3 direction;
        Vector3 point;

        //根据输入模式确定方向和目标点
        switch (input.Mode)
        {
            case ComboInputMode.Point:
                point = input.Point;
                direction = SafeDirection(point - origin);
                break;
            case ComboInputMode.Direction:
                direction = SafeDirection(input.Direction);
                point = origin + direction * skill.Range;
                break;
            default:
                point = input.Target != null ? input.Target.transform.position : origin;
                direction = input.Target != null ? SafeDirection(point - origin) : transform.forward;
                break;
        }

        return new SkillCastRequest
        {
            Skill = skill,
            Target = input.Target,
            Point = point,
            Direction = direction,
            SourceInstanceId = 0
        };
    }

    /// <summary>
    /// 安全归一化方向向量
    /// </summary>
    /// <param name="direction">要归一化的方向向量</param>
    /// <returns>归一化后的方向向量</returns>
    private Vector3 SafeDirection(Vector3 direction)
    {
        return direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
    }

    /// <summary>
    /// 检查是否可以使用连招
    /// </summary>
    /// <returns>是否可以使用连招</returns>
    private bool CanUseCombo()
    {
        return skillCaster != null && comboDefinition != null && comboDefinition.StepCount > 0;
    }

    /// <summary>
    /// 检查是否为最后一步
    /// </summary>
    /// <param name="stepIndex">步骤索引</param>
    /// <returns>是否为最后一步</returns>
    private bool IsFinalStep(int stepIndex)
    {
        return comboDefinition != null && stepIndex == comboDefinition.StepCount - 1;
    }

    /// <summary>
    /// 解析依赖关系
    /// </summary>
    private void ResolveDependencies()
    {
        if (skillCaster == null)
        {
            skillCaster = GetComponent<SkillCaster>();
        }
    }

    /// <summary>
    /// 注册连招技能
    /// </summary>
    private void RegisterComboSkills()
    {
        if (skillCaster == null || comboDefinition == null)
        {
            return;
        }

        // 注册所有步骤中的技能
        for (int i = 0; i < comboDefinition.StepCount; i++)
        {
            SkillComboStep step = comboDefinition.GetStep(i);
            if (step != null && step.Skill != null)
            {
                skillCaster.AddSkill(step.Skill);
            }
        }
    }

    /// <summary>
    /// 订阅施法器事件
    /// </summary>
    private void SubscribeCasterEvents()
    {
        if (skillCaster == null)
        {
            return;
        }

        // 先取消订阅再重新订阅，确保不会重复订阅
        skillCaster.OnCastSessionStarted -= HandleCastSessionStarted;
        skillCaster.OnCastSessionCompleted -= HandleCastSessionCompleted;
        skillCaster.OnSkillHit -= HandleSkillHit;
        skillCaster.OnCastSessionStarted += HandleCastSessionStarted;
        skillCaster.OnCastSessionCompleted += HandleCastSessionCompleted;
        skillCaster.OnSkillHit += HandleSkillHit;
    }

    /// <summary>
    /// 取消订阅施法器事件
    /// </summary>
    private void UnsubscribeCasterEvents()
    {
        if (skillCaster == null)
        {
            return;
        }

        skillCaster.OnCastSessionStarted -= HandleCastSessionStarted;
        skillCaster.OnCastSessionCompleted -= HandleCastSessionCompleted;
        skillCaster.OnSkillHit -= HandleSkillHit;
    }

    /// <summary>
    /// 处理施法会话开始事件
    /// </summary>
    /// <param name="session">施法会话</param>
    private void HandleCastSessionStarted(SkillCastSession session)
    {
        // 处理正在提交的步骤
        if (submittingStep)
        {
            SkillComboStep step = comboDefinition.GetStep(submittingStepIndex);
            if (step != null && step.Skill == session.Skill)
            {
                BeginStep(submittingStepIndex, step, session);
            }

            return;
        }

        // 如果连招处于活动状态，清除连招状态
        if (IsComboActive)
        {
            ClearComboState(true);
        }
    }

    /// <summary>
    /// 处理施法会话完成事件
    /// </summary>
    /// <param name="session">施法会话</param>
    private void HandleCastSessionCompleted(SkillCastSession session)
    {
        if (session != activeSession)
        {
            return;
        }

        // 标记施法完成并设置命中状态
        activeCastCompleted = true;
        activeStepHit = session.HasHit;
        // 打开连续窗口
        OpenContinuationWindow();
    }

    /// <summary>
    /// 处理技能命中事件
    /// </summary>
    /// <param name="session">施法会话</param>
    /// <param name="target">目标对象</param>
    private void HandleSkillHit(SkillCastSession session, GameObject target)
    {
        if (session != activeSession)
        {
            return;
        }

        // 设置命中状态并触发步骤命中事件
        activeStepHit = true;
        OnStepHit?.Invoke(currentStepIndex, target);

        SkillComboStep step = comboDefinition.GetStep(currentStepIndex);
        // 如果施法完成且需要命中，检查是否打开连续窗口
        if (activeCastCompleted && step != null && step.RequireHit)
        {
            if (Time.time >= continuationExpireTime)
            {
                ClearComboState(true);
            }
            else
            {
                OpenContinuationWindow();
            }
        }
    }

    /// <summary>
    /// 清除连招状态
    /// </summary>
    /// <param name="raiseResetEvent">是否触发重置事件</param>
    private void ClearComboState(bool raiseResetEvent)
    {
        bool wasActive = IsComboActive;

        // 重置所有连招相关状态
        state = ComboState.Idle;
        activeSession = null;
        currentStepIndex = -1;
        submittingStepIndex = -1;
        submittingStep = false;
        activeCastCompleted = false;
        activeStepHit = false;
        continuationExpireTime = 0f;
        ClearBufferedInput();

        // 如果需要且之前处于活动状态，触发重置事件
        if (raiseResetEvent && wasActive)
        {
            OnComboReset?.Invoke(comboDefinition);
        }
    }
}