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
    //需要动态加载的表--例如：1000关卡表
    private readonly Dictionary<string, Object> _preloadedAssets = new();
    private readonly Dictionary<string, UniTaskCompletionSource<Object>> _loadingAssetTasksByAddress = new();
    
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
        _preloadedAssets.Clear();
        _loadingAssetTasksByAddress.Clear();
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

    #region 预加载
    /// <summary>
    /// 批量预加载资源地址。
    /// </summary>
    /// <param name="addresses">资源地址集合。</param>
    public async UniTask PreloadAssetsAsync(IEnumerable<string> addresses)
    {
        if (addresses == null)
        {
            return;
        }

        HashSet<string> uniqueAddresses = new HashSet<string>();
        List<UniTask<UnityEngine.Object>> loadingTasks = new List<UniTask<UnityEngine.Object>>();
        foreach (string address in addresses)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                continue;
            }

            string key = NormalizeAssetKey(address);
            if (uniqueAddresses.Add(key))
            {
                loadingTasks.Add(LoadAssetAsync<UnityEngine.Object>(address));
            }
        }

        if (loadingTasks.Count > 0)
        {
            await UniTask.WhenAll(loadingTasks);
        }
    }
    
    /// <summary>
    /// 按资源地址异步加载并缓存资源。
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="address">资源地址</param>
    /// <returns>加载成功的资源，失败返回null</returns>
    public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        string key = NormalizeAssetKey(address);
        if (_preloadedAssets.TryGetValue(key, out UnityEngine.Object cachedAsset))
        {
            return cachedAsset as T;
        }

        if (_loadingAssetTasksByAddress.TryGetValue(key,
                out UniTaskCompletionSource<UnityEngine.Object> loadingTask))
        {
            return await CastAssetAsync<T>(loadingTask.Task);
        }

        UniTaskCompletionSource<UnityEngine.Object> completionSource =
            new UniTaskCompletionSource<UnityEngine.Object>();
        _loadingAssetTasksByAddress[key] = completionSource;
        LoadAssetInternalAsync(address, key, completionSource).Forget();
        return await CastAssetAsync<T>(completionSource.Task);
    }
    
    /// <summary>
    /// 获取已预加载的资源。
    /// </summary>
    /// <typeparam name="T">资源类型。</typeparam>
    /// <param name="address">资源地址。</param>
    /// <returns>缓存资源，不存在时返回null。</returns>
    public T GetPreloadedAsset<T>(string address) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        return _preloadedAssets.TryGetValue(NormalizeAssetKey(address), out UnityEngine.Object asset)
            ? asset as T
            : null;
    }

    /// <summary>
    /// 检查资源是否已经预加载。
    /// </summary>
    /// <param name="address">资源地址。</param>
    /// <returns>已缓存返回true。</returns>
    public bool IsAssetPreloaded(string address)
    {
        return !string.IsNullOrWhiteSpace(address) && _preloadedAssets.ContainsKey(NormalizeAssetKey(address));
    }
    
    /// <summary>
    /// 规范化资源键值
    /// 去除首尾空格并将字符串转换为小写，
    /// 如果文件扩展名是.json，则返回不带扩展名的文件名，否则返回处理后的字符串
    /// </summary>
    /// <param name="value">需要规范化的字符串值</param>
    /// <returns>返回规范化后的字符串</returns>
    private static string NormalizeAssetKey(string value)
    {
        // 去除字符串首尾空格并将字符串转换为小写
        string key = value.Trim().ToLowerInvariant();
        // 判断文件扩展名是否为.json
        // 如果是，则返回不带扩展名的文件名
        // 否则，返回处理后的key值
        return Path.GetExtension(key) == ".json" ? Path.GetFileNameWithoutExtension(key) : key;
    }
    /// <summary>
    /// 将通用资源加载任务转换为指定资源类型。
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="assetTask">通用资源加载任务</param>
    /// <returns>指定类型资源</returns>
    private static async UniTask<T> CastAssetAsync<T>(UniTask<UnityEngine.Object> assetTask)
        where T : UnityEngine.Object
    {
        UnityEngine.Object asset = await assetTask;
        return asset as T;
    }
    /// <summary>
    /// 执行通用资源加载并写入缓存。
    /// </summary>
    /// <param name="address">资源地址</param>
    /// <param name="key">标准化后的缓存键</param>
    /// <returns>加载成功的文本资源，失败返回null</returns>
    private async UniTaskVoid LoadAssetInternalAsync(string address, string key, UniTaskCompletionSource<Object> completionSource)
    {
        try
        {
            var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
            AssetHandle handle = package.LoadAssetAsync<UnityEngine.Object>(address);
            await handle.ToUniTask();
            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError(handle.LastError);
                completionSource.TrySetResult(null);
                return;
            }

            UnityEngine.Object asset = handle.AssetObject;
            if (asset == null)
            {
                completionSource.TrySetResult(null);
                return;
            }

            _preloadedAssets[key] = asset;
            _preloadedAssets[NormalizeAssetKey(asset.name)] = asset;
            completionSource.TrySetResult(asset);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"加载资源失败:{address} {exception.Message}");
            completionSource.TrySetResult(null);
        }
        finally
        {
            _loadingAssetTasksByAddress.Remove(key);
        }
    }
    #endregion
}