/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-03-20
*/

using UnityEngine;
using YangTools;

/// <summary>
/// 投掷子弹
/// </summary>
public class ThrowBullet : BulletBase
{
    protected ParabolaPath path;

    public ThrowBullet(BulletData data, GameObject Obj) : base(data, Obj)
    {
        bulletData = data;
        bulletObj = Obj;
        bulletObj.transform.forward = data.direction;

        radius = 0.2f;
        float hight = 0;
        float distance = Vector3.Distance(bulletData.StartPostion, bulletData.EndPostion);
        //根据距离设置抛物线高度
        if (distance > 6)
        {
            hight = distance / 3;
        }
        else if (distance > 2)
        {
            hight = distance / 4;
        }
        else
        {
            hight = 0;
        }

        path = new ParabolaPath(bulletData.StartPostion, bulletData.EndPostion, hight);
        path.isClampStartEnd = true;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (bulletData == null || bulletObj == null) return;
        path.Time += Time.deltaTime;

        Vector3 tempPos = path.Position;
        //移动
        bulletObj.transform.position = tempPos;
        //转向
        bulletObj.transform.LookAt(tempPos);

        //是否检查到物体
        if (CheckAtk())
        {
            OnDie(BulletDieType.Atk);
        }

        //到达目标点
        if (Vector3.Distance(tempPos, path.End) < 0.1f)
        {
            OnDie(BulletDieType.End);
        }
    }

    public override bool CheckAtk()
    {
        return base.CheckAtk();
    }

    public override void OnDie(BulletDieType bulletDieType)
    {
        base.OnDie(bulletDieType);
    }
}