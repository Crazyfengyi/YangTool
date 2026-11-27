using System;
using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;
using YangTools.Scripts.Core.YangExtend;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

/// <summary>
/// 播放角色动画并在配置时间执行事件
/// </summary>
[TaskIcon(BehaviorSetting.path + "AniAndEventIcon.png")]
[TaskCategory("RoleAI/Animator")]
[TaskDescription("播放动画并触发事件 同一时间点后配置的事件先执行")]
public class PlayAnimation : Action
{
    private static readonly int AttackSpeedParameter = Animator.StringToHash("atkSpeedAffect"); // Animator 攻速参数
    public string aniName; // 动画名称
    public bool ExitImmediately; // 进入状态后立即结束任务
    public AtkSpeedDepend depend; // 攻速换算方式
    public float dependPercent; // 攻速换算比例
    public float dependValue; // 固定时长
    public List<AniEventInfo> aniEventInfos = new List<AniEventInfo>(); // 动画事件配置

    private Animator animator; // 本轮动画组件
    private float timer; // 已播放时间
    private float duration; // 本轮播放时长
    private float previousSpeed; // 原 Animator 攻速参数
    private bool changedSpeed; // 本轮是否写入参数
    private bool valid; // 本轮配置是否有效
    private bool started; // 是否已请求播放
    private int stateHash; // 本轮 Animator 状态
    private float crossFade; // 本轮动画过渡时长

    #region 播放与结束

    /// <summary>
    /// 校验动画状态 攻速配置和事件绑定
    /// </summary>
    public override void OnStart()
    {
        timer = 0;
        valid = false;
        started = false;
        changedSpeed = false;
        var role = GetComponent<RoleBase>();
        animator = role != null ? role.Animator : null;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;

        if (aniName == "None")
        {
            valid = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(aniName)) return;
        stateHash = Animator.StringToHash($"{animator.GetLayerName(0)}.{aniName}");
        if (!animator.HasState(0, stateHash)) stateHash = Animator.StringToHash(aniName);
        if (!animator.HasState(0, stateHash)) return;

        var clip = animator.GetAnimClipByName(aniName);
        if (clip == null || !(clip.length > 0))
        {
            if (!ExitImmediately || depend != AtkSpeedDepend.None || (aniEventInfos?.Count ?? 0) != 0) return;
            crossFade = Utility.AniCrossTime;
            duration = 1f;
            valid = true;
            return;
        }
        crossFade = Mathf.Min(Utility.AniCrossTime, clip.length);
        duration = clip.length;
        if (depend == AtkSpeedDepend.固定值) duration = dependValue;
        if (depend == AtkSpeedDepend.百分比)
        {
            if (role.roleAttributeControl == null || role.roleAttributeControl.attributesDic == null) return;
            ValueTotal speed = role.GetRoleAttribute(RoleAttribute.AtkSpeed);
            if (speed == null) return;
            duration = dependPercent * speed.Value / (1 + speed.Value - speed.BaseValue);
        }
        if (!(duration > 0) || float.IsInfinity(duration)) return;

        if (aniEventInfos != null)
        {
            foreach (var info in aniEventInfos)
            {
                if (info == null || !Bind(info)) return;
                info.isTrigger = false;
                info.usetimePoint = Mathf.Clamp(info.timePoint * duration / clip.length, 0, duration);
            }
        }

        foreach (var parameter in animator.parameters)
        {
            if (parameter.nameHash != AttackSpeedParameter || parameter.type != AnimatorControllerParameterType.Float) continue;
            previousSpeed = animator.GetFloat(AttackSpeedParameter);
            animator.SetFloat(AttackSpeedParameter, clip.length / duration);
            changedSpeed = true;
            break;
        }
        if (depend != AtkSpeedDepend.None && !changedSpeed) return;
        valid = true;
    }

    /// <summary>
    /// 播放片段并推进事件或等待进入动画状态
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (!valid || animator == null || !animator.isActiveAndEnabled)
            return BehaviorSetting.Unsupported(this, $"动画或事件配置无效 {aniName}");
        if (aniName == "None")
        {
            if (animator.HasState(0, Animator.StringToHash(aniName))) animator.Play(aniName);
            return TaskStatus.Success;
        }

        if (!started)
        {
            started = true;
            if (IsCurrentState()) animator.Play(stateHash, 0, 0);
            else animator.CrossFade(stateHash, crossFade, 0);
        }

        if (ExitImmediately)
        {
            if (IsCurrentState()) return TaskStatus.Success;
            timer += Time.deltaTime;
            return timer > duration + 1f ? TaskStatus.Failure : TaskStatus.Running;
        }
        if (!(animator.speed > 0)) return TaskStatus.Failure;

        timer += Time.deltaTime * animator.speed;
        for (int i = (aniEventInfos?.Count ?? 0) - 1; i >= 0; i--)
        {
            var info = aniEventInfos[i];
            if (info.isTrigger || timer < info.usetimePoint) continue;
            if (info.component == null) return TaskStatus.Failure;
            info.isTrigger = true;
            try { info.InVoke(); }
            catch (TargetInvocationException exception)
            {
                return BehaviorSetting.Unsupported(this, exception.InnerException?.Message ?? exception.Message);
            }
        }
        return timer >= duration ? TaskStatus.Success : TaskStatus.Running;
    }

    /// <summary>
    /// 释放本轮动画事件和攻速状态
    /// </summary>
    public override void OnEnd()
    {
        if (changedSpeed && animator != null) animator.SetFloat(AttackSpeedParameter, previousSpeed);
        changedSpeed = false;
        valid = false;
        if (aniEventInfos == null) return;
        foreach (var info in aniEventInfos)
        {
            if (info == null) continue;
            info.isTrigger = false;
            info.component = null;
            info.methodInfo = null;
            info.parmaArray = null;
        }
    }

    /// <summary>
    /// 清理编辑器中的事件设置
    /// </summary>
    public override void OnReset()
    {
        if (aniEventInfos == null) return;
        foreach (var info in aniEventInfos) info?.OnReset();
    }

    /// <summary>
    /// 比较当前 Animator 状态的完整路径或短名称
    /// </summary>
    private bool IsCurrentState()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);
        return state.fullPathHash == stateHash || state.shortNameHash == stateHash;
    }

    #endregion

    #region 动画事件

    /// <summary>
    /// 为事件解析唯一可调用的组件方法和参数
    /// </summary>
    private bool Bind(AniEventInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.componentName) || string.IsNullOrWhiteSpace(info.methodName)) return false;
        var target = info.targetGameObject != null ? info.targetGameObject : gameObject;
        var targetType = TaskUtility.GetTypeWithinAssembly(info.componentName);
        var component = targetType != null && typeof(Component).IsAssignableFrom(targetType)
            ? target.GetComponent(targetType) : target.GetComponent(info.componentName);
        if (component == null) return false;
        MethodInfo match = null;
        object[] arguments = null;
        var values = string.IsNullOrEmpty(info.parmeterString) ? Array.Empty<string>() : info.parmeterString.Split('|');
        foreach (var candidate in component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (candidate.Name != info.methodName || candidate.ContainsGenericParameters) continue;
            var parameters = candidate.GetParameters();
            if (values.Length > parameters.Length) continue;
            var converted = new object[parameters.Length];
            bool compatible = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (i < values.Length && !string.IsNullOrEmpty(values[i]))
                {
                    if (ReflectionCallMethod.TryConvert(values[i], parameter.ParameterType, out converted[i])) continue;
                }
                else if (parameter.HasDefaultValue)
                {
                    converted[i] = parameter.DefaultValue;
                    continue;
                }
                else if (i < values.Length && parameter.ParameterType == typeof(string))
                {
                    converted[i] = string.Empty;
                    continue;
                }
                compatible = false;
                break;
            }
            if (!compatible) continue;
            if (match != null) return false;
            match = candidate;
            arguments = converted;
        }
        if (match == null) return false;
        info.component = component;
        info.methodInfo = match;
        info.parmaArray = arguments;
        return true;
    }

    #endregion
}

/// <summary>
/// 事件信息
/// </summary>
[Serializable]
public class AniEventInfo
{
    /// <summary>
    /// 触发时间点
    /// </summary>
    public float timePoint;
    [HideInInspector]
    /// <summary>
    /// 实际事件点
    /// </summary>
    public float usetimePoint;
    [HideInInspector]
    /// <summary>
    /// 是否触发
    /// </summary>
    public bool isTrigger;
    [HideInInspector]
    [Tooltip("目标物体")]
    public GameObject targetGameObject;
    [Tooltip("脚本名称")]
    public string componentName = "RoleBase";
    [Tooltip("方法名称")]
    public string methodName;
    [SerializeField]
    /// <summary>
    /// 参数列表
    /// </summary>
    public string parmeterString = "";

    //================Awake提前反射保存方法引用===================
    [HideInInspector]
    //脚本
    public Component component;
    [HideInInspector]
    //方法
    public System.Reflection.MethodInfo methodInfo;
    [HideInInspector]
    //参数
    public object[] parmaArray;

    //调用
    public void InVoke()
    {
        //反射调用
        methodInfo.Invoke(component, parmaArray);
    }

    public void OnReset()
    {
        componentName = null;
        methodName = null;
        parmeterString = "";
    }
}

public enum AtkSpeedDepend
{
    None,
    百分比,
    固定值,
}
