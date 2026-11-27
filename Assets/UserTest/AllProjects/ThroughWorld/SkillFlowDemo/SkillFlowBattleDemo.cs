using BehaviorDesigner.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 在独立场景演示行为树通过 Battle 会话播放技能时间线
/// </summary>
public sealed class SkillFlowBattleDemo : MonoBehaviour
{
    [SerializeField] private BehaviorTree integrationTree; // 整合流程调度树
    [SerializeField] private SkillCaster caster; // Battle 施法器
    [SerializeField] private SkillFlowDemoRole target; // 显示生命的测试目标
    [SerializeField] private Button castButton; // 触发整合技能
    [SerializeField] private TMP_Text label; // 状态文本
    [SerializeField] private Button interruptButton; // 同一面板的打断入口
    [SerializeField] private Button resetButton; // 同一面板的重置入口
    [SerializeField] private SkillFlowDemoRole role; // 旧技能流程状态
    private SkillCastSession current; // 最近一次施法会话

    /// <summary>
    /// 订阅按钮与会话事件
    /// </summary>
    private void Start()
    {
        castButton.onClick.AddListener(Cast);
        caster.OnCastSessionStarted += OnStarted;
        caster.OnCastSessionCompleted += OnCompleted;
        integrationTree.OnBehaviorEnd += OnTreeEnded;
        interruptButton.onClick.AddListener(Interrupt);
        resetButton.onClick.AddListener(ResetDemo);
    }

    /// <summary>
    /// 不并发启动两条 Battle 技能
    /// </summary>
    public void Cast()
    {
        if (caster.IsCasting || role.SkillControl.NeedUseSkill) return;
        current = null;
        label.text = "整合技能 / AI 调度中";
        integrationTree.EnableBehavior();
    }

    /// <summary>
    /// 记录本次真实施法会话
    /// </summary>
    private void OnStarted(SkillCastSession session)
    {
        current = session;
        label.text = "整合技能 / Timeline 执行中";
    }

    /// <summary>
    /// 显示最终结果和实际命中次数
    /// </summary>
    private void OnCompleted(SkillCastSession session)
    {
        if (session != current) return;
        label.text = $"整合技能 / {(session.WasInterrupted ? "打断" : "完成")}\n命中 {session.HitCount}  靶子生命 {target.CurrentHealth:0}";
    }

    /// <summary>
    /// 目标超距或配置无效时显示调度失败
    /// </summary>
    private void OnTreeEnded(Behavior behavior)
    {
        if (current == null && behavior.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Failure)
            label.text = "整合技能 / 请求失败";
    }

    /// <summary>
    /// 中止当前会话 结束事件负责更新状态
    /// </summary>
    private void Interrupt()
    {
        caster.CancelCast();
        integrationTree.DisableBehavior(false);
    }

    /// <summary>
    /// 测试重置同时清理整合技能的冷却
    /// </summary>
    private void ResetDemo()
    {
        Interrupt();
        caster.ResetCooldowns();
        current = null;
        label.text = "整合技能 / 待执行";
    }

    /// <summary>
    /// 移除事件并清理未完成技能
    /// </summary>
    private void OnDestroy()
    {
        if (castButton != null) castButton.onClick.RemoveListener(Cast);
        if (interruptButton != null) interruptButton.onClick.RemoveListener(Interrupt);
        if (resetButton != null) resetButton.onClick.RemoveListener(ResetDemo);
        if (integrationTree != null) integrationTree.OnBehaviorEnd -= OnTreeEnded;
        if (caster != null)
        {
            caster.OnCastSessionStarted -= OnStarted;
            caster.OnCastSessionCompleted -= OnCompleted;
            caster.CancelCast();
        }
    }
}
