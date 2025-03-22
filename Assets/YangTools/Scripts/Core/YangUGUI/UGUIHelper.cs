/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:2022-02-20
*/

using UnityEngine;
using UnityEngine.UI;
using YangTools.Scripts.Core.YangExtend;

namespace YangTools.Scripts.Core.YangUGUI
{
    /// <summary>
    /// UGUI组辅助器
    /// </summary>
    public class UGUIGroupHelper : MonoBehaviour, IUIGroupHelper
    {
        public const int DepthFactor = 200;
        private int depth;//深度
        private Canvas cachedCanvas;//缓存的canvas

        private void Awake()
        {
            cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        private void Start()
        {
            cachedCanvas.overrideSorting = true;
            cachedCanvas.sortingOrder = DepthFactor * depth;

            RectTransform tempTransform = GetComponent<RectTransform>();
            tempTransform.anchorMin = Vector2.zero;
            tempTransform.anchorMax = Vector2.one;
            tempTransform.anchoredPosition = Vector2.zero;
            tempTransform.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// 设置UI界面组深度
        /// </summary>
        /// <param name="depth">UI界面组深度</param>
        public void SetDepth(int depth)
        {
            this.depth = depth;
            cachedCanvas.overrideSorting = true;
            cachedCanvas.sortingOrder = DepthFactor * this.depth;
        }
    }

    /// <summary>
    /// UGUI界面辅助器
    /// </summary>
    public class UGUIPanelCreateHelper : MonoBehaviour, IUICreateHelper 
    {
        /// <summary>
        /// 实例化UI界面
        /// </summary>
        /// <param name="panelAsset">要实例化的界面资源</param>
        /// <returns>实例化后的界面</returns>
        public object InstantiatePanel(object panelAsset)
        {
            return Instantiate((Object)panelAsset,Vector3.zero,Quaternion.identity);
        }

        /// <summary>
        /// 创建UI界面
        /// </summary>
        /// <param name="panelInstance">界面实例</param>
        /// <param name="group">界面所属的界面组</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面</returns>
        public IUIPanel CreatePanel(object panelInstance, IUIGroup group, object userData)
        {
            GameObject gameObject = panelInstance as GameObject;
            if (gameObject == null)
            {
                Debug.LogError("UI form instance is invalid.");
                return null;
            }
            Transform transform = gameObject.transform;
            transform.SetParent(((MonoBehaviour)group.Helper).transform);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            return gameObject.GetOrAddComponent<UIPanel>();
        }

        /// <summary>
        /// 释放UI界面
        /// </summary>
        /// <param name="panelAsset">要释放的界面资源</param>
        /// <param name="panelInstance">要释放的界面实例</param>
        public void ReleasePanel(object panelAsset, object panelInstance)
        {
            Destroy((Object)panelInstance);
        }
    }
}