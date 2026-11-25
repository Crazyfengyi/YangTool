using BehaviorDesigner.Runtime;
using DataStruct;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 通过中文面板对照技能调度树的两种执行路径
/// </summary>
public class SkillFlowDemoController : MonoBehaviour
{
    [SerializeField, LabelText("施法角色")] private SkillFlowDemoRole caster; // 施法入口
    [SerializeField, LabelText("固定靶子")] private SkillFlowDemoRole target; // 伤害观察目标
    [SerializeField, LabelText("时间线配置")] private SkillFlowDemoDefinition timelineDefinition; // Timeline 参数
    [SerializeField, LabelText("行为树配置")] private SkillFlowDemoDefinition behaviorDefinition; // 行为树参数
    [SerializeField, LabelText("备用行为树配置")] private SkillFlowDemoDefinition alternateBehaviorDefinition; // 第二种技能树
    [SerializeField] private Button timelineButton; // 时间线测试按钮
    [SerializeField] private Button behaviorButton; // 行为树测试按钮
    [SerializeField] private Button interruptButton; // 打断按钮
    [SerializeField] private Button resetButton; // 重置按钮
    [SerializeField] private TMP_Text statusText; // 当前阶段文本
    [SerializeField] private TMP_Text healthText; // 靶子血量文本
    private Vector3 casterPosition, targetPosition; // 重置位置
    private Quaternion casterRotation, targetRotation; // 重置朝向
    private bool pending; // 是否正在观察一次施放
    private string mode = "尚未选择"; // 当前测试方式
    private string stage = "待执行"; // 当前或最近一次执行阶段
    private int hitsBeforeCast; // 本次施法前的命中计数
    private int shownHits = -1; // 上次显示的命中计数
    private RunTimeSkillData timelineData, behaviorData, alternateData; // 持续复用的测试技能
    private bool alternateNext; // 下次行为树按钮使用的配置
    private float nextPanelRefresh; // 冷却显示刷新时间
    private string shownStage; // 上次显示的阶段

    /// <summary>
    /// 初始化专用角色并绑定场景中已配置的按钮
    /// </summary>
    private void Start()
    {
        caster.IInit();
        target.IInit();
        timelineData = timelineDefinition.CreateRuntimeData();
        behaviorData = behaviorDefinition.CreateRuntimeData();
        alternateData = alternateBehaviorDefinition.CreateRuntimeData();
        caster.SkillControl.RegisterSkills(new[] { timelineData, behaviorData, alternateData });
        casterPosition = caster.transform.position;
        targetPosition = target.transform.position;
        casterRotation = caster.transform.rotation;
        targetRotation = target.transform.rotation;
        timelineButton.onClick.AddListener(CastTimeline);
        behaviorButton.onClick.AddListener(CastBehavior);
        interruptButton.onClick.AddListener(Interrupt);
        resetButton.onClick.AddListener(ResetTest);
        RefreshPanel();
    }

    /// <summary>
    /// 显示技能请求 实际执行与实际完成状态
    /// </summary>
    private void Update()
    {
        if (pending)
        {
            if (!caster.SkillControl.NeedUseSkill)
            {
                pending = false;
                stage = DescribeResult(caster.SkillControl.LastResult);
                Debug.Log($"[技能流程测试] {mode} 结束 本次命中 {target.ReceivedHits - hitsBeforeCast}", this);
            }
            else stage = target.ReceivedHits > hitsBeforeCast ? "命中 / 收招中"
                : caster.SkillControl.IsExecuting ? "执行中 / 等待伤害检测" : "待执行 / AI调度中";
        }
        if (shownHits != target.ReceivedHits || shownStage != stage || Time.unscaledTime >= nextPanelRefresh)
            RefreshPanel();
    }

    /// <summary>
    /// 提交时间线技能并启动 AI 调度树
    /// </summary>
    public void CastTimeline()
    {
        Cast(timelineData, "Timeline");
    }

    /// <summary>
    /// 提交行为树技能并启动同一 AI 调度树
    /// </summary>
    public void CastBehavior()
    {
        if (Cast(alternateNext ? alternateData : behaviorData, alternateNext ? "行为树 B" : "行为树 A"))
            alternateNext = !alternateNext;
    }

    /// <summary>
    /// 同一时间只允许提交一个测试技能
    /// </summary>
    private bool Cast(RunTimeSkillData data, string modeName)
    {
        if (pending || caster.SkillControl.NeedUseSkill) return false;
        mode = modeName;
        hitsBeforeCast = target.ReceivedHits;
        if (!caster.SkillControl.TryUseSkill(data, target.gameObject, out SkillUseFailReason reason))
        {
            stage = DescribeFailure(reason);
            RefreshPanel();
            return false;
        }
        pending = true;
        stage = "待执行 / AI调度中";
        caster.AIBehavior.EnableBehavior();
        Debug.Log($"[技能流程测试] 提交 {mode} 技能", this);
        RefreshPanel();
        return true;
    }

    /// <summary>
    /// 停止实际技能与本轮调度 保留命中结果用于观察
    /// </summary>
    public void Interrupt()
    {
        bool wasPending = pending;
        pending = false;
        caster.SkillControl.BreakSkill();
        caster.AIBehavior.DisableBehavior(false);
        if (wasPending)
        {
            stage = "打断";
            Debug.Log($"[技能流程测试] 打断 {mode}", this);
        }
        RefreshPanel();
    }

    /// <summary>
    /// 还原位置 生命与计数 后续可以重新施放
    /// </summary>
    public void ResetTest()
    {
        Interrupt();
        caster.transform.SetPositionAndRotation(casterPosition, casterRotation);
        target.transform.SetPositionAndRotation(targetPosition, targetRotation);
        caster.SkillControl.Reset();
        alternateNext = false;
        caster.ResetHealth();
        target.ResetHealth();
        hitsBeforeCast = 0;
        mode = "尚未选择";
        stage = "待执行";
        RefreshPanel();
    }

    /// <summary>
    /// 同步文字与按钮状态
    /// </summary>
    private void RefreshPanel()
    {
        statusText.text = $"执行方式  {mode}\n当前阶段  {stage}\n\nSkillControl → AI调度树\n→ Timeline / 技能行为树\n→ 伤害检测 → SkillEnd";
        healthText.text = $"靶子生命  {target.CurrentHealth:0}\n累计命中  {target.ReceivedHits}    公共间隔  {caster.SkillControl.CommonCooldownRemaining:0.0}s";
        timelineButton.interactable = !pending;
        behaviorButton.interactable = !pending;
        interruptButton.interactable = pending;
        shownHits = target.ReceivedHits;
        shownStage = stage;
        nextPanelRefresh = Time.unscaledTime + 0.1f;
    }

    /// <summary>
    /// 把统一结果转换为面板状态
    /// </summary>
    private static string DescribeResult(SkillExecutionResult result)
    {
        if (result.Result == SkillRunResult.Completed) return "结束";
        if (result.Result == SkillRunResult.Interrupted) return "打断";
        return (result.Result == SkillRunResult.Cancelled ? "取消 / " : "失败 / ") + DescribeFailure(result.Reason);
    }

    /// <summary>
    /// 显示可供测试定位的中文拒绝原因
    /// </summary>
    private static string DescribeFailure(SkillUseFailReason reason)
    {
        switch (reason)
        {
            case SkillUseFailReason.InvalidRole: return "角色不可施法";
            case SkillUseFailReason.InvalidTarget: return "目标失效";
            case SkillUseFailReason.OutOfRange: return "目标超出距离";
            case SkillUseFailReason.OnCooldown: return "技能冷却中";
            case SkillUseFailReason.OnCommonCooldown: return "公共间隔中";
            case SkillUseFailReason.MissingResource: return "技能资源缺失";
            case SkillUseFailReason.MissingBinding: return "轨道绑定缺失";
            case SkillUseFailReason.RequestTimeout: return "调度等待超时";
            case SkillUseFailReason.InvalidExecutor: return "执行器配置无效";
            case SkillUseFailReason.InvalidSkill: return "技能配置无效";
            case SkillUseFailReason.Busy: return "正在施放技能";
            case SkillUseFailReason.ExecutionFailed: return "技能执行失败";
            default: return "请求结束";
        }
    }

    /// <summary>
    /// 结束演示时解除界面事件并停止技能
    /// </summary>
    private void OnDestroy()
    {
        if (timelineButton != null) timelineButton.onClick.RemoveListener(CastTimeline);
        if (behaviorButton != null) behaviorButton.onClick.RemoveListener(CastBehavior);
        if (interruptButton != null) interruptButton.onClick.RemoveListener(Interrupt);
        if (resetButton != null) resetButton.onClick.RemoveListener(ResetTest);
        if (caster != null && caster.SkillControl != null) caster.SkillControl.BreakSkill();
    }
}
