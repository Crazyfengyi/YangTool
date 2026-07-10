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
using SceneHandle = YooAsset.SceneHandle;

namespace YangTools.Scripts.Core.ResourceManager
{
    public abstract class ResourceManager
    {
        /// <summary>
        /// 资源缓存条目。
        /// </summary>
        private sealed class AssetCacheEntry
        {
            public HandleBase Handle;
            public int ReferenceCount;
            public bool IsPreloaded;
            public bool IsReferenceCounted;
        }

        private static readonly Dictionary<string, Object> AssetCacheDict = new Dictionary<string, Object>();
        private static readonly Dictionary<string, HandleBase> AssetHandleCacheDict = new Dictionary<string, HandleBase>();
        private static readonly Dictionary<string, AssetCacheEntry> AssetEntryDict = new Dictionary<string, AssetCacheEntry>();
        /// <summary>
        /// 清理资源缓存
        /// </summary>
        public async UniTask ClearCache()
        {
            foreach (HandleBase handle in AssetHandleCacheDict.Values)
            {
                ReleaseHandle(handle);
            }

            AssetHandleCacheDict.Clear();
            AssetEntryDict.Clear();
            AssetCacheDict.Clear();

            try
            {
                if (YooAssets.Initialized)
                {
                    ResourcePackage package = YooAssets.TryGetPackage("DefaultPackage");
                    if (package != null)
                    {
                        await package.UnloadUnusedAssetsAsync().ToUniTask();
                    }
                }
            }
            finally
            {
                try
                {
                    await Resources.UnloadUnusedAssets().ToUniTask();
                }
                finally
                {
                    System.GC.Collect();
                }
            }
        }
        
        #region 清理无用资源

        /// <summary>
        /// 释放资源句柄并清理无用资源和内存。
        /// </summary>
        public async UniTask UnloadUnusedResourcesAsync()
        {
            List<KeyValuePair<string, AssetCacheEntry>> entries =
                new List<KeyValuePair<string, AssetCacheEntry>>(AssetEntryDict);
            for (int i = 0; i < entries.Count; i++)
            {
                KeyValuePair<string, AssetCacheEntry> item = entries[i];
                AssetCacheEntry entry = item.Value;
                if (entry.IsReferenceCounted && entry.ReferenceCount <= 0 && !entry.IsPreloaded)
                {
                    RemoveUnusedAsset(item.Key, entry);
                }
            }

            try
            {
                if (YooAssets.Initialized)
                {
                    ResourcePackage package = YooAssets.TryGetPackage("DefaultPackage");
                    if (package != null)
                    {
                        await package.UnloadUnusedAssetsAsync().ToUniTask();
                    }
                }
            }
            finally
            {
                try
                {
                    await Resources.UnloadUnusedAssets().ToUniTask();
                }
                finally
                {
                    System.GC.Collect();
                }
            }
        }
        
        /// <summary>
        /// 获取资源并增加引用计数。
        /// </summary>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <param name="location">资源地址。</param>
        /// <param name="progress">加载进度。</param>
        /// <param name="timing">异步任务的 PlayerLoop 时机。</param>
        /// <returns>资源对象。</returns>
        public static async UniTask<T> AcquireAssetAsync<T>(string location,
            System.IProgress<float> progress = null,
            PlayerLoopTiming timing = PlayerLoopTiming.Update) where T : Object
        {
            T asset = await LoadAssetAsync<T>(location, progress, timing);
            if (asset == null)
            {
                return null;
            }

            AssetCacheEntry entry = GetOrCreateEntry(location);
            entry.IsReferenceCounted = true;
            entry.ReferenceCount++;
            return asset;
        }

        /// <summary>
        /// 获取子资源并增加引用计数。
        /// </summary>
        /// <typeparam name="TObject">子资源类型。</typeparam>
        /// <param name="location">资源地址。</param>
        /// <param name="assetName">子资源名称。</param>
        /// <param name="progress">加载进度。</param>
        /// <param name="timing">异步任务的 PlayerLoop 时机。</param>
        /// <returns>子资源对象。</returns>
        public static async UniTask<TObject> AcquireSubAssetAsync<TObject>(string location, string assetName,
            System.IProgress<float> progress = null,
            PlayerLoopTiming timing = PlayerLoopTiming.Update) where TObject : Object
        {
            TObject asset = await LoadSubAssetAsync<TObject>(location, assetName, progress, timing);
            if (asset == null)
            {
                return null;
            }

            AssetCacheEntry entry = GetOrCreateEntry(location);
            entry.IsReferenceCounted = true;
            entry.ReferenceCount++;
            return asset;
        }

        /// <summary>
        /// 释放资源引用，引用计数归零后资源会被标记为可清理。
        /// </summary>
        /// <param name="location">资源地址。</param>
        public static void ReleaseAsset(string location)
        {
            if (!AssetEntryDict.TryGetValue(location, out AssetCacheEntry entry) || !entry.IsReferenceCounted)
            {
                return;
            }

            entry.ReferenceCount = Mathf.Max(0, entry.ReferenceCount - 1);
        }

        /// <summary>
        /// 取消资源的预加载保留标记。
        /// </summary>
        /// <param name="location">资源地址。</param>
        public static void ReleasePreloadedAsset(string location)
        {
            if (AssetEntryDict.TryGetValue(location, out AssetCacheEntry entry))
            {
                entry.IsPreloaded = false;
            }
        }
        
        /// <summary>
        /// 安全释放资源句柄。
        /// </summary>
        /// <param name="handle">待释放的资源句柄。</param>
        private static void ReleaseHandle(HandleBase handle)
        {
            if (handle == null || !handle.IsValid)
            {
                return;
            }

            try
            {
                handle.Release();
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"释放资源句柄失败：{exception.Message}");
            }
        }

        /// <summary>
        /// 缓存资源句柄，并释放同地址的旧句柄。
        /// </summary>
        /// <param name="location">资源地址。</param>
        /// <param name="handle">资源句柄。</param>
        private static void CacheHandle(string location, HandleBase handle)
        {
            if (AssetHandleCacheDict.TryGetValue(location, out HandleBase oldHandle) && oldHandle != handle)
            {
                ReleaseHandle(oldHandle);
            }

            AssetHandleCacheDict[location] = handle;
            GetOrCreateEntry(location).Handle = handle;
        }

        /// <summary>
        /// 获取或创建资源缓存条目。
        /// </summary>
        /// <param name="location">资源地址。</param>
        /// <returns>资源缓存条目。</returns>
        private static AssetCacheEntry GetOrCreateEntry(string location)
        {
            if (!AssetEntryDict.TryGetValue(location, out AssetCacheEntry entry))
            {
                entry = new AssetCacheEntry();
                AssetEntryDict[location] = entry;
            }

            return entry;
        }

        /// <summary>
        /// 标记资源为预加载资源，在清理无用资源时保留。
        /// </summary>
        /// <param name="location">资源地址。</param>
        private static void MarkAssetAsPreloaded(string location)
        {
            GetOrCreateEntry(location).IsPreloaded = true;
        }

        /// <summary>
        /// 移除指定的未使用资源缓存。
        /// </summary>
        /// <param name="location">资源地址。</param>
        private static void RemoveUnusedAsset(string location, AssetCacheEntry entry)
        {
            ReleaseHandle(entry.Handle);
            AssetEntryDict.Remove(location);
            AssetHandleCacheDict.Remove(location);
            AssetCacheDict.Remove(location);
        }
        #endregion

        #region 资源加载

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="location"></param>
        /// <param name="progress"></param>
        /// <param name="timing"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask<T> LoadAssetAsync<T>(string location, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update) where T : Object
        {
            if (AssetCacheDict.TryGetValue(location, out var asset))
            {
                if (asset != null && asset is T tAsset)
                {
                    return tAsset;
                }

                Debug.LogError($"资源缓存 is Null location:{location}");
            }

            Debug.Log($"加载资源:{location}");
            AssetHandle handler = YooAssets.LoadAssetAsync<T>(location);
            try
            {
                await handler.ToUniTask(progress, timing);
            }
            catch
            {
                ReleaseHandle(handler);
                throw;
            }

            if (handler.AssetObject is T obj)
            {
                AssetCacheDict[location] = obj;
                CacheHandle(location, handler);
                return obj;
            }

            System.Type actualType = handler.AssetObject?.GetType();
            ReleaseHandle(handler);
            Debug.LogError($"类型转换失败location:{location}资源类型:{actualType}加载类型:{typeof(T)}");
            return null;
        }

        /// <summary>
        /// 预加载资源，并将资源写入资源缓存。
        /// </summary>
        /// <param name="locations">资源地址集合。</param>
        /// <param name="progress">预加载进度。</param>
        /// <param name="timing">异步任务的 PlayerLoop 时机。</param>
        public static async UniTask PreloadAssetsAsync(IEnumerable<string> locations,
            System.IProgress<float> progress = null,
            PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            if (locations == null)
            {
                return;
            }

            List<string> preloadLocations = new List<string>();
            HashSet<string> uniqueLocations = new HashSet<string>();
            foreach (string location in locations)
            {
                if (!string.IsNullOrWhiteSpace(location) && uniqueLocations.Add(location))
                {
                    preloadLocations.Add(location);
                }
            }

            for (int i = 0; i < preloadLocations.Count; i++)
            {
                await LoadAssetAsync<Object>(preloadLocations[i], timing: timing);
                MarkAssetAsPreloaded(preloadLocations[i]);
                progress?.Report((i + 1f) / preloadLocations.Count);
            }
        }

        /// <summary>
        /// 加载子资源
        /// </summary>
        /// <param name="location"></param>
        /// <param name="assetName"></param>
        /// <param name="progress"></param>
        /// <param name="timing"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public static async UniTask<TObject> LoadSubAssetAsync<TObject>(string location, string assetName, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update) where TObject : Object
        {
            if (AssetCacheDict.TryGetValue(location, out var asset))
            {
                if (asset != null && asset is TObject tAsset)
                {
                    return tAsset;
                }
                Debug.LogError($"资源缓存=NULL location:{location}");
            }

            Debug.Log($"加载资源:{location}");
            SubAssetsHandle handler = YooAssets.LoadSubAssetsAsync<TObject>(location);
            try
            {
                await handler.ToUniTask(progress, timing);
            }
            catch
            {
                ReleaseHandle(handler);
                throw;
            }

            TObject subAsset = handler.GetSubAssetObject<TObject>(assetName);
            if (subAsset == null)
            {
                ReleaseHandle(handler);
                Debug.LogError($"不存在的资源类型location:{location} assetName:{assetName} TObject:{typeof(TObject)}");
                return null;
            }

            AssetCacheDict[location] = subAsset;
            CacheHandle(location, handler);
            return subAsset;
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="location"></param>
        /// <param name="sceneMode"></param>
        /// <param name="localPhysicsMode"></param>
        /// <param name="suspendLoad"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static async UniTask<SceneHandle> LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single,LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.Physics3D , bool suspendLoad = false, uint priority = 100)
        {
            Debug.Log($"YooAssets场景加载---{location}---"); 
            SceneHandle handler = YooAssets.LoadSceneAsync(location, sceneMode,localPhysicsMode,suspendLoad, priority);
            await handler.ToUniTask();

            return handler;
        }
        
        #endregion

        #region 资源处理
             
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
        /// 加载精灵图片
        /// </summary>
        public static async UniTask<Sprite> LoadSprite(string location)
        {
            return await LoadAssetAsync<Sprite>(location);
        }
        /// <summary>
        /// 设置图片精灵
        /// </summary>
        /// <param name="image"></param>
        /// <param name="spriteLocation"></param>
        /// <param name="success"></param>
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
        /// <summary>
        /// 加载声音片断
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static async UniTask<AudioClip> LoadAudioClip(string location)
        {
            return await LoadAssetAsync<AudioClip>(location);
        }
        
        #endregion
    }
}
