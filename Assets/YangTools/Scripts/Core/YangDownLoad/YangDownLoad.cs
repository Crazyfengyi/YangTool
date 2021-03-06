using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using YangTools;

namespace YangTools
{
    /// <summary>
    /// UnityWebRequest下载器
    /// </summary>
    public class YangDownLoad : MonoBehaviour
    {
        /// <summary>
        /// 单例
        /// </summary>
        private static YangDownLoad instance;
        /// <summary>
        /// 携程物体
        /// </summary>
        private static GameObject IEnumeratorObject;
        /// <summary>
        /// 设备线程锁
        /// </summary>
        private static readonly object padlock = new object();
        public static YangDownLoad Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        IEnumeratorObject = new GameObject("IEnumeratorObject");
                        instance = IEnumeratorObject.AddComponent<YangDownLoad>();
                        IEnumeratorObject.transform.SetParent(YangToolsManager.DontDestoryObject.transform);
                    }
                    return instance;
                }
            }
        }
        /// <summary>
        /// 标准下载
        /// </summary>
        /// <returns></returns>
        public static IEnumerator DownLoad(string url, Action<byte[]> succeed, Action<string> failed)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get($"{url}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError ||
                webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                failed?.Invoke(webRequest.error);
            }
            else
            {
                succeed?.Invoke(webRequest.downloadHandler.data);
            }
        }
    }

}