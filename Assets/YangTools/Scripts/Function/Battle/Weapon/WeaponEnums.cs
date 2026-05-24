using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum WeaponType
{
    [LabelText("远程")]
    Ranged,
    [LabelText("近战")]
    Melee
}

public enum WeaponFireMode
{
    [LabelText("单发")]
    Single,
    [LabelText("自动")]
    Automatic,
    [LabelText("连发")]
    Burst
}

public enum WeaponEquipSlot
{
    [LabelText("主武器")]
    Primary,
    [LabelText("副武器")]
    Secondary,
    [LabelText("近战")]
    Melee
}

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
    /// <summary>
    /// 当前弹夹中的弹药数量
    /// </summary>
    public int ClipAmmo;
    /// <summary>
    /// 是否正在重新装弹的状态标志
    /// </summary>
    public bool IsReloading;
    /// <summary>
    /// 下一次可以射击的时间点
    /// </summary>
    public float NextFireTime;
    /// <summary>
    /// 待发射的连发射击次数
    /// </summary>
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

    public FireRequest(GameObject target, Vector3 point, Vector3 direction, RequestMode mode = RequestMode.Target)
    {
        Target = target;
        Point = point;
        Direction = direction;
        Mode = mode;
    }

    public static FireRequest ForDirection(Vector3 direction, Vector3 point)
    {
        return new FireRequest(null, point, direction, RequestMode.Direction);
    }

    public static FireRequest ForPoint(Vector3 point, Vector3 direction)
    {
        return new FireRequest(null, point, direction, RequestMode.Point);
    }
}  

public enum RequestMode
{
    Target,
    Point,
    Direction
}