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

    public float AIFindTargetDistance { get; set; }
    public cfg.monster.Monster TableData { get; set; }
    public List<RunTimeSkillData> SkillsList { get; set; }
    //公共CD
    private bool startSkillCommonCD;
    private float skillCommonCDTimer;
    private float skillCommonCDInterval = 2;

    #region 生命周期
    /// <summary>
    /// 设置表格数据
    /// </summary>
    public void SetTableData(cfg.monster.Monster tableData)
    {
        TableData = tableData;

        //技能列表
        SkillsList = new List<RunTimeSkillData>();
        List<SkillWeight> skillList = tableData.SkillList;

        for (int i = 0; i < skillList.Count; i++)
        {
            Skill data = GameTableManager.Instance.Tables.TbSkill.Get(skillList[i].Id);
            SkillsList.Add(new RunTimeSkillData
            {
                skill = data,
                weight = skillList[i].Id
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
        SkillUse();
        //EmitData emitData = new EmitData();
        //emitData.bulletID = 0;//TODO:需要设置子弹ID
        //emitData.bulletCount = 1;
        //emitData.bulletShootType = BulletShootType.Throw;
        //emitter?.SetEmitData(emitData);
        //emitter?.StartShoot();
        emitter?.OnUpdate();
    }

    public void SkillUse()
    {
        if (startSkillCommonCD)
        {
            skillCommonCDTimer -= Time.deltaTime;
        }

        if (AstarPath.active != null && AIPath != null && AIPath.isStopped == true && Target)
        {
            for (int i = 0; i < SkillsList.Count; i++)
            {
                SkillsList[i].UpdateCD(Time.deltaTime);
            }

            if (SkillsList != null && SkillControl.NeedUseSkill == false && skillCommonCDTimer <= 0)
            {
                var canUseSkills = new List<RunTimeSkillData>();
                for (int i = 0; i < SkillsList.Count; i++)
                {
                    if (SkillsList[i].currentCD <= 0)
                    {
                        canUseSkills.Add(SkillsList[i]);
                    }
                }

                RunTimeSkillData target = YangExtend.GetRandomInfo(canUseSkills);
                if (target != null)
                {
                    target.ToCD();
                    AIFindTargetDistance = target.skill.AtkRang;
                    SkillControl.UseSkill(target);
                    skillCommonCDTimer = skillCommonCDInterval;
                    startSkillCommonCD = false;
                }
            }
        }
    }

    /// <summary>
    /// 开始计算技能公共CD(间隔CD)
    /// </summary>
    public void StartCommonSkillCD()
    {
        startSkillCommonCD = true;
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
            bool haveTarget = false;
            for (int i = 0; i < temp.Length; i++)
            {
                GameActor tempTarget = temp[i].gameObject.GetComponentInParent<GameActor>();
                if (tempTarget && findCampType.HasFlag(tempTarget.campType))
                {
                    haveTarget = true;
                    target = tempTarget.gameObject;
                    targetPos = tempTarget.transform.position;
                }
            }

            if (!haveTarget)
            {
                target = null;
                targetPos = null;
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

    private void OnDrawGizmosSelected()
    {

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(transform.position, GetRoleAttributeValue(RoleAttribute.GuardRang));
        }
#endif

    }
}