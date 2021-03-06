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

    public override void IInit()
    {
        GameInput = new GameInputSet();
        GameInput.Player.Enable();
        GameInput.Player.Move.performed += OnMove;
        GameInput.Player.Move.canceled += OnMoveEnd;
        GameInput.Player.Interactive.performed += OnInteractive;
    }
    public override void IDie()
    {

    }

    public override void IUpdate()
    {

    }
    public override void ILateUpdate()
    {
        if (Animator) Animator.SetFloat("Speed", inputVector3.magnitude);
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
}