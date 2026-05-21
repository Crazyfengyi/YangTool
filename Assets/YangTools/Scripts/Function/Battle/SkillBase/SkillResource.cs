using System;
using UnityEngine;

/// <summary>
/// 技能资源类,管理技能所需的资源值
/// </summary>
public class SkillResource : MonoBehaviour
{
    //获取当前资源值
    public float Current => currentResource;
    //获取最大资源值
    public float Max => maxResource;
    
    
    [SerializeField] 
    private float maxResource = 100f;    //最大资源值
    [SerializeField] 
    private float currentResource = 100f; //当前资源值
    public event Action<float, float> OnResourceChanged; //资源变化事件，当资源值改变时触发

    /// <summary>
    /// 是否有足够的资源
    /// </summary>
    public bool HasEnough(float amount)
    {
        return currentResource >= amount;
    }
    /// <summary>
    /// 尝试消耗资源
    /// </summary>
    public bool TryCost(float amount)
    {
        if (amount <= 0f) return true;
        if (!HasEnough(amount)) return false;  //如果资源不足，返回失败

        SetCurrent(currentResource - amount);  //消耗资源
        return true;  // 消耗成功
    }
    /// <summary>
    /// 恢复资源
    /// </summary>
    public void Add(float amount)
    {
        if (amount <= 0f) return;
        SetCurrent(currentResource + amount);  // 恢复资源
    }
    /// <summary>
    /// 设置当前资源值
    /// </summary>
    private void SetCurrent(float value)
    {
        currentResource = Mathf.Clamp(value, 0f, maxResource);  // 确保资源值在0到最大值之间
        OnResourceChanged?.Invoke(currentResource, maxResource);  // 触发资源变化事件
    }
}
