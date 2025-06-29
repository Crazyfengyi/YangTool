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
using System.Collections.Generic;
using System.Threading.Tasks;
using DataStruct;
using YangTools.Scripts.Core.YangObjectPool;
using YangTools.Scripts.Core.YangExtend;

/// <summary>
/// 子弹基类
/// </summary>
public class BulletBase
{
    /// <summary>
    /// 对象池子弹对象
    /// </summary>
    private BulletObjectPoolItem poolItem;
    public BulletObjectPoolItem PoolItem => poolItem;
    /// <summary>
    /// 子弹数据
    /// </summary>
    protected BulletData data;
    public BulletData Data => data;
    /// <summary>
    /// 装饰器
    /// </summary>
    private List<BulletDecoratorBase> decorator;
    /// <summary>
    /// 子弹物体
    /// </summary>
    public GameObject bulletObj;

    //事件
    public Action OnStartEvent;
    public Action OnUpdateEvent;
    public Action<BulletDieType> OnEndEvent;

    /// <summary>
    /// 目标阵营
    /// </summary>
    public ActorCampType targetCamp;
    protected bool isDie;//是否销毁
    protected float radius = 0.1f;//检查半径
    protected float timer;

    public BulletBase(BulletData data, GameObject obj)
    {
        this.data = data;
        bulletObj = obj;
        bulletObj.transform.forward = data.direction;
        decorator = new List<BulletDecoratorBase>();

        OnStartEvent?.Invoke();
    }
    /// <summary>
    /// 设置对象池子弹类引用
    /// </summary>
    public void SetBulletObjectPoolItem(BulletObjectPoolItem _poolItem)
    {
        poolItem = _poolItem;
    }

    public virtual void OnUpdate()
    {
        if (Data == null || bulletObj == null) return;
        if (isDie) return;

        OnUpdateEvent?.Invoke();

        if (Data.target != null)
        {
            //追踪子弹--直接转向目标
            //方向
            //Vector3 direction = bulletData.target.transform.position - bulletObj.transform.position;
            //bulletObj.transform.LookAt(bulletData.target.transform);
            //bulletObj.transform.Translate(direction.normalized * bulletData.speed * Time.deltaTime, Space.World);

            //追踪子弹--缓慢转向目标
            //当前朝向
            Vector3 targetDirection = (Data.target.GetBoundsCenter() - bulletObj.transform.position).normalized;
            bulletObj.transform.forward = Vector3.Slerp(bulletObj.transform.forward, targetDirection, Time.deltaTime * 10);
        }

        bulletObj.transform.Translate(bulletObj.transform.forward * Data.speed * Time.deltaTime, Space.World);
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
        if (timer >= Data.survivalMaxTime)
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
                    Data.collisionPos = target.ClosestColliderPos(bulletObj.transform.position);
                    GameBattleManager.Instance.ProjectileHitProcess(Data, target);
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
                    Data.collisionPos = colliders[i].ClosestPoint(bulletObj.transform.position);
                    needDie = true;
                }
            }
        }
        return needDie;
    }
    /// <summary>
    /// 死亡
    /// </summary>
    public virtual void OnDie(BulletDieType dieType)
    {
        if (isDie) return;
        isDie = true;

        switch (dieType)
        {
            case BulletDieType.HaveAtk:
                {
                    Vector3 effectPos = Data.collisionPos != default ? Data.collisionPos : bulletObj.transform.position;
                    GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
                    GameEffectManager.Instance.PlayEffect("BulletAtkEffect", effectPos);
                }
                break;
            case BulletDieType.EndPoint:
                {
                    Vector3 effectPos = Data.collisionPos != default ? Data.collisionPos : bulletObj.transform.position;
                    GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
                    GameEffectManager.Instance.PlayEffect("SmokeEffect", effectPos);
                }
                break;
            case BulletDieType.Collision:
                {
                    Vector3 effectPos = Data.collisionPos != default ? Data.collisionPos : bulletObj.transform.position;
                    GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
                    GameEffectManager.Instance.PlayEffect("SmokeEffect", effectPos);
                }
                break;
            case BulletDieType.TimeEnd:

                break;
            default:
                break;
        }

        if (!string.IsNullOrEmpty(Data.dieEffectName))
        {
            Vector3 effectPos = Data.collisionPos != default ? Data.collisionPos : bulletObj.transform.position;
            GameEffectManager.Instance.PlayEffect(Data.dieEffectName, effectPos);
        }

        OnEndEvent?.Invoke(dieType);
        GameProjectileManager.Instance.RemoveBullet(this);
    }
}

/// <summary>
/// 对象池子弹对象
/// </summary>
public class BulletObjectPoolItem : IPoolItem<BulletObjectPoolItem>
{
    public string PoolKey { get; set; }
    public bool IsInPool { get; set; }
    public GameObject obj;

    private string pathName;
    public BulletObjectPoolItem()
    {
    }

    public BulletObjectPoolItem(string name)
    {
        pathName = name;
    }

    public void InitData(BulletData bulletData)
    {
    }

    public Task OnCreate()
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResourceLoad($"Bullets/{pathName}"));
        obj = tempObj;
        return Task.CompletedTask;
    }

    public void OnGet()
    {
        obj.DefaultGameObjectOnGet(null);
    }

    public void OnRecycle()
    {
        obj.DefaultGameObjectRecycle();
    }

    public void OnDestroy()
    {
        obj.DefaultGameObjectDestroy();
    }
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