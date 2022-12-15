/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-28 
*/
using UnityEngine;

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
        GameActorManager.Instance.CreateTree(new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)));
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameActorManager.Instance.CreateMonster();
        }
    }
}