/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-11-13 
*/
using UnityEngine;
using System.Collections;

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
        // GetBulletData(emitData.bulletID); 通过ID火气子弹
        BulletData data = new BulletData();
        data.owner = handle;
        data.speed = 10;
        data.FromPostion = ((PlayerController)handle).shootPoint.transform.position;
        data.direction = ((PlayerController)handle).model.transform.forward;

        //Collider[] temp = Physics.OverlapSphere(transform.position, 10);
        //if (temp.Length > 0)
        //{
        //    for (int i = 0; i < temp.Length; i++)
        //    {
        //        GameActor target = temp[i].gameObject.GetComponentInParent<GameActor>();
        //        if (target && target.campType == ActorCampType.Monster)
        //        {
        //            data.target = target.gameObject;
        //            break;
        //        }
        //    }
        //}

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
                        GameProjectileManager.Instance.CreateBullet(data, ActorCampType.MonsterAndBuilding);
                    }
                }
                break;
            default:
                GameProjectileManager.Instance.CreateBullet(data, ActorCampType.MonsterAndBuilding);
                break;
        }
    }
}