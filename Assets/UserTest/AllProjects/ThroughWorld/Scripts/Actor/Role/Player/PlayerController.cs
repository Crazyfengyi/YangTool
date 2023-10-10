/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-07-23
*/

using System;
using CMF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using YangTools;
using YangTools.UGUI;

public class PlayerController : RoleBase
{
    public Vector3 inputVector3;
    public bool isJumpPressed;//跳跃键按下

    public GameObject modelRoot;//模型父节点
    public GameObject model;//模型
    public GameObject shootPoint;//发射点

    private EmitterBase emitter;//发射器
    private AdvancedWalkerController advancedWalker;//移动脚本

    #region 事件
    protected Action<BulletBase, EmitterBase> ShootBulletEvent { get; set; }
    /// <summary>
    /// 子弹生成回调
    /// </summary>
    void OnBulletCreated(BulletBase bullet, EmitterBase emitterBase)
    {
        ShootBulletEvent?.Invoke(bullet, emitterBase);
    }
    #endregion

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

    private bool isJumpAni;

    public override void IInit()
    {
        base.IInit();
        campType = ActorCampType.Player;
        canAtkCamp = ActorCampType.MonsterAndBuilding;

        GameInputSet gameInput = GameInputManager.Instance.GameInput;
        gameInput.Player.Move.performed += OnMove;
        gameInput.Player.Move.canceled += OnMoveEnd;
        gameInput.Player.Interactive.performed += OnInteractive;
        gameInput.Player.Jump.performed += OnJump;
        gameInput.Player.Jump.canceled += OnJump;

        emitter = new PlayerEmitter(this);
        advancedWalker = GetComponent<AdvancedWalkerController>();
        advancedWalker.cameraTransform = CameraManager.Instance.CameraLeftRightTransform;
        roleBuffControl.Add(BuffID.buff_10001);

        roleAttributeControl.ChangeAttribute(RoleAttribute.GuardRang, 20);
    }

    public override void IUpdate()
    {
        base.IUpdate();

        if (Input.GetKeyDown(KeyCode.R))
        {
            roleBuffControl.Add(BuffID.buff_10001);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            EmitData emitData = new EmitData();
            emitData.bulletID = 0;//TODO:需要设置子弹ID
            emitData.bulletCount = 6;
            emitData.timeInterval = 0;
            emitData.bulletShootType = BulletShootType.Circle;
            emitter.SetEmitData(emitData);
            emitter.StartShoot(OnBulletCreated);
        }

        emitter?.OnUpdate();
        if (Animator)
        {
            Animator.SetFloat("Speed", inputVector3.magnitude);
        }
        if (!advancedWalker.IsGrounded() && isJumpAni == false)
        {
            isJumpAni = true;
            AniControl.Play("Jump", 0, 0);
        }

        //跳跃切换到地面
        if (advancedWalker.IsGrounded() && isJumpAni == true)
        {
            AniControl.Play("Land");
            isJumpAni = false;
        }
    }

    public override void ILateUpdate()
    {
    }

    public override void IFixedUpdate()
    {
    }

    public override void IDie()
    {
        base.IDie();
        GameSoundManager.Instance.PlaySound("Audio_Click");
        GameEffectManager.Instance.PlayEffect("DieEffect", transform.position);
        Destroy(gameObject);
        UIMonoInstance.Instance.OpenUIPanel("GameOverPanel", "One");
        YangToolsManager.SetCursorLock(false);
    }

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
                if (tempTarget && canAtkCamp.HasFlag(tempTarget.campType))
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

    #region 输入

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 v2 = context.ReadValue<Vector2>();
        inputVector3 = v2;
    }

    public void OnMoveEnd(InputAction.CallbackContext context)
    {
        inputVector3 = Vector3.zero;
    }

    public void OnInteractive(InputAction.CallbackContext context)
    {
        InteractorSystem.Instance.OnInter();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        isJumpPressed = value > 0;
    }

    #endregion 输入

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
    //    return true;
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

    //#endregion 攻击和被击接口实现
}