/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       WIN-VJ19D9AB7HB
 *UnityVersion：2023.2.0b16
 *创建时间:         2023-12-10
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class RescoreManager
{
    private Dictionary<string, Object> assetCacheDict;

    protected void OnEnter()
    {
        assetCacheDict = new Dictionary<string, Object>();
    }

    protected  void OnExit()
    {
        assetCacheDict.Clear();
        assetCacheDict = null;
    }
    /// <summary>
    /// 异步实例化
    /// </summary>
    /// <param name="location">地址</param>
    /// <param name="parent">父节点</param>
    /// <param name="inWorldSpace">世界空间</param>
    /// <returns></returns>
    public async UniTask<GameObject> InstantiateGameObject(string location, Transform parent, bool inWorldSpace)
    {
        GameObject prefab = await LoadAssetAsync<GameObject>(location);
        GameObject instance = null;
        if (parent != null)
        {
            instance = GameObject.Instantiate(prefab, parent, inWorldSpace);
        }
        else
        {
            instance = GameObject.Instantiate(prefab);
        }

        return instance;
    }
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="location"></param>
    /// <param name="progress"></param>
    /// <param name="timing"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async UniTask<T> LoadAssetAsync<T>(string location, System.IProgress<float> progress = null,
        PlayerLoopTiming timing = PlayerLoopTiming.Update) where T : Object
    {
        if (assetCacheDict.TryGetValue(location, out var asset))
        {
            if (asset != null && asset is T tAsset)
            {
                return tAsset;
            }
            else
            {
                Debug.LogError($"资源缓存 is Null location:{location}");
            }
        }

        // Debugger.Log($"加载资源:{location}", Debugger.ELogColor.Orange);

        var handler = YooAsset.YooAssets.LoadAssetAsync<T>(location);

        await handler.ToUniTask(progress, timing);

        if (handler.AssetObject is T obj)
        {
            assetCacheDict[location] = obj;

            return obj;
        }

        Debug.LogError($"类型转换失败 location:{location} 资源类型:{handler.AssetObject.GetType()} 加载类型:{typeof(T)}");
        return null;
    }

    public async UniTask<TObject> LoadSubAssetAsync<TObject>(string location, string assetName,
        System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        where TObject : Object
    {
        if (assetCacheDict.TryGetValue(location, out var asset))
        {
            if (asset != null && asset is TObject tAsset)
            {
                return tAsset;
            }
            else
            {
                Debug.LogError($"资源缓存=NULL location:{location}");
            }
        }

        // Debug.Log($"加载资源:{location}", Debug.ELogColor.Orange);

        var handler = YooAsset.YooAssets.LoadSubAssetsAsync<TObject>(location);

        await handler.ToUniTask(progress, timing);

        var subAsset = handler.GetSubAssetObject<TObject>(location);

        if (subAsset == null)
        {
            Debug.LogError($"不存在的资源类型 location:{location} assetName:{assetName} TObject:{typeof(TObject)}");
        }

        return subAsset;
    }

    public async UniTask<YooAsset.SceneHandle> LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single,LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.Physics3D , bool suspendLoad = false, uint priority = 100)
    {
        Debug.Log($"YooAssets 场景加载 ---{location}---");
        SceneHandle handler = YooAsset.YooAssets.LoadSceneAsync(location, sceneMode, localPhysicsMode,suspendLoad, priority);
        await handler.ToUniTask();

        return handler;
    }

    public async UniTask<Sprite> LoadSprite(string location)
    {
        return await LoadAssetAsync<Sprite>(location);
    }

    public async void SetImageSprite(Image image, string spriteLocation, System.Action<Image> success = null)
    {
        if (image == null)
        {
            Debug.LogError("参数错误 Image = NUll");
            return;
        }

        if (string.IsNullOrEmpty(spriteLocation))
        {
            Debug.LogError("参数错误 spriteLocation = NUll");
            return;
        }

        if (image.sprite != null && image.sprite.name.Equals(spriteLocation))
        {
            return;
        }

        var sprite = await LoadSprite(spriteLocation);

        if (image != null)
        {
            image.sprite = sprite;
            success?.Invoke(image);
        }
    }

    public async UniTask<AudioClip> LoadAudioClip(string location)
    {
        return await LoadAssetAsync<AudioClip>(location);
    }
}