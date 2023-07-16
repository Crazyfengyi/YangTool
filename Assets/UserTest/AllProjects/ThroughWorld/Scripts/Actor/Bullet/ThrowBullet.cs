/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-03-20
*/

using UnityEngine;
using YangTools;

/// <summary>
/// 投掷子弹
/// </summary>
public class ThrowBullet : BulletBase
{
    /// <summary>
    /// 初速度
    /// </summary>
    public Vector3 initialVelocity;
    /// <summary>
    /// 加速度
    /// </summary>
    public Vector3 aVelocity;
    /// <summary>
    /// 当前速度
    /// </summary>
    public Vector3 currentVelocity;

    public ThrowBullet(BulletData data, GameObject Obj) : base(data, Obj)
    {
        initialVelocity = data.initialVelocity;
        aVelocity = data.aVelocity;
        radius = data.checkRadius;

        currentVelocity = initialVelocity;
    }

    public override void OnUpdate()
    {
        if (bulletData == null || bulletObj == null) return;

        //移动
        currentVelocity += aVelocity * Time.deltaTime;
        Vector3 tempPos = bulletObj.transform.position + currentVelocity * Time.deltaTime;
        bulletObj.transform.position = tempPos;

        CheckAllCollision();
    }
}