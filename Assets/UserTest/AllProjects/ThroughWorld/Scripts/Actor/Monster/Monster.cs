/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-14 
*/
using UnityEngine;
using System.Collections;

/// <summary>
/// 怪物控制器
/// </summary>
public class Monster : RoleBase
{
    public override void IInit()
    {
        base.IInit();
        roleBuffControl.Add(BuffID.buff_10001);
    }
    public override void IDie()
    {

    }
    public override void IUpdate()
    {
        base.IUpdate();
    }
    public override void ILateUpdate()
    {
        //if (Animator) Animator.SetFloat("Speed", inputVector3.magnitude);
    }
    public override void IFixedUpdate()
    {

    }
}