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
using YangTools.UGUI;

public class AddAtk : BulletDecoratorBase
{
    public int addAtk = 10;//TODO:读表

    public AddAtk(BulletBase bullet) : base(bullet)
    {
        bullet.Data.damageInfo.damage += addAtk;
    }
}