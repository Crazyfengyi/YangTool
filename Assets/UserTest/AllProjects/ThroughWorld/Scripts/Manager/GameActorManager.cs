/*
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using cfg.monster;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core.YangMiniMap;
using YangTools.Scripts.Core.YangToolsManager;

/// <summary>
/// 游戏角色管理类
/// </summary>
public class GameActorManager : MonoSingleton<GameActorManager>
{
    //预制体
    public PlayerController playerPrefab;
    public PlayerController MainPlayer => mainPlayer;
    private PlayerController mainPlayer;

    public List<Monster> AllMonster => allMonster;
    [HideInEditorMode]
    [ShowInInspector]
    private static List<Monster> allMonster = new List<Monster>();
    [HideInEditorMode]
    [ShowInInspector]
    //生命周期更新列表
    private static List<ICustomLife> customLives = new List<ICustomLife>();

    protected override void Awake()
    {
        base.Awake();
        customLives.Clear();
        allMonster.Clear();
    }

    private void Start()
    {
        CreateMainPlayer();
    }
    /// <summary>
    /// 创建主角
    /// </summary>
    public void CreateMainPlayer()
    {
        ICustomLife script = Instantiate(playerPrefab);
        if (script != null)
        {
            mainPlayer = script as PlayerController;
            customLives.Add(script);
            CameraManager.Instance.SetMainPlayer(mainPlayer);
            MiniMapManager.Instance.SetMainPlayer(mainPlayer.modelRoot);
            var tableData = GameTableManager.Instance.Tables.TbPlayer.Get(1000);
            mainPlayer.SetTableData(tableData);
            mainPlayer.IInit();

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
    public void OnSceneChangeStart(string sceneName)
    {
        for (int i = allMonster.Count - 1; i >= 0; i--)
        {
            allMonster[i].IDestroy();
        }
        allMonster.Clear();
    }
    /// <summary>
    /// 怪物生成
    /// </summary>
    public Monster CreateMonster(int id = 10001)
    {
        GameObject obj = GameResourceManager.Instance.ResoruceLoad($"Monster/Monster_{id}");
        if (obj)
        {
            ICustomLife tempScript = Instantiate(obj, new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10)), Quaternion.identity).GetComponent<ICustomLife>();
            Monster monster = tempScript as Monster;
            customLives.Add(monster);
            allMonster.Add(monster);
            cfg.monster.Monster tableData = GameTableManager.Instance.Tables.TbMonster.Get(id);
            monster.SetTableData(tableData);
            monster.IInit();
            return monster;
        }

        return null;
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