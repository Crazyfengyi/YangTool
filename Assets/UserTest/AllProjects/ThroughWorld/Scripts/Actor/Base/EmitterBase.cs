/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-08-05 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using DataStruct;

/// <summary>
/// 发射器基类
/// </summary>
public abstract class EmitterBase
{
    /// <summary>
    /// 持有者
    /// </summary>
    public GameActor handle;
    /// <summary>
    /// 发射信息
    /// </summary>
    public EmitData emitData;

    protected int shootCount;//发射次数
    protected float timer;
    protected bool startShoot;//开始发射

    protected Action<BulletBase, EmitterBase> ShootEvent { get; set; }
    public EmitterBase(GameActor _actor)
    {
        handle = _actor;
    }

    /// <summary>
    /// 开始发射
    /// </summary>
    public virtual void StartShoot(Action<BulletBase, EmitterBase> callBack)
    {
        startShoot = true;
        ShootEvent = callBack;
    }

    public virtual void OnUpdate()
    {
        if (!startShoot) return;
        if (emitData == null) return;
        timer += Time.deltaTime;
        if (timer >= emitData.timeInterval)
        {
            timer = 0;
            Shoot();
            shootCount++;
            if (shootCount >= emitData.loopCount)
            {
                startShoot = false;
            }
        }
    }

    /// <summary>
    /// 发射
    /// </summary>
    public virtual void Shoot()
    {
        BulletData bulletData = null;// GetBulletData(emitData.bulletID);

        switch (emitData.bulletShootType)
        {
            case BulletShootType.Circle:
                {
                    float angle = 360 / emitData.bulletCount;
                    Vector3 startDirection = ((PlayerController)handle).model.transform.forward;
                    for (int i = 0; i < emitData.bulletCount; i++)
                    {
                        Vector3 temp = Quaternion.AngleAxis(angle * i, Vector3.up) * startDirection;
                        bulletData.direction = temp;
                        bulletData.targetCampType = handle.canAtkCamp;
                        BulletBase bullet = GameProjectileManager.Instance.CreateBullet(bulletData);
                        ShootEvent?.Invoke(bullet, this);
                    }
                }
                break;
            default:
                {
                    BulletBase bullet = GameProjectileManager.Instance.CreateBullet(bulletData);
                    ShootEvent?.Invoke(bullet, this);
                }
                break;
        }
    }

    /// <summary>
    /// 设置发射信息
    /// </summary>
    public virtual void SetEmitData(EmitData _emitData)
    {
        emitData = _emitData;
    }
}

/// <summary>
/// 发射信息
/// </summary>
public class EmitData
{
    /// <summary>
    /// 子弹ID
    /// </summary>
    public int bulletID;

    /// <summary>
    /// 是否循环
    /// </summary>
    public bool isLoop = false;

    /// <summary>
    /// 发射次数
    /// </summary>
    public int loopCount = 1;

    /// <summary>
    /// 时间间隔
    /// </summary>
    public float timeInterval = 1f;

    /// <summary>
    /// 生成子弹数量
    /// </summary>
    public float bulletCount;

    /// <summary>
    /// 角度
    /// </summary>
    public float angle;

    /// <summary>
    /// 子弹发射类型
    /// </summary>
    public BulletShootType bulletShootType;
}