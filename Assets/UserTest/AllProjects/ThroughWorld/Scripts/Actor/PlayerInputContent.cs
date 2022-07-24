/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-24 
*/
using UnityEngine;
using System.Collections;
using CMF;

public class PlayerInputContent : CharacterInput
{
    private Player player;
    private void Awake()
    {
        player = GetComponent<Player>();
    }
    public override float GetHorizontalMovementInput()
    {
        return player.inputVector3.x;
    }

    public override float GetVerticalMovementInput()
    {
        return player.inputVector3.y;
    }

    public override bool IsJumpKeyPressed()
    {
        return false;
    }
}