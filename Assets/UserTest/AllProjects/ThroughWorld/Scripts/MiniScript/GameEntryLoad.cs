/* 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-11-27 
*/
using UnityEngine;
using YangTools;

public class GameEntryLoad : MonoBehaviour
{
    public void Start()
    {
        GameSceneManager.Instance.Load("GameMain");
    }
}