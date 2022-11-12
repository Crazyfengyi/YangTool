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

namespace YangTools.UGUI
{
    /// <summary>
    /// UGUI界面逻辑基类
    /// </summary>
    public abstract class UGUIPanelBase : MonoBehaviour
    {
        public const int DepthFactor = 10;//UI界面深度系数
        private static Font mainFont = null;//主字体

        private bool available = false;//是否可用的
        private bool visible = false;//是否显示
        private UIPanel uiPanel = null;//UI界面
        private Transform cachedTransform = null;//缓存的Transform
        private CanvasGroup canvasGroup = null;//缓存的CanvasGroup
        private Canvas cachedCanvas = null;//缓存的Canvas
        private List<Canvas> cachedCanvasList = new List<Canvas>();//缓存的Canvas列表

        private int originalLayer = 0;//原始层级

        #region 对外属性
        /// <summary>
        /// 获取界面
        /// </summary>
        public UIPanel UIPanel
        {
            get
            {
                return uiPanel;
            }
        }
        /// <summary>
        /// 获取或设置界面名称
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
        /// 获取界面是否可用
        /// </summary>
        public bool Available
        {
            get
            {
                return available;
            }
        }
        /// <summary>
        /// 获取或设置界面是否可见
        /// </summary>
        public bool Visible
        {
            get
            {
                return available && visible;
            }
            set
            {
                if (!available)
                {
                    Debug.LogWarning(string.Format("UI form '{0}' is not available.", Name));
                    return;
                }
                if (visible == value)
                {
                    return;
                }
                visible = value;
                SetVisible(value);
            }
        }
        /// <summary>
        /// 获取已缓存的Transform
        /// </summary>
        public Transform CachedTransform
        {
            get
            {
                return cachedTransform;
            }
        }
        /// <summary>
        /// 原始深度
        /// </summary>
        public int OriginalDepth
        {
            get;
            private set;
        }
        /// <summary>
        /// 深度
        /// </summary>
        public int Depth
        {
            get
            {
                return cachedCanvas.sortingOrder;
            }
        }
        #endregion

        #region 父类设置
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
            UGUIPanelBase.mainFont = mainFont;
        }
        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="uiSoundId"></param>
        public void PlayUISound(int uiSoundId)
        {
            //TODO 差实现
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
                UIMonoInstance.Instance.CloseUIPanel(this.UIPanel);
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
        /// 界面初始化
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        protected internal virtual void OnInit(object userData)
        {
            if (cachedTransform == null)
            {
                cachedTransform = transform;
            }

            uiPanel = GetComponent<UIPanel>();
            originalLayer = gameObject.layer;

            cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            cachedCanvas.overrideSorting = true;
            OriginalDepth = cachedCanvas.sortingOrder;

            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            RectTransform trans = GetComponent<RectTransform>();
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.anchoredPosition = Vector2.zero;
            trans.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].font = mainFont;
                if (!string.IsNullOrEmpty(texts[i].text))
                {
                    texts[i].text = texts[i].text.ToString();//多语言
                }
            }
        }
        /// <summary>
        /// 界面回收
        /// </summary>
        protected internal virtual void OnRecycle()
        {
        }
        /// <summary>
        /// 界面打开
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        protected internal virtual void OnOpen(object userData)
        {
            available = true;
            Visible = true;

            canvasGroup.alpha = 1f;
            StopAllCoroutines();
            //StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));//动画
        }
        /// <summary>
        /// 界面关闭
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发</param>
        /// <param name="userData">用户自定义数据</param>
        protected internal virtual void OnClose(bool isShutdown, object userData)
        {
            gameObject.SetLayerRecursively(originalLayer);
            Visible = false;
            available = false;
        }
        /// <summary>
        /// 界面暂停
        /// </summary>
        protected internal virtual void OnPause()
        {
            Visible = false;
        }
        /// <summary>
        /// 界面暂停恢复
        /// </summary>
        protected internal virtual void OnResume()
        {
            Visible = true;
            canvasGroup.alpha = 0f;
            StopAllCoroutines();
            //StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));
        }
        /// <summary>
        /// 界面遮挡
        /// </summary>
        protected internal virtual void OnCover()
        {
        }
        /// <summary>
        /// 界面遮挡恢复
        /// </summary>
        protected internal virtual void OnReveal()
        {
        }
        /// <summary>
        /// 界面激活
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        protected internal virtual void OnRefocus(object userData)
        {
        }
        /// <summary>
        /// 界面轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        protected internal virtual void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
        }
        /// <summary>
        /// 界面深度改变
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度</param>
        protected internal virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            int oldDepth = Depth;
            int deltaDepth = UGUIGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth + OriginalDepth;
            GetComponentsInChildren(true, cachedCanvasList);
            for (int i = 0; i < cachedCanvasList.Count; i++)
            {
                cachedCanvasList[i].sortingOrder += deltaDepth;
            }

            cachedCanvasList.Clear();
        }
        #endregion
    }
}