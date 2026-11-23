using UnityEngine;

/// <summary>
/// 使用旧输入系统触发技能连击的最小示例。
/// </summary>
public class SkillComboKeyInputExample : MonoBehaviour
{
    [SerializeField] private SkillComboController comboController;
    [SerializeField] private KeyCode triggerKey = KeyCode.Mouse0;
    [SerializeField] private GameObject target;
    [SerializeField] private bool autoFindTarget = true;
    [SerializeField] private bool logComboEvents = true;

    private void Reset()
    {
        comboController = GetComponent<SkillComboController>();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeComboEvents();
    }

    private void OnDisable()
    {
        UnsubscribeComboEvents();
    }

    private void Update()
    {
        if (comboController == null || !Input.GetKeyDown(triggerKey))
        {
            return;
        }

        if (target != null)
        {
            comboController.TryTrigger(target);
        }
        else
        {
            comboController.TryTriggerInDirection(transform.forward);
        }
    }

    /// <summary>
    /// 解析连击控制器和同级测试目标。
    /// </summary>
    private void ResolveReferences()
    {
        if (comboController == null)
        {
            comboController = GetComponent<SkillComboController>();
        }

        if (target != null || !autoFindTarget)
        {
            return;
        }

        Transform searchRoot = transform.parent != null ? transform.parent : transform;
        SimpleHealth[] candidates = searchRoot.GetComponentsInChildren<SimpleHealth>(true);
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] != null && candidates[i].gameObject != gameObject)
            {
                target = candidates[i].gameObject;
                return;
            }
        }
    }

    /// <summary>
    /// 订阅连击测试日志事件。
    /// </summary>
    private void SubscribeComboEvents()
    {
        if (comboController == null || !logComboEvents)
        {
            return;
        }

        comboController.OnStepStarted -= HandleStepStarted;
        comboController.OnStepHit -= HandleStepHit;
        comboController.OnComboCompleted -= HandleComboCompleted;
        comboController.OnComboReset -= HandleComboReset;
        comboController.OnStepStarted += HandleStepStarted;
        comboController.OnStepHit += HandleStepHit;
        comboController.OnComboCompleted += HandleComboCompleted;
        comboController.OnComboReset += HandleComboReset;
    }

    /// <summary>
    /// 取消订阅连击测试日志事件。
    /// </summary>
    private void UnsubscribeComboEvents()
    {
        if (comboController == null)
        {
            return;
        }

        comboController.OnStepStarted -= HandleStepStarted;
        comboController.OnStepHit -= HandleStepHit;
        comboController.OnComboCompleted -= HandleComboCompleted;
        comboController.OnComboReset -= HandleComboReset;
    }

    private void HandleStepStarted(int stepIndex, SkillDefinition skill)
    {
        Debug.Log($"[连击测试] 第 {stepIndex + 1} 段开始：{skill?.name}", this);
    }

    private void HandleStepHit(int stepIndex, GameObject hitTarget)
    {
        Debug.Log($"[连击测试] 第 {stepIndex + 1} 段命中：{hitTarget?.name}", this);
    }

    private void HandleComboCompleted(SkillComboDefinition combo)
    {
        Debug.Log($"[连击测试] 连击完成：{combo?.name}", this);
    }

    private void HandleComboReset(SkillComboDefinition combo)
    {
        Debug.Log($"[连击测试] 连击重置：{combo?.name}", this);
    }
}
