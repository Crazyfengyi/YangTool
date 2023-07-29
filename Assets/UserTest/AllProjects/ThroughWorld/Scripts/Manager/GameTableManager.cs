/** 
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

public class GameTableManager : MonoSingleton<GameTableManager>
{
    Tables table;
    protected override void Awake()
    {
        base.Awake();
        table = new Tables(Loader);

        //Item item = table.TbItem.Get(10000);
        //Debug.LogError($"测试:{item.Name},{item.Desc}");
    }

    private JSONNode Loader(string fileName)
    {
        return JSON.Parse(File.ReadAllText(Application.dataPath + "/Plugins/Luban/GenerateDatas/json/" + fileName + ".json"));
    }

}