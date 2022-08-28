/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using UnityEngine;
using System.Collections;
using YangTools;
using System.Collections.Generic;
/// <summary>
/// 游戏角色管理类
/// </summary>
public class GameActorManager : MonoSingleton<GameActorManager>
{
    //生命周期更新列表
    private static List<ICustomLife> customLives = new List<ICustomLife>();
    //预制体
    public PlayerController playerPrefab;
    private PlayerController mainPlayer;
    public PlayerController MainPlayer
    {
        get { return mainPlayer; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        _ = Instance;
    }
    protected override void Awake()
    {
        base.Awake();

    }
    public void Start()
    {
        ICustomLife player = Instantiate(playerPrefab);
        if (player != null)
        {
            customLives.Add(player);
            player.IInit();
            mainPlayer = player as PlayerController;
            DontDestroyOnLoad(mainPlayer);
        }

        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Monster_{10001}");
        if (obj)
        {
            ICustomLife monster = Instantiate(obj, new Vector3(10f, 0f, 0f), Quaternion.identity).GetComponent<ICustomLife>();
            customLives.Add(monster);
            monster.IInit();
        }
    }
    public void Update()
    {
        for (int i = 0; i < customLives.Count; i++)
        {
            customLives[i].IUpdate();
        }
    }
    public void LateUpdate()
    {
        for (int i = 0; i < customLives.Count; i++)
        {
            customLives[i].ILateUpdate();
        }
    }
    public void FixedUpdate()
    {
        for (int i = 0; i < customLives.Count; i++)
        {
            customLives[i].IFixedUpdate();
        }
    }
    public void CreateMonster(int id = 10001)
    {
        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Monster_{id}");
        if (obj)
        {
            ICustomLife monster = Instantiate(obj, new Vector3(10f, 0f, 0f), Quaternion.identity).GetComponent<ICustomLife>();
            customLives.Add(monster);
            monster.IInit();
        }
    }
}