/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-14 
*/
using Pathfinding;
using UnityEngine;

/// <summary>
/// 怪物控制器
/// </summary>
public class Monster : RoleBase
{
    private AIPath aiPath;
    public GameObject model;//模型
    public GameObject shootPoint;//发射点

    private EmitterBase emitter;//发射器
    private float timer;
    private float interval = 1f;

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

    public override void IInit()
    {
        base.IInit();
        campType = ActorCampType.Monster;
        canAtkCamp = ActorCampType.PlayerAndBuilding;

        aiPath = GetComponent<AIPath>();
        emitter = new MonsterEmitter(this);

        roleAttributeControl.ChangeAttribute(RoleAttribute.AtkRang, Random.Range(6, 12));
        roleBuffControl.Add(BuffID.buff_10001);
    }
    public override void IUpdate()
    {
        base.IUpdate();

        if (AstarPath.active != null && aiPath != null && GameActorManager.Instance.MainPlayer != null)
        {
            aiPath.destination = GameActorManager.Instance.MainPlayer.transform.position;
            if (Vector3.Distance(transform.position, GameActorManager.Instance.MainPlayer.transform.position) < GetRoleAttribute(RoleAttribute.AtkRang))
            {
                if (aiPath.isStopped != true)
                {
                    aiPath.isStopped = true;
                    timer = 0;
                }

                Vector3 targetPos = GameActorManager.Instance.MainPlayer.transform.position;
                targetPos.y = 0;
                model.transform.LookAt(targetPos);
            }
            else
            {
                aiPath.isStopped = false;
            }
            Animator.SetFloat("Speed", aiPath.velocity.magnitude);
        }

        if (AstarPath.active != null && aiPath != null && aiPath.isStopped == true && GameActorManager.Instance.MainPlayer != null)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0;
                EmitData emitData = new EmitData();
                emitData.bulletID = 0;//TODO:需要设置子弹ID
                emitData.bulletCount = 1;
                emitData.bulletShootType = BulletShootType.Throw;
                emitter?.SetEmitData(emitData);
                emitter?.StartShoot();
            }
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

    #region 攻击和被击接口实现
    public override void Atk(AtkInfo atkInfo)
    {
        if (!IsCanAtk()) return;

        if (atkInfo.targetActor.IsCanBeHit())
        {
            //伤害信息创建
            DamageInfo damageInfo = GetDamageInfo();
            damageInfo.atkPos = transform.position;
            GameBattleManager.Instance.HitProcess(damageInfo, atkInfo.targetActor);
            ShowAtkEffect(atkInfo.atkEffectInfo);
        }
    }
    public override void BeHit(ref DamageInfo damageInfo)
    {
        healthControl.MinusHp(damageInfo);
    }
    public override DamageInfo GetDamageInfo()
    {
        var result = new DamageInfo();
        result.damage = roleAttributeControl.GetAttribute(RoleAttribute.Atk).Value;
        return result;
    }
    public override DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        damageInfo.damage = damageInfo.damage - roleAttributeControl.GetAttribute(RoleAttribute.Def).Value;
        damageInfo.damage = Mathf.Max(damageInfo.damage, 0);
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