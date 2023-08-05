/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2023-05-21 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using YangTools.Extend;

/// <summary>
/// 抛物线子弹--只适用于到结束位置后自动销毁的物体(结束位置确定)
/// </summary>
public class ParabolaBullet : BulletBase
{
    protected ParabolaPath path;
    private float speed;
    public ParabolaBullet(BulletData data, GameObject Obj) : base(data, Obj)
    {
        speed = data.speed;
        radius = data.checkRadius;

        //假设抛物线最高点为目标点
        float hight = Data.TargetPostion.y;
        //结束位置为2倍--因为抛物线最高点为目标点,那结束位置就双倍
        float distance = Vector3.Distance(Data.StartPostion.SetYValue(), Data.TargetPostion.SetYValue()) * 2;
        //方向
        var direction = (Data.TargetPostion.SetYValue() - Data.StartPostion.SetYValue()).normalized;
        //结束位置
        var endPos = Data.StartPostion.SetYValue() + direction * distance;

        path = new ParabolaPath(Data.StartPostion, endPos, hight);
        path.isClampStartEnd = true;
    }
    public override void OnUpdate()
    {
        if (Data == null || bulletObj == null) return;
        path.Time += Time.deltaTime * speed;

        Vector3 tempPos = path.Position;
        bulletObj.transform.position = tempPos;
        //转向
        bulletObj.transform.LookAt(tempPos);

        CheckAllCollision();
        //到达目标点
        if (Vector3.Distance(tempPos, path.End) < 0.1f)
        {
            OnDie(BulletDieType.EndPoint);
        }
    }
}