/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-09-04 
*/

using DataStruct;
using DG.Tweening;
using UnityEngine;
using YangTools.Scripts.Core.YangExtend;

/// <summary>
/// 灌木
/// </summary>
public class Bush : GameActor
{
    public bool haveEnterEffect;
    public float rang;

    #region 生命周期
    public override void IInit()
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
    public override void IDie()
    {
    }
    public override void IDestroy()
    {
    }
    #endregion

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
        return rang;
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
        if (haveEnterEffect)
        {
            //树叶特效
            GameEffectManager.Instance.PlayEffect("BushEnter", transform.position);

            //抖动
            DOTween.Kill(gameObject);
            gameObject.transform.DOShakePosition(0.36f,0.06f,30)
                .SetTarget(gameObject);
        }
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
        float size = GetSize(RoleSizeType.ColliderSize);
        Vector3 temp = Random.onUnitSphere;
        temp = temp.SetYValue();
        temp = temp.normalized;
        GameObject obj = GameActorManager.Instance.CreateItem("Apple", transform.position + temp * size);
    }
    #endregion
}