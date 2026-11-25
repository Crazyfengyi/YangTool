/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-14 
*/
using System.Collections.Generic;
using cfg.skill;
using DataStruct;
using DG.Tweening;
using Pathfinding;
using UnityEngine;
using YangExtend = YangTools.Scripts.Core.YangExtend.YangExtend;

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
    // 复用候选列表 避免每次选技创建集合
    private readonly List<RunTimeSkillData> skillCandidates = new List<RunTimeSkillData>();

    #region 生命周期
    /// <summary>
    /// 设置表格数据
    /// </summary>
    public void SetTableData(cfg.monster.Monster tableData)
    {
        TableData = tableData;

        //技能列表
        SkillsList = new List<RunTimeSkillData>();
        List<SkillWeight> skillList = tableData?.SkillList;
        if (skillList == null) return;

        for (int i = 0; i < skillList.Count; i++)
        {
            if (skillList[i] == null || skillList[i].Weight <= 0) continue;
            Skill data = GameTableManager.Instance.Tables.TbSkill.Get(skillList[i].Id);
            if (data == null) continue;
            SkillsList.Add(new RunTimeSkillData
            {
                skill = data,
                weight = skillList[i].Weight
            });
        }
    }
    public override void IInit()
    {
        base.IInit();
        SkillControl.RegisterSkills(SkillsList);
        RefreshSkillApproachDistance();
        campType = ActorCampType.Monster;
        canAtkCamp = ActorCampType.PlayerAndBuilding;
        findCampType = ActorCampType.Player;

        AIPath = GetComponent<AIPath>();
        emitter = new MonsterEmitter(this);

        roleAttributeControl.ChangeAttribute(RoleAttribute.Hp, TableData.Hp);
        roleAttributeControl.ChangeAttribute(RoleAttribute.Mp, TableData.Mp);
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

    /// <summary>
    /// 冷却由角色控制器独立更新 此处只选择并提交可用技能
    /// </summary>
    public void SkillUse()
    {
        if (SkillControl == null || SkillsList == null || !isActiveAndEnabled) return;
        RefreshSkillApproachDistance();
        if (AstarPath.active == null || AIPath == null || !AIPath.isStopped || Target == null ||
            SkillControl.NeedUseSkill || SkillControl.CommonCooldownRemaining > 0) return;

        skillCandidates.Clear();
        foreach (RunTimeSkillData data in SkillsList)
        {
            if (data?.skill == null || data.weight <= 0) continue;
            if (SkillControl.CanUseSkill(data, Target) == SkillUseFailReason.None) skillCandidates.Add(data);
        }
        if (skillCandidates.Count == 0) return;
        RunTimeSkillData selected = YangExtend.GetRandomInfo(skillCandidates);
        if (selected != null && SkillControl.TryUseSkill(selected, Target, out _))
            AIFindTargetDistance = selected.skill.AtkRang;
    }

    /// <summary>
    /// 空闲时按可用技能刷新追击停止距离 等待时保持请求技能的范围
    /// </summary>
    private void RefreshSkillApproachDistance()
    {
        if (SkillControl == null || SkillsList == null) return;
        if (SkillControl.NeedUseSkill)
        {
            AIFindTargetDistance = SkillControl.CurrentRunTimeSkillData.skill.AtkRang;
            return;
        }
        float distance = 0;
        float fallback = 0;
        foreach (RunTimeSkillData data in SkillsList)
        {
            if (data?.skill == null || data.weight <= 0) continue;
            if (!SkillControl.HasSkillResources(data)) continue;
            fallback = Mathf.Max(fallback, data.skill.AtkRang);
            if (SkillControl.GetCooldownRemaining(data) <= 0)
                distance = Mathf.Max(distance, data.skill.AtkRang);
        }
        AIFindTargetDistance = Mathf.Max(0.1f, distance > 0 ? distance : fallback);
    }

    /// <summary>
    /// 保留旧公共冷却入口 控制器保证不会重复延长间隔
    /// </summary>
    public void StartCommonSkillCD()
    {
        SkillControl?.StartCommonCooldown();
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
