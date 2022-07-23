/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-23 
*/
using UnityEngine;
using System.Collections;

public abstract class GameActor : MonoBehaviour, ICustomLife, IInteractive
{
    public abstract void IInit();
    public abstract void IUpdate();
    public abstract void ILateUpdate();
    public abstract void IFixedUpdate();
    public abstract void IDie();

    /// <summary>
    /// 距离
    /// </summary>
    public virtual float Distance()
    {
        return float.MaxValue;
    }
    /// <summary>
    /// 可以交互
    /// </summary>
    public virtual bool CanInter()
    {
        return true;
    }
    /// <summary>
    /// 进入范围
    /// </summary>
    public virtual void EnterRang()
    {

    }
    /// <summary>
    /// 退出范围
    /// </summary>
    public virtual void ExitRang()
    {

    }
    /// <summary>
    /// 交互 
    /// </summary>
    public virtual void InterAction()
    {

    }
}