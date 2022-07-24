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

public class Player : RoleBase
{
    public Vector3 inputVector3;
    public override void IInit()
    {
    }
    public override void IDie()
    {

    }

    public override void IUpdate()
    {

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
        //前方
        var forward = CameraManager.Instance.mainCamera.transform.forward;
        //在xz平面的投影
        forward = Vector3.ProjectOnPlane(forward, Vector3.up);
        forward = forward.normalized;
        //右方
        var right = CameraManager.Instance.mainCamera.transform.right;
        //在xz平面的投影
        right = Vector3.ProjectOnPlane(right, Vector3.up);
        right = right.normalized;
        //方向
        Vector3 direction = forward + right;

        inputVector3 = v2;
    }
}