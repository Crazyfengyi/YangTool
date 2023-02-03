/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-20
*/

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YangTools.Extend;
using Debug = UnityEngine.Debug;

namespace YangTools.UGUI
{
    /// <summary>
    /// UGUI界面逻辑基类
    /// </summary>
    public abstract class UGUIPanelBase<T> : MonoBehaviour, IUGUIPanel where T : UGUIDataBase
    {
        public const int DepthFactor = 10;//UI界面深度系数

        private bool available = false;//是否可用的
        private bool visible = false;//是否显示
        private UIPanel uiPanel;//UI界面
        private Transform cachedTransform;//缓存的Transform
        private CanvasGroup canvasGroup;//缓存的CanvasGroup
        private Canvas cachedCanvas;//缓存的Canvas
        private List<Canvas> cachedCanvasList = new List<Canvas>();//缓存的Canvas列表

        private Transform Node;//页面表现节点(动画节点)

        private int originalLayer = 0;//原始层级

        #region 对外属性

        /// <summary>
        /// 获取界面
        /// </summary>
        public UIPanel UIPanel => uiPanel;

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
        public bool Available => available;

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
        public Transform CachedTransform => cachedTransform;

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
        public int Depth => cachedCanvas.sortingOrder;

        public int SerialId => throw new System.NotImplementedException();

        public string UIPanelAssetName => throw new System.NotImplementedException();

        public object Handle { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public IUIGroup UIGroup => throw new System.NotImplementedException();

        public int DepthInUIGroup => throw new System.NotImplementedException();

        public bool IsOpening => throw new System.NotImplementedException();

        public bool PauseCoveredUIPanel => throw new System.NotImplementedException();

        #endregion 对外属性

        #region 静态方法
        public static void OpenPanel(T userData, string assetName, string groupName)
        {
            UIMonoInstance.Instance.OpenUIPanel(assetName, groupName, (object)userData);
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
            YangUIManager.mainFont = mainFont;
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
        public void ClosePanel(bool ignoreFade = true)
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

        #endregion 父类设置

        #region 生命周期

        /// <summary>
        /// 界面初始化
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public virtual void OnInit(object userData)
        {
            if (cachedTransform == null) cachedTransform = transform;

            uiPanel = GetComponent<UIPanel>();
            originalLayer = gameObject.layer;

            cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            cachedCanvas.overrideSorting = true;
            OriginalDepth = cachedCanvas.sortingOrder;

            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            Node = transform.Find("Node");

            RectTransform trans = GetComponent<RectTransform>();
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.anchoredPosition = Vector2.zero;
            trans.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            //TODO:多语言处理
            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].font = YangUIManager.mainFont;
                if (!string.IsNullOrEmpty(texts[i].text))
                {
                    texts[i].text = texts[i].text.ToString();//多语言
                }
            }
        }

        /// <summary>
        /// 界面回收
        /// </summary>
        public virtual void OnRecycle()
        {
        }

        /// <summary>
        /// 界面打开
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public virtual void OnOpen(object userData)
        {
            available = true;
            Visible = true;

            DOTween.Kill(this);
            //默认动画
            if (Node)
            {
                DOTween.Sequence()
                    .AppendCallback(() =>
                    {
                        canvasGroup.alpha = 0.6f;
                        Node.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                    })
                    .Append(Node.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutElastic))
                    .Join(canvasGroup.DOFade(1f, 0.2f))
                    .OnComplete(() =>
                    {
                        canvasGroup.alpha = 1f;
                        Node.transform.localScale = Vector3.one;
                    })
                    .SetTarget(this);
            }
        }

        /// <summary>
        /// 界面关闭
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发</param>
        /// <param name="userData">用户自定义数据</param>
        public virtual void OnClose(bool isShutdown, object userData)
        {
            gameObject.SetLayerRecursively(originalLayer);
            Visible = false;
            available = false;
        }

        /// <summary>
        /// 界面暂停
        /// </summary>
        public virtual void OnPause()
        {
            Visible = false;
        }

        /// <summary>
        /// 界面暂停恢复
        /// </summary>
        public virtual void OnResume()
        {
            Visible = true;
        }

        /// <summary>
        /// 界面遮挡
        /// </summary>
        public virtual void OnCover()
        {
        }

        /// <summary>
        /// 界面遮挡恢复
        /// </summary>
        public virtual void OnReveal()
        {
        }

        /// <summary>
        /// 界面激活
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public virtual void OnRefocus(object userData)
        {
        }

        /// <summary>
        /// 界面轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        public virtual void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
        }

        /// <summary>
        /// 界面深度改变
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度</param>
        public virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
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

        public void OnInit(int serialId, string uiPanelAssetName, IUIGroup uiGroup, bool pauseCovereduiPanel, bool isNewInstance, object userData)
        {
            throw new System.NotImplementedException();
        }

        #endregion 生命周期
    }
}