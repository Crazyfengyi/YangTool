/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using YangTools;
using YangTools.MiniMap;

/// <summary>
/// 游戏角色管理类
/// </summary>
public class GameActorManager : MonoSingleton<GameActorManager>
{
    [ShowInInspector]
    //生命周期更新列表
    private static List<ICustomLife> customLives = new List<ICustomLife>();
    //预制体
    public PlayerController playerPrefab;
    private PlayerController mainPlayer;
    public PlayerController MainPlayer
    {
        get { return mainPlayer; }
    }
    [ShowInInspector]
    private static List<Monster> allMonster = new List<Monster>();
    public List<Monster> AllMonster => allMonster;

    protected override void Awake()
    {
        base.Awake();
        customLives.Clear();
        allMonster.Clear();
        CreateMainPlayer();
    }
    /// <summary>
    /// 创建主角
    /// </summary>
    public void CreateMainPlayer()
    {
        ICustomLife player = Instantiate(playerPrefab);
        if (player != null)
        {
            mainPlayer = player as PlayerController;
            customLives.Add(player);
            player.IInit();
            CameraManager.Instance.ChangeMainPlayer();
            MiniMapManager.Instance.SetMainPlayer(mainPlayer.modelRoot);

            DontDestroyOnLoad(mainPlayer);
        }
    }
    public void Update()
    {
        for (int i = 0; i < customLives.Count; i++)
        {
            customLives[i]?.IUpdate();
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
    /// <summary>
    /// 场景切换
    /// </summary>
    public void OnSceneChange(string sceneName)
    {
        for (int i = allMonster.Count - 1; i >= 0; i--)
        {
            allMonster[i].IDestroy();
        }
        allMonster.Clear();
    }
    public void CreateMonster(int id = 10001)
    {
        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Monster/Monster_{id}");
        if (obj)
        {
            ICustomLife monster = Instantiate(obj, new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10)), Quaternion.identity).GetComponent<ICustomLife>();
            customLives.Add(monster);
            allMonster.Add(monster as Monster);
            monster.IInit();
        }
    }
    public void CreateTree(Vector3 pos)
    {
        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Build/Tree");
        if (obj)
        {
            ICustomLife temp = Instantiate(obj, pos, Quaternion.identity).GetComponent<ICustomLife>();
            customLives.Add(temp);
            temp.IInit();
        }
    }
    public GameObject CreateItem(string itemName, Vector3 pos)
    {
        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Item/{itemName}");
        if (obj)
        {
            ICustomLife temp = Instantiate(obj, pos, Quaternion.identity).GetComponent<ICustomLife>();
            customLives.Add(temp);
            temp.IInit();
            return obj;
        }

        return null;
    }
    public void RemoveActor(GameActor gameActor, bool isDie = true)
    {
        switch (gameActor)
        {
            case PlayerController player:
                {

                }
                break;
            case Monster monster:
                {
                    if (isDie)
                    {
                        //怪物死亡掉落物品
                        GameActorManager.Instance.CreateItem("Apple", monster.transform.position);
                    }
                    allMonster.Remove(monster);
                }
                break;
            default:
                break;
        }

        ICustomLife iCustomLife = gameActor;
        customLives.Remove(iCustomLife);

        if (isDie) gameActor.IDie();
    }
}