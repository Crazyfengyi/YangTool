/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-28 
*/
using UnityEngine;
using System.Collections;
using YangTools;
using System.Collections.Generic;

/// <summary>
/// 抛射体管理器(子弹)
/// </summary>
/// <remarks>Shooter射手---->emitter发射器--(发射信息--子弹id)-->生成子弹bullet</remarks>
public class GameProjectileManager : MonoSingleton<GameProjectileManager>
{
    //所有子弹
    private static List<BulletBase> allBullet = new List<BulletBase>();
    public void Update()
    {
        for (int i = 0; i < allBullet.Count; i++)
        {
            allBullet[i].OnUpdate();
        }
    }
    /// <summary>
    /// 创建子弹
    /// </summary>
    public BulletBase CreateBullet(BulletData bulletData)
    {
        GameObject Obj = GameResourceManager.Instance.ResoruceLoad($"Bullets/BulletTest");
        GameObject bulletObj = GameObject.Instantiate(Obj, bulletData.FromPostion, Quaternion.identity);

        BulletBase bulletBase = new BulletBase(bulletData, bulletObj);
        allBullet.Add(bulletBase);
        return bulletBase;
    }
    public void RemoveBullet(BulletBase bulletBase)
    {
        allBullet.Remove(bulletBase);
    }
}
/// <summary>
/// 发射器基类
/// </summary>
public class EmitterBase
{
    /// <summary>
    /// 发射信息
    /// </summary>
    public EmitData emitData;

    private int shootCount;//发射次数
    private float timer;
    public void OnUpdate()
    {
        if (emitData == null) return;
        if (shootCount > emitData.loopCount) return;
        timer += Time.deltaTime;
        if (timer > emitData.timeInterval)
        {
            timer = 0;
            Shoot();
            shootCount++;
        }
    }
    /// <summary>
    /// 发射
    /// </summary>
    public void Shoot()
    {
        BulletData bulletData = null;// GetBulletData(emitData.bulletID);
        switch (emitData.bulletShootType)
        {
            case BulletShootType.Circle:
                {


                }
                break;
            default:
                GameProjectileManager.Instance.CreateBullet(bulletData);
                break;
        }
    }
}
/// <summary>
/// 子弹基类
/// </summary>
public class BulletBase
{
    /// <summary>
    /// 子弹数据
    /// </summary>
    private BulletData bulletData;
    public BulletData BulletData => bulletData;
    /// <summary>
    /// 子弹物体
    /// </summary>
    public GameObject bulletObj;
    public BulletBase(BulletData data, GameObject Obj)
    {
        bulletData = data;
        bulletObj = Obj;
        bulletObj.transform.forward = data.direction;
    }
    public void OnUpdate()
    {
        if (bulletData == null) return;

        if (bulletData.target != null)
        {
            //追踪子弹--直接转向目标
            //方向
            //Vector3 direction = bulletData.target.transform.position - bulletObj.transform.position;
            //bulletObj.transform.LookAt(bulletData.target.transform);
            //bulletObj.transform.Translate(direction.normalized * bulletData.speed * Time.deltaTime, Space.World);

            //追踪子弹--缓慢转向目标
            //当前朝向
            Vector3 targetDirection = (bulletData.target.transform.position - bulletObj.transform.position).normalized;
            bulletObj.transform.forward = Vector3.Slerp(bulletObj.transform.forward, targetDirection, Time.deltaTime * 10);
        }

        bulletObj.transform.Translate(bulletObj.transform.forward * bulletData.speed * Time.deltaTime, Space.World);
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
    public float timeInterval = 0.1f;
    /// <summary>
    /// 生成子弹数量
    /// </summary>
    public float bulletCount;
    /// <summary>
    /// 子弹发射
    /// </summary>
    public BulletShootType bulletShootType;
}
/// <summary>
/// 子弹数据
/// </summary>
public class BulletData
{
    /// <summary>
    /// 创建者
    /// </summary>
    public GameActor owner;
    //创建它的技能
    //public SkillBase skill;  //命中后 调用技能的OnBulletHit(hitPos,hitTarget)
    /// <summary>
    /// 攻击信息
    /// </summary>
    public DamageInfo damageInfo;
    /// <summary>
    /// 出发点
    /// </summary>
    public Vector3 FromPostion;
    /// <summary>
    /// 目标
    /// </summary>
    public GameObject target;
    /// <summary>
    /// 速度
    /// </summary>
    public float speed;
    /// <summary>
    /// 加速度
    /// </summary>
    public float acceleration;
    /// <summary>
    /// 最大速度
    /// </summary>
    public float maxSpeed = 600;
    /// <summary>
    /// 转向速度
    /// </summary>
    public float angleSpeed = 10;
    /// <summary>
    /// 方向
    /// </summary>
    public Vector3 direction;
    /// <summary>
    /// 最大存活时间
    /// </summary>
    public float survivalMaxTime = 100f;
}