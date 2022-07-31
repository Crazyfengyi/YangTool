/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-31 
*/
using UnityEngine;
using System.Collections;
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
    public override float Distance(Vector3 point)
    {
        return 6f;
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
    public override void EnterRang()
    {
        Debug.LogError("进入范围");
    }
    /// <summary>
    /// 退出范围
    /// </summary>
    public override void ExitRang()
    {
        Debug.LogError("退出范围");
    }
    /// <summary>
    /// 交互 
    /// </summary>
    public override void InterAction(RoleBase role)
    {
        Debug.LogError("交互");
        SceneLoader.Instance.IsAutoSkip = true;
        SceneLoader.Instance.Load("GameLevel1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    #endregion
}