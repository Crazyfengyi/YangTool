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
using Cysharp.Threading.Tasks;
using YangTools.Scripts.Core;
using YooAsset;

public class GameTableManager : MonoSingleton<GameTableManager>
{
    private Tables table;
    public Tables Tables => table;
    
    private readonly Dictionary<string, TextAsset> _jsonAssets = new();
    
    public async UniTask LoadAllConfigs()
    {
        var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
        var assetInfos = package.GetAssetInfos("LubanData");
        foreach (var assetInfo in assetInfos)
        {
            AssetHandle handle = package.LoadAssetAsync<TextAsset>(assetInfo.Address);
            await handle.ToUniTask();
            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError(handle.LastError);
                continue;
            }
            TextAsset config = handle.AssetObject as TextAsset;
            _jsonAssets[config.name.ToLower()] = config;
        }
        table = new Tables(LoadJson);
    }

    private JSONNode LoadJson(string fileName)
    {
        string key = fileName.ToLower();
        if (_jsonAssets.TryGetValue(key, out TextAsset textAsset))
        {
            return JSON.Parse(textAsset.text);
        }
        Debug.LogError($"不存在的配置文件:{fileName}");
        return null;
    }
}