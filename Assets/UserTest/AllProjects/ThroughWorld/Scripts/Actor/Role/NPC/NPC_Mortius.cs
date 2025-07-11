/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-31 
*/
using UnityEngine;
using YangTools;

/// <summary>
/// NPC莫提乌斯
/// </summary>
public class NPC_Mortius : RoleBase
{
    #region 生命周期
    public override void IInit()
    {
    }
    public override void IDie()
    {
    }
    public override void IUpdate()
    {
    }
    public override void ILateUpdate()
    {
    }
    public override void IFixedUpdate()
    {
    }
    #endregion

    #region 交互接口
    /// <summary>
    /// 距离
    /// </summary>
    public override float DistanceToPoint(Vector3 point)
    {
        return Vector3.Distance(transform.position, point);
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
    public override void OnEnterRang(RoleBase role)
    {
    }
    /// <summary>
    /// 退出范围
    /// </summary>
    public override void OnExitRang(RoleBase role)
    {
    }
    /// <summary>
    /// 交互 
    /// </summary>
    public override void Interact(RoleBase role)
    {
        YangTools.GameManager.Instance.SceneLoad("GameLevel2");
    }
    #endregion
}