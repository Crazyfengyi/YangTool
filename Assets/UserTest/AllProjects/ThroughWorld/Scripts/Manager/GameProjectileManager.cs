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
public class GameProjectileManager : MonoSingleton<GameProjectileManager>
{
    //Shooter 射手
    //emitter
    //发射器，发射体

    //子弹生成器主要是创建子弹，所以需要包含子弹类的所有参数
    /*
	 *   publicbool bAuto = false;
		 public GameObject bulletPrefab;
		//子弹目标 5public GameObject target;
		//初速度 7publicfloat velocity = 0f;
		//加速度 9publicfloat acceleration = 30f;
		//总生命周期11publicfloat lifeTime = 3f;
		//初始方向13public Vector2 direction = Vector2.zero;
		//最大速度15publicfloat maxVelocity = 600;
		//角速度17publicfloat palstance = 120;
		//角度波动范围19publicfloat angelRange = 0f;
		//延迟21publicfloat delay = 1f;
		//是否循环23publicbool bLoop = false;
		//时间间隔25publicfloat timeCell = .1f;
		//生成数量27publicint count = 1;
		//伤害29publicfloat damage;
		//碰撞类型31public CollisionType collisionType;
		//是否有子系统33publicbool bChildShooter = false;
		//子系统是谁35public GameObject childShooter;
	 * 
	 * 
	 * */

    /*
	 *  //目标 2public GameObject Target { get; set; }
		//瞬时速度 4publicfloat Velocity { get; set; }
		//剩余生命周期 6publicfloat LifeTime { get; set; }
		//角速度 8publicfloat Palstance { get; set; }
		//线性加速度10publicfloat Acceleration { get; set; }
		//最大速度12publicfloat MaxVelocity { get; set; }
	 * */

    /*
	 * 创建追踪子弹需要的参数主要有：
		Owner：表示子弹的创建者
		Ability：表示子弹关联的技能
		FromPosition：子弹的出发地点
		Target：子弹追踪的目标
		Speed：子弹的飞行速率
		创建线性子弹需要的参数主要有：

		Owner：表示子弹的创建者
		Ability：表示子弹关联的技能
		FromPosition：子弹的出发地点
		Velocity：子弹的飞行速度和方向
		StartWidth，EndWith，Distance：（等腰梯形检测盒）起点宽度，终点宽度，飞行距离
		FilterTargetInfo：子弹筛选目标信息

		由于子弹在创建时传入了Ability参数，那么当子弹检测到命中目标后便可以通过调用GetAbility().OnProjectileHit(projHandle, hitTarget, hitPosition)在Ability Class中执行命中逻辑，
		这样它就可以在技能模块中通过执行策划配置来实现各种效果了。子弹本身是没有任何特殊逻辑的，它只有位置更新，
		检查命中目标和是否结束并销毁这几个简单的功能。
	*/

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
/// 子弹基类
/// </summary>
public class BulletBase
{
    /// <summary>
    /// 子弹数据
    /// </summary>
    public BulletData bulletData;
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
    /// 方向
    /// </summary>
    public Vector3 direction;
}