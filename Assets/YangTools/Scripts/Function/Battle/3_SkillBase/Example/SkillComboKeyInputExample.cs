using UnityEngine;

/// <summary>
/// 使用旧输入系统触发技能连击的最小示例
/// </summary>
public class SkillComboKeyInputExample : MonoBehaviour
{
    // 接收示例按键的连招控制器
    [SerializeField] private SkillComboController comboController;
    // 提交一次连招输入的测试按键
    [SerializeField] private KeyCode triggerKey = KeyCode.Mouse0;
    // 示例操作的目标对象
    [SerializeField] private GameObject target;
    // 是否在同级层级自动寻找测试目标
    [SerializeField] private bool autoFindTarget = true;
    // 是否记录连招事件测试日志
    [SerializeField] private bool logComboEvents = true;

    /// <summary>
    /// 在编辑器重置时取得同物体的连招控制器
    /// </summary>
    private void Reset()
    {
        comboController = GetComponent<SkillComboController>();
    }

    /// <summary>
    /// 解析示例引用并订阅连招日志事件
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();
        SubscribeComboEvents();
    }

    /// <summary>
    /// 取消连招日志事件订阅
    /// </summary>
    private void OnDisable()
    {
        UnsubscribeComboEvents();
    }

    /// <summary>
    /// 读取配置按键并向目标或角色前方提交一次连招输入
    /// </summary>
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
    /// 解析连击控制器和同级测试目标
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
    /// 订阅连击测试日志事件
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
    /// 取消订阅连击测试日志事件
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

    /// <summary>
    /// 记录当前连招段开始的测试日志
    /// </summary>
    /// <param name="stepIndex">从零开始的连招步骤索引</param>
    /// <param name="skill">本次操作的技能定义</param>
    private void HandleStepStarted(int stepIndex, SkillDefinition skill)
    {
        Debug.Log($"[连击测试] 第 {stepIndex + 1} 段开始：{skill?.name}", this);
    }

    /// <summary>
    /// 记录当前连招段实际命中的测试日志
    /// </summary>
    /// <param name="stepIndex">从零开始的连招步骤索引</param>
    /// <param name="hitTarget">本段实际命中的目标</param>
    private void HandleStepHit(int stepIndex, GameObject hitTarget)
    {
        Debug.Log($"[连击测试] 第 {stepIndex + 1} 段命中：{hitTarget?.name}", this);
    }

    /// <summary>
    /// 记录连招完成的测试日志
    /// </summary>
    /// <param name="combo">本次连招定义</param>
    private void HandleComboCompleted(SkillComboDefinition combo)
    {
        Debug.Log($"[连击测试] 连击完成：{combo?.name}", this);
    }

    /// <summary>
    /// 记录连招重置的测试日志
    /// </summary>
    /// <param name="combo">本次连招定义</param>
    private void HandleComboReset(SkillComboDefinition combo)
    {
        Debug.Log($"[连击测试] 连击重置：{combo?.name}", this);
    }
}
