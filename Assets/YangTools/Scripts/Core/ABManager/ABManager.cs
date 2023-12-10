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
using TMPro;
using YangTools;
using YangTools.UGUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class BundleManager
{
    private static BundleManager _inst;
    public static BundleManager Instance
    {
        get
        {
            if (BundleManager._inst == null)
            {
                BundleManager._inst = new BundleManager();
            }
            return BundleManager._inst;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitBundleManager()
    {
        BundleManager._inst = new BundleManager();
    }


    private Dictionary<AssetBundle, int> LoadedBundles;

    private BundleManager()
    {
        IEnumerable<AssetBundle> allLoadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles();
        LoadedBundles = new Dictionary<AssetBundle, int>();
        foreach (AssetBundle key in allLoadedAssetBundles)
        {
            LoadedBundles.Add(key, 1);
        }
    }

    private bool isBundleLoaded(string bundle)
    {
        foreach (KeyValuePair<AssetBundle, int> keyValuePair in this.LoadedBundles)
        {
            if (keyValuePair.Key.name == bundle)
            {
                return true;
            }
        }
        return false;
    }

    private void LoadBundles(params string[] bundles)
    {
        for (int i = 0; i < bundles.Length; i++)
        {
            string bundle = bundles[i].ToLower();
            if (!this.isBundleLoaded(bundle))
            {
                this._LoadBundle(bundle);
            }
        }
    }

    public void LoadBundlesAsync(ThreadPriority priority, params string[] bundles)
    {
        Application.backgroundLoadingPriority = priority;
        for (int i = 0; i < bundles.Length; i++)
        {
            string bundle = bundles[i].ToLower();
            if (!this.isBundleLoaded(bundle))
            {
                this._LoadBundleAsync(bundle);
            }
        }
    }

    private void UnloadBundle(string bundle, bool unloadAll = false)
    {
        AssetBundle bundle2 = this.GetBundle(bundle);
        if (bundle2)
        {
            this.LoadedBundles.Remove(bundle2);
            bundle2.Unload(unloadAll);
        }
    }

    private AssetBundle GetBundle(string bundle)
    {
        string b = bundle.ToLower();
        foreach (KeyValuePair<AssetBundle, int> keyValuePair in this.LoadedBundles)
        {
            if (keyValuePair.Key.name == b)
            {
                return keyValuePair.Key;
            }
        }
        return null;
    }

    public AssetBundle GetOrLoadBundle(string bundle)
    {
        AssetBundle bundle2 = this.GetBundle(bundle);
        if (!bundle2)
        {
            return this._LoadBundle(bundle);
        }
        return bundle2;
    }

    private AssetBundle _LoadBundle(string bundle)
    {
        string path = Application.streamingAssetsPath + "/" + bundle;
        if (File.Exists(path))
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
            stopwatch.Stop();
            UnityEngine.Debug.Log(string.Format("Loading bundle {0} took {1} seconds", bundle, stopwatch.ElapsedMilliseconds / 1000L));
            this.LoadedBundles.Add(assetBundle, 1);
            return assetBundle;
        }
        return null;
    }

    private AssetBundleCreateRequest _LoadBundleAsync(string bundle)
    {
        if (File.Exists(Application.streamingAssetsPath + "/" + bundle))
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + bundle);
            ab.completed += delegate (AsyncOperation operation)
            {
                timer.Stop();
                this.LoadedBundles.Add(ab.assetBundle, 1);
                UnityEngine.Debug.Log(string.Format("asyncLoadBundle {0} took {1} seconds", bundle, timer.ElapsedMilliseconds / 1000L));
            };
            return ab;
        }
        return null;
    }

    public BundleManager.VirtualBundle this[string bundlename]
    {
        get
        {
            for (int i = 0; i < this.VirtualBundles.Count; i++)
            {
                if (this.VirtualBundles[i].name == bundlename)
                {
                    return this.VirtualBundles[i];
                }
            }
            BundleManager.VirtualBundle virtualBundle = new BundleManager.VirtualBundle(bundlename, false, ThreadPriority.Normal);
            this.VirtualBundles.Add(virtualBundle);
            return virtualBundle;
        }
    }

    private List<BundleManager.VirtualBundle> VirtualBundles = new List<BundleManager.VirtualBundle>();

    public class VirtualBundle
    {

        private AssetBundle bundle;

        public string name;

        private Dictionary<int, GameObject> loadedAssets;

        private Dictionary<int, Material> loadedMaterias;
        internal VirtualBundle(string bundlename, bool async = false, ThreadPriority priority = ThreadPriority.Normal)
        {
            this.loadedAssets = new Dictionary<int, GameObject>();
            this.loadedMaterias = new Dictionary<int, Material>();
            this.name = bundlename;
            if (!async)
            {
                this.bundle = BundleManager.Instance.GetOrLoadBundle(bundlename);
                return;
            }
            if (BundleManager.Instance.isBundleLoaded(bundlename))
            {
                this.bundle = BundleManager.Instance.GetBundle(bundlename);
                return;
            }
            Application.backgroundLoadingPriority = priority;
            AssetBundleCreateRequest cr = BundleManager.Instance._LoadBundleAsync(bundlename);
            if (cr != null)
            {
                cr.completed += delegate (AsyncOperation operation)
                {
                    this.bundle = cr.assetBundle;
                };
            }
        }

        public void Unload(bool unloadAll)
        {
            if (this.bundle)
            {
                this.bundle.Unload(unloadAll);
                BundleManager.Instance.VirtualBundles.Remove(this);
                BundleManager.Instance.LoadedBundles.Remove(this.bundle);
                this.UnloadLoadedAssets(true);
            }
        }

        public void UnloadLoadedAssets(bool lowAsync = true)
        {
            this.loadedAssets = new Dictionary<int, GameObject>();
            this.loadedMaterias = new Dictionary<int, Material>();
            if (lowAsync)
            {
                Application.backgroundLoadingPriority = ThreadPriority.Low;
            }
            Resources.UnloadUnusedAssets();
        }

        public GameObject LoadFromBundle(string path)
        {
            int hashCode = path.GetHashCode();
            GameObject gameObject;
            if (!this.loadedAssets.TryGetValue(hashCode, out gameObject) && this.bundle)
            {
                gameObject = this.bundle.LoadAsset<GameObject>(path);
                if (gameObject)
                {
                    this.loadedAssets.Add(hashCode, gameObject);
                    return gameObject;
                }
            }
            return gameObject;
        }

        public Material LoadMaterialFromBundle(string path)
        {
            int hashCode = path.GetHashCode();
            Material material;
            if (!this.loadedMaterias.TryGetValue(hashCode, out material) && this.bundle)
            {
                material = this.bundle.LoadAsset<Material>(path);
                if (material)
                {
                    this.loadedMaterias.Add(hashCode, material);
                    return material;
                }
            }
            return material;
        }

        public GameObject Instantiate(string AssetPath, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            int hashCode = AssetPath.GetHashCode();
            GameObject gameObject;
            if (!this.loadedAssets.TryGetValue(hashCode, out gameObject) && this.bundle)
            {
                gameObject = this.bundle.LoadAsset<GameObject>(AssetPath);
                if (gameObject)
                {
                    this.loadedAssets.Add(hashCode, gameObject);
                    return UnityEngine.Object.Instantiate<GameObject>(gameObject, position, rotation, parent);
                }
            }
            if (!gameObject)
            {
                return null;
            }
            return UnityEngine.Object.Instantiate<GameObject>(gameObject, position, rotation, parent);
        }
    }
}

