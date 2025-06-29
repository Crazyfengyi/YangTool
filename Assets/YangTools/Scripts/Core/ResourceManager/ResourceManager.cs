/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       WIN-VJ19D9AB7HB
 *UnityVersion：2023.2.0b16
 *创建时间:         2023-12-10
*/

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YooAsset;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace YangTools.Scripts.Core.ResourceManager
{
    public class ResourceManager
    {
        private static Dictionary<string, Object> assetCacheDict = new Dictionary<string, Object>();

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
        public static async UniTask<GameObject> InstantiateGameObject(string location, Transform parent, bool inWorldSpace)
        {
            GameObject prefab = await LoadAssetAsync<GameObject>(location);
            GameObject instance = null;
            instance = parent != null ? Object.Instantiate(prefab, parent, inWorldSpace) : Object.Instantiate(prefab);
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
        public static async UniTask<T> LoadAssetAsync<T>(string location, System.IProgress<float> progress = null,
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

            Debug.LogError($"类型转换失败location:{location}资源类型:{handler.AssetObject.GetType()}加载类型:{typeof(T)}");
            return null;
        }

        public static async UniTask<TObject> LoadSubAssetAsync<TObject>(string location, string assetName,
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
                Debug.LogError($"不存在的资源类型location:{location} assetName:{assetName} TObject:{typeof(TObject)}");
            }

            return subAsset;
        }

        public static async UniTask<YooAsset.SceneHandle> LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single,LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.Physics3D , bool suspendLoad = false, uint priority = 100)
        {
            Debug.Log($"YooAssets场景加载---{location}---"); 
            SceneHandle handler = YooAsset.YooAssets.LoadSceneAsync(location, sceneMode,localPhysicsMode,suspendLoad, priority);
            await handler.ToUniTask();

            return handler;
        }

        public static async UniTask<Sprite> LoadSprite(string location)
        {
            return await LoadAssetAsync<Sprite>(location);
        }

        public static async void SetImageSprite(Image image, string spriteLocation, System.Action<Image> success = null)
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

        public static async UniTask<AudioClip> LoadAudioClip(string location)
        {
            return await LoadAssetAsync<AudioClip>(location);
        }
    }
}