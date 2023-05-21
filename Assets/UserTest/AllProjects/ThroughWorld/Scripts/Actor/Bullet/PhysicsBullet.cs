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
  
/// <summary>
/// 物理子弹
/// </summary>
public class PhysicsBullet : BulletBase
{
    public PhysicsBullet(BulletData data, GameObject Obj) : base(data, Obj)
    {
        bulletData = data;
        bulletObj = Obj;
        bulletObj.transform.forward = data.direction;
        //initialVelocity = data.initialVelocity;
        //aVelocity = data.aVelocity;
        //radius = data.checkRadius;

        //currentVelocity = initialVelocity;
    }

    //TODO:用刚体做,有物理表现--碰撞到建筑后反弹


} 