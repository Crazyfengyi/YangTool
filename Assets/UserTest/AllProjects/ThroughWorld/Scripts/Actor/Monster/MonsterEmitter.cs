/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-11-27 
*/
using UnityEngine;
using System.Collections;
/// <summary>
/// 怪物发射器
/// </summary>
public class MonsterEmitter : EmitterBase
{
    public MonsterEmitter(GameActor _actor) : base(_actor)
    {

    }
    /// <summary>
    /// 发射
    /// </summary>
    public override void Shoot()
    {
        if (!handle.Target.transform) return;

        // GetBulletData(emitData.bulletID); 通过ID获取子弹
        BulletData data = new BulletData();
        data.owner = handle;
        data.speed = 10;
        data.StartPostion = ((Monster)handle).shootPoint.transform.position;
        data.EndPostion = handle.Target.transform.position;
        data.direction = ((Monster)handle).model.transform.forward;
        data.name = "TestBullet";
        data.damageInfo = handle.GetDamageInfo();
        data.damageInfo.beHitEffectInfo = new EffectInfo();

        //测试
        emitData.bulletShootType = BulletShootType.Throw;

        switch (emitData.bulletShootType)
        {
            case BulletShootType.Circle:
                {
                    data.name = "TestBullet";
                    float angle = 360 / emitData.bulletCount;
                    Vector3 startDirection = ((PlayerController)handle).model.transform.forward;
                    for (int i = 0; i < emitData.bulletCount; i++)
                    {
                        Vector3 temp = Quaternion.AngleAxis(angle * i, Vector3.up) * startDirection;
                        data.direction = temp;
                        GameProjectileManager.Instance.CreateBullet(data, emitData.bulletShootType, handle.canAtkCamp);
                    }
                }
                break;
            case BulletShootType.Throw:
                {
                    data.name = "ThrowBullet";
                    GameProjectileManager.Instance.CreateBullet(data, emitData.bulletShootType, handle.canAtkCamp);
                }
                break;
            default:
                GameProjectileManager.Instance.CreateBullet(data, emitData.bulletShootType, handle.canAtkCamp);
                break;
        }
    }
}