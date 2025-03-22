/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-08-05 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;

/// <summary>
/// 子弹装饰器基类
/// </summary>
public abstract class BulletDecoratorBase
{
    public BulletBase bullet;//子弹

    public BulletDecoratorBase(BulletBase bullet)
    {
        this.bullet = bullet;
    }
}