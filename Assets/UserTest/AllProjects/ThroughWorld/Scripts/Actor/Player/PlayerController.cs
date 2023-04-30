/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-07-23
*/

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

    private bool isJumpAni;

    public override void IInit()
    {
        base.IInit();
        campType = ActorCampType.Player;

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
            emitData.bulletShootType = BulletShootType.None;
            emitter.SetEmitData(emitData);
            emitter.StartShoot();
        }

        emitter?.OnUpdate();
        if (Animator)
        {
            Animator.SetFloat("Speed", inputVector3.magnitude);
        }
        if (!advancedWalker.IsGrounded() && isJumpAni == false)
        {
            isJumpAni = true;
            Animator.Play("Jump", 0, 0);
        }

        //跳跃切换到地面
        if (advancedWalker.IsGrounded() && isJumpAni == true)
        {
            Animator.Play("Land");
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
        GameSoundManager.Instance.PlaySound("Audio_Click");
        GameEffectManager.Instance.PlayEffect("DieEffect", transform.position);
        Destroy(gameObject);
        UIMonoInstance.Instance.OpenUIPanel("GameOverPanel", "One");
    }

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

    #region 攻击和被击接口实现

    public override void Atk(AtkInfo atkInfo)
    {
        if (!IsCanAtk()) return;

        if (atkInfo.targetActor.IsCanBeHit())
        {
            GameBattleManager.Instance.AtkProcess(this, atkInfo.targetActor);
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
        return true;
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

    #endregion 攻击和被击接口实现
}