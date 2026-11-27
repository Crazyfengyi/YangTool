using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

/// <summary>
/// 在指定组件上调用具有唯一匹配参数的方法
/// </summary>
[TaskIcon("{SkinColor}ReflectionIcon.png")]
[TaskCategory("RoleAI/Reflection")]
[TaskDescription("在指定组件上调用具有唯一匹配参数的方法")]
public class ReflectionCallMethod : BehaviorDesigner.Runtime.Tasks.Action
{
    public string scriptName = "RoleBase"; // 保留原节点配置
    public string methodName; // 保留原节点配置
    public List<string> parmeterList = new List<string>(); // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Result(TryInvoke());
    }
    /// <summary>
    /// 兼容旧节点的方法调用入口
    /// </summary>
    public void Invoke()
    {
        TryInvoke();
    }
    /// <summary>
    /// 查找唯一签名并执行方法
    /// </summary>
    private bool TryInvoke()
    {
        if (string.IsNullOrWhiteSpace(scriptName) || string.IsNullOrWhiteSpace(methodName)) return false;
        var type = TaskUtility.GetTypeWithinAssembly(scriptName);
        var component = type != null && typeof(Component).IsAssignableFrom(type)
            ? gameObject.GetComponent(type) : gameObject.GetComponent(scriptName);
        if (component == null) return false;
        MethodInfo match = null;
        object[] arguments = null;
        int count = parmeterList?.Count ?? 0;
        foreach (var candidate in component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (candidate.Name != methodName || candidate.ContainsGenericParameters) continue;
            var parameters = candidate.GetParameters();
            if (parameters.Length != count) continue;
            var converted = new object[count];
            bool valid = true;
            for (int i = 0; i < count; i++)
            {
                if (TryConvert(parmeterList[i], parameters[i].ParameterType, out converted[i])) continue;
                valid = false;
                break;
            }
            if (!valid) continue;
            if (match != null) return false;
            match = candidate;
            arguments = converted;
        }
        if (match == null) return false;
        try
        {
            var result = match.Invoke(component, arguments);
            return !(result is bool value) || value;
        }
        catch (TargetInvocationException exception)
        {
            BehaviorSetting.Unsupported(this, exception.InnerException?.Message ?? exception.Message);
            return false;
        }
    }
    /// <summary>
    /// 转换节点参数并拒绝不支持的类型
    /// </summary>
    internal static bool TryConvert(string value, Type type, out object result)
    {
        result = null;
        if (type != typeof(string) && type != typeof(int) && type != typeof(long) &&
            type != typeof(float) && type != typeof(bool) && !type.IsEnum) return false;
        try
        {
            result = type.IsEnum ? Enum.Parse(type, value, true) : Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or OverflowException)
        {
            return false;
        }
    }
}
