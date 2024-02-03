/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-07-16 
*/  
using System;
using System.Collections;
using DataStruct;
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;

/// <summary>
/// 延迟炸弹
/// </summary>
public class DelayBomb : BulletBase
{
    public DelayBomb(BulletData data, GameObject Obj) : base(data, Obj)
    {
        

    }

    public override void OnUpdate()
    {
        //base.OnUpdate();
        if (Data == null || bulletObj == null) return;
        if (isDie) return;

        timer += Time.deltaTime;
        if (timer >= Data.survivalMaxTime)
        {
            CheckAtk();
            OnDie(BulletDieType.TimeEnd);
        }
    }
} 