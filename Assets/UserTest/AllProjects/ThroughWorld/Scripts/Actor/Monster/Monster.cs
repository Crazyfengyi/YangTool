/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-14 
*/
using UnityEngine;
using System.Collections;
using Pathfinding;

/// <summary>
/// 怪物控制器
/// </summary>
public class Monster : RoleBase
{
    private AIPath aiPath;
    public override void IInit()
    {
        base.IInit();
        campType = ActorCampType.Monster;
        roleBuffControl.Add(BuffID.buff_10001);
        aiPath = GetComponent<AIPath>();
    }
    public override void IDie()
    {

    }
    public override void IUpdate()
    {
        base.IUpdate();

        if (AstarPath.active != null && aiPath != null && GameActorManager.Instance.MainPlayer != null)
        {
            aiPath.destination = GameActorManager.Instance.MainPlayer.transform.position;
            if (Vector3.Distance(transform.position, GameActorManager.Instance.MainPlayer.transform.position) < 3)
            {
                aiPath.isStopped = true;
            }
            else
            {
                aiPath.isStopped = false;
            }
        }
    }
    public override void ILateUpdate()
    {
        //if (Animator) Animator.SetFloat("Speed", inputVector3.magnitude);
    }
    public override void IFixedUpdate()
    {

    }

    #region 攻击和被击接口实现
    public override void Atk(AtkInfo atkInfo)
    {

    }
    public override void BeHit(ref DamageInfo damageInfo)
    {
        healthControl.MinusHp(damageInfo);
    }
    public override DamageInfo GetDamageInfo()
    {
        return null;
    }
    public override DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        return damageInfo;
    }
    public override bool IsCanAtk()
    {
        return false;
    }
    public override bool IsCanBeHit()
    {
        return true;
    }
    public override void ShowAtkEffect(EffectInfo atkEffectInfo)
    {
    }
    public override void ShowBeHitEffect(EffectInfo hitEffectInfo)
    {
    }
    #endregion
}