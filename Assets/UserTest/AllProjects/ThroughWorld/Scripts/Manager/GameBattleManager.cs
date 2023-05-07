/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-10-15 
*/
using UnityEngine;
using YangTools;
using YangTools.Log;

/// <summary>
/// 战斗管理器
/// </summary>
public class GameBattleManager : MonoSingleton<GameBattleManager>
{
    /// <summary>
    /// 瞬时击中流程
    /// </summary>
    public void HitProcess(DamageInfo damageInfo, GameActor beAtker)
    {
        if (damageInfo == null)
        {
            Debuger.ToError($"攻击信息为空,攻击流程退出");
            return;
        }
        //受击点
        Vector3 atkPos = beAtker.ClosestColliderPos(damageInfo.atkPos);
        damageInfo.behitPos = atkPos;
        damageInfo = beAtker.GetHitCompute(damageInfo);
        beAtker.BeHit(ref damageInfo);
        //damageInfo 免疫伤害需要处理
        beAtker.ShowBeHitEffect(damageInfo.beHitEffectInfo);
    }
    /// <summary>
    /// 子弹击中流程
    /// </summary>
    public void ProjectileHitProcess(BulletData bulletData, GameActor beAtker)
    {
        //伤害信息创建
        DamageInfo damageInfo = bulletData.damageInfo;
        if (damageInfo == null)
        {
            Debuger.ToError($"攻击信息为空,攻击流程退出");
            return;
        }
        //受击点
        Vector3 atkPos = beAtker.ClosestColliderPos(bulletData.owner.transform.position);
        damageInfo.behitPos = atkPos;
        damageInfo = beAtker.GetHitCompute(damageInfo);
        beAtker.BeHit(ref damageInfo);
        //damageInfo 免疫伤害需要处理
        beAtker.ShowBeHitEffect(damageInfo.beHitEffectInfo);
    }
}