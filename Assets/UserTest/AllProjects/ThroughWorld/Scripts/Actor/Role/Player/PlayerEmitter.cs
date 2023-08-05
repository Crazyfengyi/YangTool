/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-11-13 
*/
using UnityEngine;

/// <summary>
/// 玩家发射器
/// </summary>
public class PlayerEmitter : EmitterBase
{
    public PlayerEmitter(GameActor _actor) : base(_actor)
    {
    }
    /// <summary>
    /// 发射
    /// </summary>
    public override void Shoot()
    {
        // GetBulletData(emitData.bulletID); 通过ID获取子弹
        BulletData data = new BulletData();
        data.owner = handle;
        data.speed = 10;
        data.StartPostion = ((PlayerController)handle).shootPoint.transform.position;
        data.direction = ((PlayerController)handle).model.transform.forward;
        data.name = "TestBullet";
        data.target = handle.Target.gameObject;
        data.damageInfo = handle.GetDamageInfo();
        data.damageInfo.beHitEffectInfo = new EffectInfo();
        data.targetCampType = handle.canAtkCamp;

        data.bulletType = (BulletType)emitData.bulletShootType;

        switch (emitData.bulletShootType)
        {
            case BulletShootType.Circle:
                {
                    float angle = 360 / emitData.bulletCount;
                    Vector3 startDirection = ((PlayerController)handle).model.transform.forward;
                    for (int i = 0; i < emitData.bulletCount; i++)
                    {
                        Vector3 temp = Quaternion.AngleAxis(angle * i, Vector3.up) * startDirection;
                        data.direction = temp;
                        GameProjectileManager.Instance.CreateBullet(data);
                    }
                }
                break;
            default:
                GameProjectileManager.Instance.CreateBullet(data);
                break;
        }
    }
}