/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-08-28
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YangTools;
using YangTools.Extend;
using YangTools.ObjectPool;
using static Sirenix.OdinInspector.Editor.UnityPropertyEmitter;

/// <summary>
/// 抛射体管理器(子弹)
/// </summary>
/// <remarks>Shooter射手-->emitter发射器--(发射信息--子弹id)-->生成子弹bullet(AtkInfo=>DamageInfo&&EffectInfo)</remarks>
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
    public void OnSceneChangeStart(string sceneName)
    {
        for (int i = 0; i < allBullet.Count; i++)
        {
            allBullet[i].OnDie(BulletDieType.SceneChange);
        }
        allBullet.Clear();
    }

    /// <summary>
    /// 创建子弹
    /// </summary>
    public BulletBase CreateBullet(BulletData bulletData)
    {
        //子弹对象
        BulletObjectPoolItem poolItem = YangObjectPool.Get<BulletObjectPoolItem>(bulletData.name, bulletData.name);
        poolItem.InitData(bulletData);
        GameObject bulletObj = poolItem.obj;
        bulletObj.transform.position = bulletData.StartPostion;
        BulletBase bulletBase = null; //需要表里拿配置的静态数据赋值
        //TODO:更具子弹ID子弹类型
        switch (bulletData.bulletType)
        {
            case BulletType.None:
                bulletBase = new BulletBase(bulletData, bulletObj);
                break;
            case BulletType.Circle:
                bulletBase = new BulletBase(bulletData, bulletObj);
                break;
            case BulletType.Throw:
                bulletBase = new ThrowBullet(bulletData, bulletObj);
                break;
            case BulletType.Parabola:
                bulletBase = new ParabolaBullet(bulletData, bulletObj);
                break;
            case BulletType.Physics:
                bulletBase = new PhysicsBullet(bulletData, bulletObj);
                break;
            case BulletType.Bomb:
                bulletBase = new DelayBomb(bulletData, bulletObj);
                break;
            default:
                bulletBase = new BulletBase(bulletData, bulletObj);
                break;
        }

        bulletBase.targetCamp = bulletData.targetCampType;
        bulletBase.SetBulletObjectPoolItem(poolItem);
        allBullet.Add(bulletBase);
        return bulletBase;
    }

    /// <summary>
    /// 移除子弹
    /// </summary>
    public void RemoveBullet(BulletBase bulletBase)
    {
        allBullet.Remove(bulletBase);
        YangObjectPool.Recycle(bulletBase.BulletObjectPoolItem);
    }
}

#region 发射器和子弹基类 信息类
/// <summary>
/// 发射器基类
/// </summary>
public class EmitterBase
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

    public EmitterBase(GameActor _actor)
    {
        handle = _actor;
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
                        GameProjectileManager.Instance.CreateBullet(bulletData);
                    }
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
    /// 对象池子弹对象
    /// </summary>
    private BulletObjectPoolItem bulletObjectPoolItem;
    public BulletObjectPoolItem BulletObjectPoolItem => bulletObjectPoolItem;

    /// <summary>
    /// 子弹数据
    /// </summary>
    protected BulletData bulletData;
    public BulletData BulletData => bulletData;

    /// <summary>
    /// 子弹物体
    /// </summary>
    public GameObject bulletObj;

    /// <summary>
    /// 目标阵营
    /// </summary>
    public ActorCampType targetCamp;

    protected bool isDie;//是否销毁
    protected float radius = 0.1f;//检查半径
    protected float timer;

    public BulletBase(BulletData data, GameObject Obj)
    {
        bulletData = data;
        bulletObj = Obj;
        bulletObj.transform.forward = data.direction;
    }
    /// <summary>
    /// 设置对象池子弹类引用
    /// </summary>
    public void SetBulletObjectPoolItem(BulletObjectPoolItem _bulletObjectPoolItem)
    {
        bulletObjectPoolItem = _bulletObjectPoolItem;
    }

    public virtual void OnUpdate()
    {
        if (bulletData == null || bulletObj == null) return;
        if (isDie) return;
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

        CheckAllCollision();
    }
    /// <summary>
    /// 检查所有碰撞相关
    /// </summary>
    public void CheckAllCollision()
    {
        if (isDie) return;

        if (CheckAtk())
        {
            OnDie(BulletDieType.HaveAtk);
        }

        if (CheckCollision())
        {
            OnDie(BulletDieType.Collision);
        }

        timer += Time.deltaTime;
        if (timer >= bulletData.survivalMaxTime)
        {
            OnDie(BulletDieType.TimeEnd);
        }
    }
    /// <summary>
    /// 检查攻击
    /// </summary>
    public virtual bool CheckAtk()
    {
        Collider[] temp = Physics.OverlapSphere(bulletObj.transform.position, radius);
        bool needDie = false;
        if (temp.Length > 0)
        {
            for (int i = 0; i < temp.Length; i++)
            {
                GameActor target = temp[i].gameObject.GetComponentInParent<GameActor>();
                if (target && targetCamp.HasFlag(target.campType) && target.IsCanAtk())
                {
                    bulletData.collisionPos = target.ClosestColliderPos(bulletObj.transform.position);
                    GameBattleManager.Instance.ProjectileHitProcess(bulletData, target);
                    needDie = true;
                }
            }
        }
        return needDie;
    }
    /// <summary>
    /// 检查碰撞
    /// </summary>
    public virtual bool CheckCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(bulletObj.transform.position, radius);
        bool needDie = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                int targetLayer = LayerMask.GetMask("Ground", "Build");
                //位运算
                if ((1 << colliders[i].gameObject.layer & targetLayer) != 0)
                {
                    bulletData.collisionPos = colliders[i].ClosestPoint(bulletObj.transform.position);
                    needDie = true;
                }
            }
        }
        return needDie;
    }
    /// <summary>
    /// 死亡
    /// </summary>
    public virtual void OnDie(BulletDieType bulletDieType)
    {
        if (isDie) return;
        isDie = true;

        switch (bulletDieType)
        {
            case BulletDieType.HaveAtk:
                {
                    Vector3 effectPos = bulletData.collisionPos != default ? bulletData.collisionPos : bulletObj.transform.position;
                    GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
                    GameEffectManager.Instance.PlayEffect("BulletAtkEffect", effectPos);
                }
                break;
            case BulletDieType.EndPoint:
                {
                    Vector3 effectPos = bulletData.collisionPos != default ? bulletData.collisionPos : bulletObj.transform.position;
                    GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
                    GameEffectManager.Instance.PlayEffect("SmokeEffect", effectPos);
                }
                break;
            case BulletDieType.Collision:
                {
                    Vector3 effectPos = bulletData.collisionPos != default ? bulletData.collisionPos : bulletObj.transform.position;
                    GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
                    GameEffectManager.Instance.PlayEffect("SmokeEffect", effectPos);
                }
                break;
            case BulletDieType.TimeEnd:

                break;
            default:
                break;
        }

        if (!string.IsNullOrEmpty(bulletData.dieEffectName))
        {
            Vector3 effectPos = bulletData.collisionPos != default ? bulletData.collisionPos : bulletObj.transform.position;
            GameEffectManager.Instance.PlayEffect(bulletData.dieEffectName, effectPos);
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
/// <summary>
/// 子弹数据
/// </summary>
public class BulletData
{
    public string name;
    /// <summary>
    /// 子弹类型
    /// </summary>
    public BulletType bulletType;
    /// <summary>
    /// 创建者
    /// </summary>
    public GameActor owner;
    /// <summary>
    /// 目标阵营
    /// </summary>
    public ActorCampType targetCampType;
    //创建它的技能
    //public SkillBase skill;  //命中后 调用技能的OnBulletHit(hitPos,hitTarget)
    /// <summary>
    /// 击中伤害信息
    /// </summary>
    public DamageInfo damageInfo;

    /// <summary>
    /// 出发点
    /// </summary>
    public Vector3 StartPostion;

    /// <summary>
    /// 目标点
    /// </summary>
    public Vector3 TargetPostion;

    /// <summary>
    /// 目标
    /// </summary>
    public GameObject target;

    /// <summary>
    /// 初速度
    /// </summary>
    public Vector3 initialVelocity;

    /// <summary>
    /// 速度
    /// </summary>
    public float speed = 1;

    /// <summary>
    /// 加速度
    /// </summary>
    public Vector3 aVelocity = new Vector3(0, -9.8f, 0);

    /// <summary>
    /// 伤害检查半径
    /// </summary>
    public float checkRadius = 0.2f;

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
    public float survivalMaxTime = 120f;

    /// <summary>
    /// 碰撞点
    /// </summary>
    public Vector3 collisionPos;

    /// <summary>
    /// 死亡特效名称
    /// </summary>
    public string dieEffectName;
}
/// <summary>
/// 对象池子弹对象
/// </summary>
public class BulletObjectPoolItem : IPoolItem<BulletObjectPoolItem>
{
    public string PoolKey { get; set; }
    public bool IsInPool { get; set; }
    public GameObject obj;

    public BulletObjectPoolItem()
    {
    }

    public BulletObjectPoolItem(string name)
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad($"Bullets/{name}"));
        obj = tempObj;
    }

    public void InitData(BulletData bulletData)
    {
    }

    public void OnGet()
    {
        obj.DefualtGameObjectOnGet(null);
    }

    public void OnRecycle()
    {
        obj.DefualtGameObjectRecycle();
    }

    public void OnDestroy()
    {
        obj.DefualtGameObjectDestory();
    }
}
#endregion