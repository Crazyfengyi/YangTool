/*
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-07-29 
*/
using UnityEngine;
using YangTools;
using SimpleJSON;
using System.IO;
using cfg;
using cfg.item;
using cfg.player;
using YangTools.Scripts.Core;

public class GameTableManager : MonoSingleton<GameTableManager>
{
    private Tables table;
    public Tables Tables => table;

    protected override void Awake()
    {
        base.Awake();
        table = new Tables(Loader);

        //Player item = table.TbPlayer.Get(10000);

        //Debug.LogError($"测试:{item.Name},{item.Desc}");
    }

    private JSONNode Loader(string fileName)
    {
        return JSON.Parse(File.ReadAllText(Application.dataPath + "/Plugins/Luban/GenerateDatas/json/" + fileName + ".json"));
    }

}