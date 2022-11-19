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
    private static List<Monster> allMonster = new List<Monster>();
    public List<Monster> AllMonster => allMonster;

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
            mainPlayer = player as PlayerController;
            customLives.Add(player);
            player.IInit();
            DontDestroyOnLoad(mainPlayer);
        }

        GameActorManager.Instance.CreateMonster();
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
    /// <summary>
    /// 场景切换
    /// </summary>
    public void OnSceneChange()
    {
        for (int i = 0; i < allMonster.Count; i++)
        {
            allMonster[i].IDestroy();
        }
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
        if (isDie)
        {
            gameActor.IDie();
        }
        switch (gameActor)
        {
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
    }
}