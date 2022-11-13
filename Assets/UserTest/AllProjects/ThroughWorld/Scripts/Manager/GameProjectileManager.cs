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
    /// 场景切换
    /// </summary>
    public void OnSceneChange()
    {
        for (int i = 0; i < allBullet.Count; i++)
        {
            allBullet[i].OnDie(BulletDieType.SceneChange);
        }
    }
    /// <summary>
    /// 创建子弹
    /// </summary>
    public BulletBase CreateBullet(BulletData bulletData)
    {
        //TODO:需要换成对象池
        GameObject Obj = GameResourceManager.Instance.ResoruceLoad($"Bullets/BulletTest");
        GameObject bulletObj = GameObject.Instantiate(Obj, bulletData.FromPostion, Quaternion.identity);

        BulletBase bulletBase = new BulletBase(bulletData, bulletObj);
        allBullet.Add(bulletBase);
        return bulletBase;
    }
    /// <summary>
    /// 移除子弹
    /// </summary>
    public void RemoveBullet(BulletBase bulletBase)
    {
        allBullet.Remove(bulletBase);
        //TODO:需要换成对象池
        Destroy(bulletBase.bulletObj);
    }
}
/// <summary>
/// 发射器基类
/// </summary>
public class EmitterBase
{
    /// <summary>
    /// 发射者
    /// </summary>
    public GameActor actor;
    /// <summary>
    /// 发射信息
    /// </summary>
    public EmitData emitData;

    protected int shootCount;//发射次数
    protected float timer;
    protected bool startShoot;//开始发射
    public EmitterBase(GameActor _actor)
    {
        actor = _actor;
    }
    /// <summary>
    /// 开始发射
    /// </summary>
    public virtual void StartShoot()
    {
        startShoot = true;
    }
    public virtual void OnUpdate()
    {
        if (!startShoot) return;
        if (emitData == null) return;
        timer += Time.deltaTime;
        if (timer > emitData.timeInterval)
        {
            timer = 0;
            Shoot();
            shootCount++;
            if (shootCount > emitData.loopCount)
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

                }
                break;
            default:
                GameProjectileManager.Instance.CreateBullet(bulletData);
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

    private float timer;
    public BulletBase(BulletData data, GameObject Obj)
    {
        bulletData = data;
        bulletObj = Obj;
        bulletObj.transform.forward = data.direction;
    }
    public void OnUpdate()
    {
        if (bulletData == null || bulletObj == null) return;
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
        CheckAtk();

        timer += Time.deltaTime;
        if (timer >= bulletData.survivalMaxTime)
        {
            OnDie(BulletDieType.TimeOut);
        }
    }
    /// <summary>
    /// 检查攻击
    /// </summary>
    public void CheckAtk()
    {
        Collider[] temp = Physics.OverlapSphere(bulletObj.transform.position, 0.5f);
        if (temp.Length > 0)
        {
            bool needDie = false;
            for (int i = 0; i < temp.Length; i++)
            {
                GameActor target = temp[i].gameObject.GetComponentInParent<GameActor>();
                if (target && target.campType == ActorCampType.Monster)//TODO:根据目标类型造成伤害
                {
                    bulletData.collisionPos = target.ClosestColliderPos(bulletObj.transform.position);
                    GameBattleManager.Instance.AtkProcess(bulletData.owner, target);
                    needDie = true;
                }
            }

            if (needDie) OnDie(BulletDieType.Atk);
        }
    }
    /// <summary>
    /// 死亡
    /// </summary>
    public virtual void OnDie(BulletDieType bulletDieType)
    {
        switch (bulletDieType)
        {
            case BulletDieType.Atk:
                {
                    Vector3 effectPos = bulletData.collisionPos != default ? bulletData.collisionPos : bulletObj.transform.position;
                    GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
                    GameEffectManager.Instance.PlayEffect("BulletAtkEffect", effectPos);
                }
                break;
            default:
                break;
        }

        GameProjectileManager.Instance.RemoveBullet(this);
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
    /// 角度
    /// </summary>
    public float angle;
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
    public float survivalMaxTime = 10f;
    /// <summary>
    /// 碰撞点
    /// </summary>
    public Vector3 collisionPos;
}