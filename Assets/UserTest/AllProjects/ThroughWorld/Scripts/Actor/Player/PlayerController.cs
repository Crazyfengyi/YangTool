/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-23 
*/
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : RoleBase
{
    public Vector3 inputVector3;
    private GameInputSet GameInput;
    public GameObject model;//模型
    public GameObject shootrPoint;//发射点

    private EmitterBase emitter;//发射器

    public override void IInit()
    {
        base.IInit();
        campType = ActorCampType.Player;

        #region 输入
        GameInput = new GameInputSet();
        GameInput.Player.Enable();
        GameInput.Player.Move.performed += OnMove;
        GameInput.Player.Move.canceled += OnMoveEnd;
        GameInput.Player.Interactive.performed += OnInteractive;
        #endregion

        #region 发射器
        emitter = new PlayerEmitter(this);
        #endregion

        roleBuffControl.Add(BuffID.buff_10001);
    }
    public override void IUpdate()
    {
        base.IUpdate();
        if (Input.GetKeyDown(KeyCode.R))
        {
            roleBuffControl.Add(BuffID.buff_10001);
        }

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    Collider[] temp = Physics.OverlapSphere(transform.position, 10);
        //    if (temp.Length > 0)
        //    {
        //        for (int i = 0; i < temp.Length; i++)
        //        {
        //            GameActor target = temp[i].gameObject.GetComponentInParent<GameActor>();
        //            if (target && target.campType == ActorCampType.Monster)
        //            {
        //                AtkInfo atkInfo = new AtkInfo();
        //                atkInfo.targetActor = target;
        //                Atk(atkInfo);
        //            }
        //        }
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.Q))
        {
            EmitData emitData = new EmitData();
            emitData.bulletID = 0;//TODO:需要设置子弹ID
            emitData.bulletCount = 6;
            emitData.bulletShootType = BulletShootType.Circle;
            emitter.SetEmitData(emitData);
            emitter.StartShoot();
        }

        emitter?.OnUpdate();
        if (Animator) Animator.SetFloat("Speed", inputVector3.magnitude);
    }
    public override void ILateUpdate()
    {
    }
    public override void IFixedUpdate()
    {

    }
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

    }
    public override DamageInfo GetDamageInfo()
    {
        var result = new DamageInfo();
        result.damage = roleAttributeControl.GetAttribute(RoleAttribute.Atk).Value;
        return result;
    }
    public override DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        return damageInfo;
    }
    public override bool IsCanAtk()
    {
        return true;
    }
    public override bool IsCanBeHit()
    {
        return false;
    }
    public override void ShowAtkEffect(EffectInfo atkEffectInfo)
    {
    }
    public override void ShowBeHitEffect(EffectInfo hitEffectInfo)
    {
    }
    #endregion

}