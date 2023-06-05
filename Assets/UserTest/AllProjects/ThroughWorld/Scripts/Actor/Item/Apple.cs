/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-09-04 
*/
using UnityEngine;

public class Apple : GameActor
{
    public override void IInit()
    {
    }
    public override void IFixedUpdate()
    {
    }
    public override void ILateUpdate()
    {
    }
    public override void IUpdate()
    {
    }
    public override void IDie()
    {
    }
    public override void IDestroy()
    {
    }

    #region 交互物
    /// <summary>
    /// 距离
    /// </summary>
    public override float Distance(Vector3 point)
    {
        return base.Distance(point);
    }
    /// <summary>
    /// 覆盖范围(物体需要的范围)
    /// </summary>
    public override float GetOverideMaxDistance()
    {
        return float.MaxValue;
    }
    /// <summary>
    /// 交互类型
    /// </summary>
    public InterActiveType GetInterActiveType()
    {
        return InterActiveType.Trigger;
    }
    /// <summary>
    /// 可以交互
    /// </summary>
    public override bool CanInter()
    {
        return true;
    }
    /// <summary>
    /// 进入范围
    /// </summary>
    public override void EnterRang(RoleBase role)
    {

    }
    /// <summary>
    /// 退出范围
    /// </summary>
    public override void ExitRang(RoleBase role)
    {

    }
    /// <summary>
    /// 交互 
    /// </summary>
    public override void InterAction(RoleBase role)
    {
        GameUIManager.Instance.AddTipsShow("获得苹果");
        Destroy(gameObject);
    }
    #endregion
}