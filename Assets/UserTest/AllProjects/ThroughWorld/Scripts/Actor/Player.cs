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
    private Rigidbody rb;

    public override void IInit()
    {
        rb = GetComponentInChildren<Rigidbody>(true);
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
        v2 = v2 * 10;
        rb.velocity = new Vector3(v2.x, 0, v2.y);
    }
}