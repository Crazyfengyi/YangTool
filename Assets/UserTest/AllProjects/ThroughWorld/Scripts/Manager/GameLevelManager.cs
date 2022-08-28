/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-28 
*/
using UnityEngine;
using System.Collections;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance;

    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        GameActorManager.Instance.CreateMonster();

    }
}