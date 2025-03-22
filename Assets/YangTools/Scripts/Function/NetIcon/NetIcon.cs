/* 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-09-23 
*/  
using System;
using System.Collections;  
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using YangTools;
using UnityEngine.Networking;

/// <summary>
/// 网络icon
/// </summary>
[RequireComponent(typeof(Image))]
public class NetIcon : MonoBehaviour
{
    private Image mIcon;
    private void Awake()
    {
        mIcon = transform.GetComponent<Image>();
    }
    /// <summary>
    /// 从网络地址加载icon
    /// </summary>
    public void LoadImageByWeb(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        StartCoroutine(StartLoadImage(url));
    }
    /// <summary>
    /// 下载图片
    /// </summary>
    private IEnumerator StartLoadImage(string url)
    {
        UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture(url);
        DownloadHandlerTexture downloadHandlerTexture = new DownloadHandlerTexture(true);
        unityWebRequest.downloadHandler = downloadHandlerTexture;
        yield return unityWebRequest.SendWebRequest();
        Texture2D texture = downloadHandlerTexture.texture;
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite sprite = Sprite.Create(texture, rect, pivot);
        mIcon.sprite = sprite;
    }
}
