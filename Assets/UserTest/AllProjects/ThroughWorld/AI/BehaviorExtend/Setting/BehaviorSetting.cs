using System.Runtime.CompilerServices;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 保存旧扩展的节点上下文及通用失败处理
/// </summary>
public static class BehaviorSetting
{
    public const string path = "Assets/UserTest/AllProjects/ThroughWorld/AI/BehaviorExtend/Gizmos/"; // 项目现有图标目录
    private static readonly ConditionalWeakTable<RoleBase, Points> records = new(); // 按角色隔离记录
    private static readonly ConditionalWeakTable<Task, object> diagnostics = new(); // 每个节点仅报告一次缺项

    /// <summary>
    /// 保存供同角色多个节点共享的记录
    /// </summary>
    internal sealed class Points
    {
        public Vector3? Position; // 普通记录点
        public Vector3? Attack; // 攻击记录点
        public Vector3? Direction; // 初始朝向记录
    }

    /// <summary>
    /// 取得角色的节点记录
    /// </summary>
    internal static Points Record(RoleBase role) => records.GetValue(role, _ => new Points());

    /// <summary>
    /// 将条件转换为行为树结果
    /// </summary>
    internal static TaskStatus Result(bool value) => value ? TaskStatus.Success : TaskStatus.Failure;

    /// <summary>
    /// 报告旧系统接口缺失并返回失败
    /// </summary>
    internal static TaskStatus Unsupported(Task task, string reason)
    {
        if (!diagnostics.TryGetValue(task, out _))
        {
            diagnostics.Add(task, new object());
            Debug.LogWarning($"[ThroughWorld行为树] {task.GetType().Name}: {reason}", task.Owner);
        }
        return TaskStatus.Failure;
    }

    /// <summary>
    /// 取得角色模型朝向根
    /// </summary>
    internal static Transform Root(RoleBase role) => role != null && role.ModelInfo != null ? role.ModelInfo.Root : null;

    /// <summary>
    /// 转到水平目标方向
    /// </summary>
    internal static TaskStatus Face(Transform root, Vector3 direction, float speed, bool immediate = false)
    {
        if (root == null) return TaskStatus.Failure;
        direction.y = 0;
        if (direction.sqrMagnitude < 0.000001f) return TaskStatus.Success;
        direction.Normalize();
        if (immediate || Vector3.Angle(root.forward, direction) <= 1f)
        {
            root.rotation = Quaternion.LookRotation(direction);
            return TaskStatus.Success;
        }
        if (!(speed > 0) || float.IsInfinity(speed)) return TaskStatus.Failure;
        root.rotation = Quaternion.RotateTowards(root.rotation, Quaternion.LookRotation(direction), speed * Mathf.Rad2Deg * Time.deltaTime);
        return TaskStatus.Running;
    }

    /// <summary>
    /// 判断当前技能目标在水平范围内
    /// </summary>
    internal static bool InSkillRange(RoleBase role)
    {
        if (role == null || role.Target == null || role.SkillControl?.CurrentRunTimeSkillData?.skill == null) return false;
        Vector3 offset = role.Target.transform.position - role.transform.position;
        offset.y = 0;
        float range = Mathf.Max(0, role.SkillControl.CurrentRunTimeSkillData.skill.AtkRang);
        return offset.sqrMagnitude <= range * range;
    }

    /// <summary>
    /// 校验资源和模型挂点后调用角色特效入口
    /// </summary>
    internal static TaskStatus PlayRoleEffect(Task task, RoleBase role, string effectName, ModelPointType point, bool follow)
    {
        if (Root(role) == null || string.IsNullOrWhiteSpace(effectName)) return TaskStatus.Failure;
        if (role.ModelInfo.GetEffectPoint(point) == null || role.ModelInfo.GetEffectPoint(ModelPointType.Foot) == null)
            return TaskStatus.Failure;
        if (GameResourceManager.Instance.ResourceLoad($"Effects/{effectName}") == null)
            return Unsupported(task, $"缺少特效资源 {effectName}");

        return Result((follow ? role.PlayEffectAtSelf(effectName, point) : role.PlayEffect(effectName, point)) != null);
    }

    /// <summary>
    /// 按配置比较数值 百分比模式使用调用方提供的归一化值
    /// </summary>
    internal static bool Compare(float value, float threshold, MathCompare mode)
    {
        switch (mode)
        {
            case MathCompare.小于:
            case MathCompare.百分比小于: return value < threshold;
            case MathCompare.等于:
            case MathCompare.百分比等于: return Mathf.Approximately(value, threshold);
            case MathCompare.大于:
            case MathCompare.百分比大于: return value > threshold;
            case MathCompare.小于等于:
            case MathCompare.百分比小于等于: return value <= threshold;
            case MathCompare.大于等于:
            case MathCompare.百分比大于等于: return value >= threshold;
            default: return false;
        }
    }
}
