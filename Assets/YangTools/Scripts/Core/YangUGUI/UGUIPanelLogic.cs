/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-20 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using YangTools.Extend;
using UnityEngine.UI;

namespace YangTools.UI
{
    /// <summary>
    /// UGUI界面逻辑基类。
    /// </summary>
    public abstract class UGUIPanelLogic : MonoBehaviour
    {
        private bool m_Available = false;
        private bool m_Visible = false;
        private UIPanelBase m_UIForm = null;
        private Transform m_CachedTransform = null;
        private int m_OriginalLayer = 0;
        /// <summary>
        /// 获取界面。
        /// </summary>
        public UIPanelBase UIForm
        {
            get
            {
                return m_UIForm;
            }
        }
        /// <summary>
        /// 获取或设置界面名称。
        /// </summary>
        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
            }
        }
        /// <summary>
        /// 获取界面是否可用。
        /// </summary>
        public bool Available
        {
            get
            {
                return m_Available;
            }
        }
        /// <summary>
        /// 获取或设置界面是否可见。
        /// </summary>
        public bool Visible
        {
            get
            {
                return m_Available && m_Visible;
            }
            set
            {
                if (!m_Available)
                {
                    Debug.LogWarning(string.Format("UI form '{0}' is not available.", Name));
                    return;
                }
                if (m_Visible == value)
                {
                    return;
                }
                m_Visible = value;
                SetVisible(value);
            }
        }
        /// <summary>
        /// 获取已缓存的 Transform。
        /// </summary>
        public Transform CachedTransform
        {
            get
            {
                return m_CachedTransform;
            }
        }

        //======================================
        public const int DepthFactor = 100;
        private const float FadeTime = 0.3f;
        private static Font s_MainFont = null;
        private Canvas m_CachedCanvas = null;
        private CanvasGroup m_CanvasGroup = null;
        private List<Canvas> m_CachedCanvasContainer = new List<Canvas>();
        public int OriginalDepth
        {
            get;
            private set;
        }
        public int Depth
        {
            get
            {
                return m_CachedCanvas.sortingOrder;
            }
        }

        #region 父类设置
        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="uiSoundId"></param>
        public void PlayUISound(int uiSoundId)
        {
        }
        /// <summary>
        /// 设置字体
        /// </summary>
        /// <param name="mainFont"></param>
        public static void SetMainFont(Font mainFont)
        {
            if (mainFont == null)
            {
                Debug.LogError("Main font is invalid.");
                return;
            }
            s_MainFont = mainFont;
        }
        /// <summary>
        /// 关闭页面
        /// </summary>
        /// <param name="ignoreFade"></param>
        public void ClosePanel(bool ignoreFade = false)
        {
            StopAllCoroutines();

            if (ignoreFade)
            {
                UIMonoInstance.Instance.CloseUIPanel(this.UIForm);
            }
            else
            {
                //StartCoroutine(CloseCo(FadeTime));//动画
            }
        }
        /// <summary>
        /// 设置界面的可见性
        /// </summary>
        protected virtual void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        #endregion

        #region 生命周期
        /// <summary>
        /// 界面初始化。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnInit(object userData)
        {
            if (m_CachedTransform == null)
            {
                m_CachedTransform = transform;
            }

            m_UIForm = GetComponent<UIPanelBase>();
            m_OriginalLayer = gameObject.layer;

            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            m_CachedCanvas.overrideSorting = true;
            OriginalDepth = m_CachedCanvas.sortingOrder;

            m_CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            RectTransform trans = GetComponent<RectTransform>();
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.anchoredPosition = Vector2.zero;
            trans.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].font = s_MainFont;
                if (!string.IsNullOrEmpty(texts[i].text))
                {
                    texts[i].text = texts[i].text.ToString();//多语言
                }
            }
        }
        /// <summary>
        /// 界面回收。
        /// </summary>
        protected internal virtual void OnRecycle()
        {
        }
        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnOpen(object userData)
        {
            m_Available = true;
            Visible = true;

            m_CanvasGroup.alpha = 0f;
            StopAllCoroutines();
            //StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));//动画
        }
        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnClose(bool isShutdown, object userData)
        {
            //gameObject.SetLayerRecursively(m_OriginalLayer);
            Visible = false;
            m_Available = false;
        }
        /// <summary>
        /// 界面暂停。
        /// </summary>
        protected internal virtual void OnPause()
        {
            Visible = false;
        }
        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        protected internal virtual void OnResume()
        {
            Visible = true;
            m_CanvasGroup.alpha = 0f;
            StopAllCoroutines();
            //StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));
        }
        /// <summary>
        /// 界面遮挡。
        /// </summary>
        protected internal virtual void OnCover()
        {
        }
        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        protected internal virtual void OnReveal()
        {
        }
        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnRefocus(object userData)
        {
        }
        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }
        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        protected internal virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            int oldDepth = Depth;
            int deltaDepth = UGUIGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth + OriginalDepth;
            GetComponentsInChildren(true, m_CachedCanvasContainer);
            for (int i = 0; i < m_CachedCanvasContainer.Count; i++)
            {
                m_CachedCanvasContainer[i].sortingOrder += deltaDepth;
            }

            m_CachedCanvasContainer.Clear();
        }
        #endregion
    }
}