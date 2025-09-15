/*
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-07-29 
*/

using System.Collections.Generic;
using UnityEngine;
using YangTools;
using SimpleJSON;
using System.IO;
using Bright.Serialization;
using cfg;
using cfg.item;
using cfg.player;
using Cysharp.Threading.Tasks;
using YangTools.Scripts.Core;

public class GameTableManager : MonoSingleton<GameTableManager>
{
    private Tables table;
    public Tables Tables => table;
    
    private readonly Dictionary<string, TextAsset> _bytesAssets = new ();
    
    protected override void Awake()
    {
        base.Awake();
        table = new Tables(Loader);
        //Player item = table.TbPlayer.Get(10000);
        //Debug.LogError($"测试:{item.Name},{item.Desc}");
    }

    private JSONNode Loader(string fileName)
    {
        return JSON.Parse(File.ReadAllText(Application.dataPath + "/YangTools/LubanData/GenerateDatas/json/" + fileName + ".json"));
    }
    
    //备选 另一种方案
    // public async UniTask LoadAllConfigs()
    // {
    //     LoadAssetsByTagOperation<TextAsset> loadExcel = new LoadAssetsByTagOperation<TextAsset>("LubanData");
    //
    //     YooAsset.YooAssets.StartOperation(loadExcel);
    //
    //     await loadExcel.ToUniTask();
    //
    //     foreach (var config in loadExcel.AssetObjects)
    //     {
    //         Debug.Log($"加载配置文件:{config.name}");
    //         _bytesAssets.Add(config.name.ToLower(), config);
    //     }
    //
    //     table = new Tables(LoadByteBuf);
    // }
    //
    // private ByteBuf LoadByteBuf(string fileName)
    // {
    //
    //     if (_bytesAssets.TryGetValue(fileName, out var buff))
    //     {
    //         return new ByteBuf(buff.bytes);
    //     }
    //
    //     Debug.LogError($"不存在的配置文件:{fileName}");
    //     return null;
    // }
}