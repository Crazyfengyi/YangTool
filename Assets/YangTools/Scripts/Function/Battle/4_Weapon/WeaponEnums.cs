using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 定义武器类别
/// </summary>
public enum WeaponType
{
    [LabelText("远程")]
    Ranged,
    [LabelText("近战")]
    Melee
}

/// <summary>
/// 定义武器射击的触发方式
/// </summary>
public enum WeaponFireMode
{
    [LabelText("单发")]
    Single,
    [LabelText("自动")]
    Automatic,
    [LabelText("连发")]
    Burst
}

/// <summary>
/// 定义武器可装备的槽位
/// </summary>
public enum WeaponEquipSlot
{
    [LabelText("主武器")]
    Primary,
    [LabelText("副武器")]
    Secondary,
    [LabelText("近战")]
    Melee
}

/// <summary>
/// 说明一次武器射击请求未被接受的原因
/// </summary>
public enum WeaponFireFailReason
{
    None,
    NoWeaponEquipped,
    OnCooldown,
    EmptyClip,
    Reloading,
    InvalidTarget,
    CastRejected
}

/// <summary>
/// 武器运行时状态类
/// </summary>
[Serializable]
public class WeaponRuntimeState
{
    // 当前弹夹中的弹药数量
    public int ClipAmmo;
    // 是否正在重新装弹的状态标志
    public bool IsReloading;
    // 下一次可以射击的时间点
    public float NextFireTime;
    // 待发射的连发射击次数
    public int PendingBurstShots;

    /// <summary>
    /// 初始化武器运行时状态
    /// </summary>
    /// <param name="weapon">武器定义对象，包含武器的基本属性信息</param>
    public void Init(WeaponDefinition weapon)
    {
        // 如果武器定义不为空，则初始化弹夹容量，否则设为0
        ClipAmmo = weapon != null ? Mathf.Max(weapon.ClipSize, 0) : 0;
        // 初始化装弹状态为未装弹
        IsReloading = false;
        // 初始化下次射击时间为0
        NextFireTime = 0f;
        // 初始化待发射连发次数为0
        PendingBurstShots = 0;
    }
}
    
/// <summary>
/// 射击请求结构体
/// </summary>
public readonly struct FireRequest
{
    public readonly GameObject Target; // 目标游戏对象
    public readonly Vector3 Point; // 目标点
    public readonly Vector3 Direction; // 射击方向
    public readonly RequestMode Mode; // 请求模式

    /// <summary>
    /// 保存指定请求模式和瞄准数据
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="point">世界空间目标坐标</param>
    /// <param name="direction">世界空间方向</param>
    /// <param name="mode">目标 坐标或方向瞄准模式</param>
    public FireRequest(GameObject target, Vector3 point, Vector3 direction, RequestMode mode = RequestMode.Target)
    {
        Target = target;
        Point = point;
        Direction = direction;
        Mode = mode;
    }

    /// <summary>
    /// 创建沿指定方向射击的请求
    /// </summary>
    /// <param name="direction">世界空间方向</param>
    /// <param name="point">世界空间目标坐标</param>
    public static FireRequest ForDirection(Vector3 direction, Vector3 point)
    {
        return new FireRequest(null, point, direction, RequestMode.Direction);
    }

    /// <summary>
    /// 创建向世界坐标射击的请求
    /// </summary>
    /// <param name="point">世界空间目标坐标</param>
    /// <param name="direction">世界空间方向</param>
    public static FireRequest ForPoint(Vector3 point, Vector3 direction)
    {
        return new FireRequest(null, point, direction, RequestMode.Point);
    }
}  

/// <summary>
/// 区分武器射击请求的瞄准方式
/// </summary>
public enum RequestMode
{
    Target,
    Point,
    Direction
}