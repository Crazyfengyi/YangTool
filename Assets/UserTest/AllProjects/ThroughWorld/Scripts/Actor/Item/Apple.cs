/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-09-04 
*/

using DataStruct;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEngine;

public class Apple : GameActor
{
    private Vector3 startPos;
    private float timer;
    public override void IInit()
    {
        startPos = transform.position;
    }
    public override void IFixedUpdate()
    {
    }
    public override void ILateUpdate()
    {
    }
    public override void IUpdate()
    {
        timer += Time.deltaTime;
        transform.position = startPos + new Vector3(0, Mathf.Abs(Mathf.Sin(timer) * 0.3f), 0);
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
    public override float DistanceToPoint(Vector3 point)
    {
        return base.DistanceToPoint(point);
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
        GameUIManager.Instance.AddTipsShow("获得苹果");
        GameActorManager.Instance.RemoveActor(this);
        Destroy(gameObject);
    }
    #endregion
}