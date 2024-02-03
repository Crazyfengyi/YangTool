/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-08-28
*/

using System.Collections.Generic;
using DataStruct;
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
        YangObjectPool.Recycle(bulletBase.PoolItem);
    }
}
