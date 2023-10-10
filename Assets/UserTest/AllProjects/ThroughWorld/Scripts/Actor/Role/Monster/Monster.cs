/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-14 
*/
using System.Collections.Generic;
using cfg.skill;
using DG.Tweening;
using Pathfinding;
using UnityEngine;
using YangTools.Extend;

/// <summary>
/// 怪物控制器
/// </summary>
public class Monster : RoleBase
{
    public AIPath AIPath { get; set; }
    public GameObject shootPoint;//发射点

    private EmitterBase emitter;//发射器

    private ActorCampType findCampType;//寻路目标阵营
    /// <summary>
    /// 目标物体
    /// </summary>
    public override GameObject Target
    {
        get { return target; }
    }
    /// <summary>
    /// 目标点
    /// </summary>
    public override Vector3? TargetPos
    {
        get { return targetPos; }
    }
    /// <summary>
    /// 回归欲望(100上限) TODO:需要改成字典+类统一管理
    /// </summary>
    public float returnDesire;
    public bool needReturnStartPos;

    public cfg.monster.Monster TableData { get; set; }
    public List<RunTimeSkillData> SkillsList { get; set; }
    #region 生命周期
    /// <summary>
    /// 设置表格数据
    /// </summary>
    public void SetTableData(cfg.monster.Monster tableData)
    {
        TableData = tableData;

        //技能列表
        SkillsList = new List<RunTimeSkillData>();
        List<int> skillList = tableData.SkillList;
        for (int i = 0; i < skillList.Count; i++)
        {
            Skill data = GameTableManager.Instance.Tables.TbSkill.Get(skillList[i]);
            SkillsList.Add(new RunTimeSkillData
            {
                skill = data,
            });
        }
    }
    public override void IInit()
    {
        base.IInit();
        campType = ActorCampType.Monster;
        canAtkCamp = ActorCampType.PlayerAndBuilding;
        findCampType = ActorCampType.Player;

        AIPath = GetComponent<AIPath>();
        emitter = new MonsterEmitter(this);

        roleAttributeControl.ChangeAttribute(RoleAttribute.HP, TableData.Hp);
        roleAttributeControl.ChangeAttribute(RoleAttribute.MP, TableData.Mp);
        roleAttributeControl.ChangeAttribute(RoleAttribute.Atk, TableData.Atk);
        roleAttributeControl.ChangeAttribute(RoleAttribute.Def, TableData.Def);

        roleAttributeControl.ChangeAttribute(RoleAttribute.GuardRang, TableData.GuardRang);

        roleBuffControl.Add(BuffID.buff_10001);
    }
    public override void IUpdate()
    {
        base.IUpdate();
        ReturnDesireUpdate();

        //if (AstarPath.active != null && AIPath != null && Target)
        //{
        //    AIPath.destination = Target.transform.position;
        //    if (Vector3.Distance(transform.position, Target.transform.position) < GetRoleAttribute(RoleAttribute.AtkRang))
        //    {
        //        if (AIPath.isStopped == false)
        //        {
        //            AIPath.isStopped = true;
        //            timer = 0;
        //        }

        //        ModelInfo?.Root?.transform.LookAt(Target.transform.position.SetYValue());
        //    }
        //    else
        //    {
        //        AIPath.isStopped = false;
        //        if (Target) ModelInfo?.Root?.transform.LookAt(Target.transform.position.SetYValue());
        //    }
        //    Animator.SetFloat("Speed", AIPath.velocity.magnitude);
        //}

        if (AstarPath.active != null && AIPath != null && AIPath.isStopped == true && Target)
        {
            if (SkillsList != null)
            {
                for (int i = 0; i < SkillsList.Count; i++)
                {
                    SkillsList[i].UpdateCD(Time.deltaTime);
                }

                for (int i = 0; i < SkillsList.Count; i++)
                {
                    if (SkillsList[i].currentCD <= 0)
                    {
                        SkillsList[i].ToCD();
                        SkillControl.UseSkill(SkillsList[i].skill.ResName, SkillsList[i].skill.TimelineORbehavior == 0);
                        break;
                    }
                }
            }

            //EmitData emitData = new EmitData();
            //emitData.bulletID = 0;//TODO:需要设置子弹ID
            //emitData.bulletCount = 1;
            //emitData.bulletShootType = BulletShootType.Throw;
            //emitter?.SetEmitData(emitData);
            //emitter?.StartShoot();
        }
        emitter?.OnUpdate();
    }
    public override void ILateUpdate()
    {
        base.ILateUpdate();
        //if (Animator) Animator.SetFloat("Speed", inputVector3.magnitude);
    }
    public override void IFixedUpdate()
    {
        base.IFixedUpdate();
    }
    public override void IDie()
    {
        base.IDie();
        GameSoundManager.Instance.PlaySound("Audio_Click");
        GameEffectManager.Instance.PlayEffect("DieEffect", transform.position);
        Destroy(gameObject);
    }
    public override void IDestroy()
    {
        base.IDestroy();
        Destroy(gameObject);
    }
    #endregion

    #region 情绪更新
    private float returnDesireTimer;
    private float returnDesireInterval = 1;
    /// <summary>
    /// 回归欲望
    /// </summary>
    public void ReturnDesireUpdate()
    {
        returnDesireTimer += Time.deltaTime;
        if (returnDesireTimer >= returnDesireInterval)
        {
            returnDesireTimer = 0;
            if (Vector3.Distance(StartPos, transform.position) > 10)
            {
                returnDesire++;
                if (returnDesire >= 100)
                {
                    returnDesire = 0;
                    needReturnStartPos = true;
                }
            }
        }
    }
    #endregion

    #region 搜索攻击目标
    /// <summary>
    /// 搜索攻击目标
    /// </summary>
    public override void SearchAtkTarget()
    {
        Collider[] temp = Physics.OverlapSphere(transform.position, roleAttributeControl.GetAttribute(RoleAttribute.GuardRang).Value);
        if (temp.Length > 0)
        {
            for (int i = 0; i < temp.Length; i++)
            {
                GameActor tempTarget = temp[i].gameObject.GetComponentInParent<GameActor>();
                if (tempTarget && findCampType.HasFlag(tempTarget.campType))
                {
                    target = tempTarget.gameObject;
                    targetPos = tempTarget.transform.position;
                }
            }
        }
        else
        {
            target = null;
            targetPos = null;
        }
    }
    #endregion

    //#region 攻击和被击接口实现
    //public override void Atk(AtkInfo atkInfo)
    //{
    //    if (!IsCanAtk()) return;

    //    if (atkInfo.targetActor.IsCanBeHit())
    //    {
    //        //伤害信息创建
    //        DamageInfo damageInfo = GetDamageInfo();
    //        damageInfo.atkPos = transform.position;
    //        GameBattleManager.Instance.HitProcess(damageInfo, atkInfo.targetActor);
    //        ShowAtkEffect(atkInfo.atkEffectInfo);
    //    }
    //}
    //public override void BeHit(ref DamageInfo damageInfo)
    //{
    //    healthControl.MinusHp(damageInfo);
    //}
    //public override DamageInfo GetDamageInfo()
    //{
    //    var result = new DamageInfo();
    //    result.damage = roleAttributeControl.GetAttribute(RoleAttribute.Atk).Value;
    //    return result;
    //}
    //public override DamageInfo GetHitCompute(DamageInfo damageInfo)
    //{
    //    damageInfo.damage = damageInfo.damage - roleAttributeControl.GetAttribute(RoleAttribute.Def).Value;
    //    damageInfo.damage = Mathf.Max(damageInfo.damage, 0);
    //    return damageInfo;
    //}
    //public override bool IsCanAtk()
    //{
    //    return false;
    //}
    //public override bool IsCanBeHit()
    //{
    //    return true;
    //}
    //public override void ShowAtkEffect(EffectInfo atkEffectInfo)
    //{
    //}
    //public override void ShowBeHitEffect(EffectInfo hitEffectInfo)
    //{
    //}
    //#endregion

    #region 交互
    /// <summary>
    /// 可以交互
    /// </summary>
    public override bool CanInter()
    {
        return false;
    }
    #endregion
}