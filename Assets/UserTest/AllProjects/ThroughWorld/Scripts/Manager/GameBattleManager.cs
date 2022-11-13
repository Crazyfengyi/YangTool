/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-10-15 
*/
using UnityEngine;
using System.Collections;
using YangTools;
using YangTools.Log;

public class GameBattleManager : MonoSingleton<GameBattleManager>
{
    public void AtkProcess(GameActor atker, GameActor beAtker)
    {
        DamageInfo damageInfo = atker.GetDamageInfo();
        //攻击点
        Vector3 atkPos = beAtker.ClosestColliderPos(atker.transform.position);
        damageInfo.atkPos = atkPos;
        if (damageInfo == null)
        {
            Debuger.ToError($"攻击信息为空,攻击流程退出");
            return;
        }
        damageInfo = beAtker.GetHitCompute(damageInfo);
        beAtker.BeHit(ref damageInfo);

        //damageInfo 免疫伤害需要处理
        //beAtker.ShowBeHitEffect();
    }
}