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
        if (!handle.Target) return;

        // GetBulletData(emitData.bulletID); 通过ID获取子弹
        BulletData data = new BulletData();
        data.owner = handle;
        data.speed = 1;
        data.StartPostion = ((Monster)handle).shootPoint.transform.position;
        data.TargetPostion = handle.Target.transform.position;
        data.direction = ((Monster)handle).model.transform.forward;
        data.name = "TestBulletPhysic";
        data.damageInfo = handle.GetDamageInfo();
        data.damageInfo.beHitEffectInfo = new EffectInfo();
        data.target = handle.Target;

        //测试
        emitData.bulletShootType = BulletShootType.Physics;

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

                    //看下目标方向
                    Vector3 projectileXZPos = new Vector3(data.StartPostion.x, 0.0f, data.StartPostion.z);
                    Vector3 targetXZPos = new Vector3(data.TargetPostion.x, 0.0f, data.TargetPostion.z);

                    //发射角度
                    var LaunchAngle = 60;
                    //加速度--重力加速度
                    var G = Physics.gravity.y;
                    //水平距离
                    var R = Vector3.Distance(projectileXZPos, targetXZPos);
                    //垂直距离
                    var H = data.TargetPostion.y - data.StartPostion.y;
                    //太高了就调整角度
                    if (H > 6) LaunchAngle = 80;
                    if (H < 2) LaunchAngle = 30;

                    //tan(α)
                    float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);

                    //P 1 = P 0 + v 0 t + 0.5 a t 2 推论而来---(匀速运动)---时间与角度相关(时间被公式转换约掉了)
                    //计算弹丸落在目标物体上所需的初始速度
                    float Vz = Mathf.Sqrt(Mathf.Abs(G * R * R / (2.0f * (H - R * tanAlpha))));
                    float Vy = tanAlpha * Vz;

                    //在局部空间中创建速度矢量，并转换成全局空间
                    Vector3 localVelocity = new Vector3(0f, Vy, Vz);
                    Vector3 globalVelocity = ((Monster)handle).shootPoint.transform.TransformDirection(localVelocity);
                    data.initialVelocity = globalVelocity;
                    GameProjectileManager.Instance.CreateBullet(data, emitData.bulletShootType, handle.canAtkCamp);
                }
                break;
            case BulletShootType.Parabola:
                GameProjectileManager.Instance.CreateBullet(data, emitData.bulletShootType, handle.canAtkCamp);
                break;
            case BulletShootType.Physics:
                GameProjectileManager.Instance.CreateBullet(data, emitData.bulletShootType, handle.canAtkCamp);
                break;
            default:
                GameProjectileManager.Instance.CreateBullet(data, emitData.bulletShootType, handle.canAtkCamp);
                break;
        }
    }
}