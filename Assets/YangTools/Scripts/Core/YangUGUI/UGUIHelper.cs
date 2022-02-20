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
using UnityEngine.UI;

namespace YangTools.UI
{
    /// <summary>
    /// UGUI界面辅助器
    /// </summary>
    public class UGUIPanelHelper : MonoBehaviour, IUIPanelHelper
    {
        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAsset">要实例化的界面资源。</param>
        /// <returns>实例化后的界面。</returns>
        public object InstantiateUIForm(object uiFormAsset)
        {
            return Instantiate((Object)uiFormAsset);
        }
        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面</returns>
        public IUIPanel CreateUIPanel(object uiFormInstance, IUIGroup uiGroup, object userData)
        {
            GameObject gameObject = uiFormInstance as GameObject;
            if (gameObject == null)
            {
                Debug.LogError("UI form instance is invalid.");
                return null;
            }
            Transform transform = gameObject.transform;
            transform.SetParent(((MonoBehaviour)uiGroup.Helper).transform);
            transform.localScale = Vector3.one;
            return gameObject.GetOrAddComponent<UIPanelBase>();
        }
        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormAsset">要释放的界面资源。</param>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        public void ReleaseUIPanel(object uiFormAsset, object uiFormInstance)
        {
            Destroy((Object)uiFormInstance);
        }
    }
    /// <summary>
    /// UGUI界面组辅助器
    /// </summary>
    public class UGUIGroupHelper : MonoBehaviour, IUIGroupHelper
    {
        public const int DepthFactor = 1000;
        private int m_Depth = 0;
        private Canvas m_CachedCanvas = null;
        private void Awake()
        {
            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }
        private void Start()
        {
            m_CachedCanvas.overrideSorting = true;
            m_CachedCanvas.sortingOrder = DepthFactor * m_Depth;

            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
        }
        /// <summary>
        /// 设置界面组深度
        /// </summary>
        /// <param name="depth">界面组深度</param>
        public void SetDepth(int depth)
        {
            m_Depth = depth;
            m_CachedCanvas.overrideSorting = true;
            m_CachedCanvas.sortingOrder = DepthFactor * depth;
        }
    }
}