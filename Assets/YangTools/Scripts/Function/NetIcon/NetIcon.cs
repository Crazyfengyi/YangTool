/* 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-09-23 
*/  
using System.Collections;
using UnityEngine;  
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// 网络icon
/// </summary>
[RequireComponent(typeof(Image))]
public class NetIcon : MonoBehaviour
{
    private Image mIcon; //图标组件
    private Coroutine mLoadCoroutine; //图片加载协程
    private Sprite mRuntimeSprite; //运行时创建的精灵
    private Texture2D mRuntimeTexture; //运行时下载的纹理
    private int mLoadVersion; //当前加载版本

    /// <summary>
    /// 初始化图标组件
    /// </summary>
    private void Awake()
    {
        mIcon = GetComponent<Image>();
    }

    /// <summary>
    /// 释放运行时创建的图片资源
    /// </summary>
    private void OnDestroy()
    {
        mLoadVersion++;
        ReleaseRuntimeImage();
    }

    /// <summary>
    /// 从网络地址加载icon
    /// </summary>
    public void LoadImageByWeb(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning($"{nameof(NetIcon)} 图片地址为空", this);
            return;
        }

        mLoadVersion++;
        if (mLoadCoroutine != null)
        {
            StopCoroutine(mLoadCoroutine);
        }

        mLoadCoroutine = StartCoroutine(StartLoadImage(url, mLoadVersion));
    }

    /// <summary>
    /// 下载图片
    /// </summary>
    private IEnumerator StartLoadImage(string url, int loadVersion)
    {
        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
        yield return webRequest.SendWebRequest();

        if (loadVersion != mLoadVersion)
        {
            yield break;
        }

        mLoadCoroutine = null;
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"{nameof(NetIcon)} 下载图片失败 {webRequest.error} {url}", this);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
        if (texture == null)
        {
            Debug.LogWarning($"{nameof(NetIcon)} 未能解析图片 {url}", this);
            yield break;
        }

        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        ReleaseRuntimeImage();
        mRuntimeTexture = texture;
        mRuntimeSprite = Sprite.Create(mRuntimeTexture, rect, pivot);
        mIcon.sprite = mRuntimeSprite;
    }

    /// <summary>
    /// 释放本组件下载并创建的图片资源
    /// </summary>
    private void ReleaseRuntimeImage()
    {
        if (mIcon != null && mIcon.sprite == mRuntimeSprite)
        {
            mIcon.sprite = null;
        }

        if (mRuntimeSprite != null)
        {
            Destroy(mRuntimeSprite);
            mRuntimeSprite = null;
        }

        if (mRuntimeTexture != null)
        {
            Destroy(mRuntimeTexture);
            mRuntimeTexture = null;
        }
    }
}
