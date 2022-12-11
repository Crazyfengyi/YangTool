/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-20 
*/
using UnityEngine;
using System.Collections;
using YangTools.Extend;
using YangTools.Log;
using UnityEngine.UI;

namespace YangTools.UGUI
{
    /// <summary>
    /// UGUI组辅助器
    /// </summary>
    public class UGUIGroupHelper : MonoBehaviour, IUIGroupHelper
    {
        public const int DepthFactor = 200;
        private int depth = 0;//深度
        private Canvas cachedCanvas = null;//缓存的canvas
        private void Awake()
        {
            cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }
        private void Start()
        {
            cachedCanvas.overrideSorting = true;
            cachedCanvas.sortingOrder = DepthFactor * depth;

            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
        }
        /// <summary>
        /// 设置UI界面组深度
        /// </summary>
        /// <param name="depth">UI界面组深度</param>
        public void SetDepth(int depth)
        {
            this.depth = depth;
            cachedCanvas.overrideSorting = true;
            cachedCanvas.sortingOrder = DepthFactor * depth;
        }
    }
    /// <summary>
    /// UGUI界面辅助器
    /// </summary>
    public class UGUIPanelHelper : MonoBehaviour, IUIPanelHelper
    {
        /// <summary>
        /// 实例化UI界面
        /// </summary>
        /// <param name="uiPanelAsset">要实例化的界面资源</param>
        /// <returns>实例化后的界面</returns>
        public object InstantiateUIPanel(object uiPanelAsset)
        {
            return Instantiate((Object)uiPanelAsset);
        }
        /// <summary>
        /// 创建UI界面
        /// </summary>
        /// <param name="uiPanelInstance">界面实例</param>
        /// <param name="uiGroup">界面所属的界面组</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面</returns>
        public IUIPanel CreateUIPanel(object uiPanelInstance, IUIGroup uiGroup, object userData)
        {
            GameObject gameObject = uiPanelInstance as GameObject;
            if (gameObject == null)
            {
                Debug.LogError("UI form instance is invalid.");
                return null;
            }
            Transform transform = gameObject.transform;
            transform.SetParent(((MonoBehaviour)uiGroup.Helper).transform);
            transform.localScale = Vector3.one;
            return gameObject.GetOrAddComponent<UIPanel>();
        }
        /// <summary>
        /// 释放UI界面
        /// </summary>
        /// <param name="uiPanelAsset">要释放的界面资源</param>
        /// <param name="uiPanelInstance">要释放的界面实例</param>
        public void ReleaseUIPanel(object uiPanelAsset, object uiPanelInstance)
        {
            Destroy((Object)uiPanelInstance);
        }
    }
}