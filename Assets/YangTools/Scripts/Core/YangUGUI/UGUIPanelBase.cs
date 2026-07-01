/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-20
*/

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YangTools.Scripts.Core.YangExtend;
using Debug = UnityEngine.Debug;

namespace YangTools.Scripts.Core.YangUGUI
{
    /// <summary>
    /// UGUI Panel逻辑基类
    /// </summary>
    public abstract class UGUIPanelBase<T> : MonoBehaviour, IUGUIPanel where T : UGUIDataBase
    {
        private const int DepthFactor = 10;//UI界面深度系数

        private Dictionary<Canvas,int> cachedCanvasDic = new();//缓存的Canvas列表
        private CanvasGroup canvasGroup;//缓存的CanvasGroup
        private Canvas cachedCanvas;//缓存的Canvas
        private RectTransform bgMask;
        private CanvasGroup bgMaskCanvasGroup;
        
        private bool visible;//是否显示

        private Transform node;//页面表现节点(动画节点)
        private int originalLayer;//原始层级

        [Sirenix.OdinInspector.FoldoutGroup("基础设置")]
        [LabelText("打开动画")]
        public bool needOpenAni;
        [Sirenix.OdinInspector.FoldoutGroup("基础设置")]
        [LabelText("打开动画时间")]
        public float aniTime = 0.42f;
        [Sirenix.OdinInspector.FoldoutGroup("基础设置")]
        [LabelText("关闭动画")]
        public bool needCloseAni;
        [Sirenix.OdinInspector.FoldoutGroup("基础设置")]
        [LabelText("关闭动画时间")]
        public float aniTime2 = 0.42f;

        public T windowData;
        #region 对外属性

        /// <summary>
        /// 获取界面
        /// </summary>
        private UIPanel UIPanel { get; set; }

        /// <summary>
        /// 获取或设置界面名称
        /// </summary>
        public string Name
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }

        /// <summary>
        /// 获取界面是否可用
        /// </summary>
        public bool Available { get; private set; } = false;

        /// <summary>
        /// 获取或设置界面是否可见
        /// </summary>
        public bool Visible
        {
            get => Available && visible;
            set
            {
                if (!Available)
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
        public Transform CachedTransform { get; private set; }

        /// <summary>
        /// 原始深度
        /// </summary>
        public int OriginalDepth { get; private set; }

        /// <summary>
        /// 深度
        /// </summary>
        public int Depth => cachedCanvas.sortingOrder;

        #endregion 对外属性

        #region 功能方法

        /// <summary>
        /// 关闭自身
        /// </summary>
        public void CloseSelfPanel()
        {
            StopAllCoroutines();
            if (needCloseAni)
            {
                DOTween.Kill(this,true);
                DOTween.Sequence()
                    .AppendCallback(() =>
                    {
                        bgMaskCanvasGroup.alpha = 1f;
                    })
                    .Append(node.DOLocalMove(startLocalPos - new Vector3(0, Screen.height, 0), aniTime2).SetEase(Ease.Linear))
                    .Join(bgMaskCanvasGroup.DOFade(0f, aniTime2))
                    .OnComplete(() =>
                    {
                        bgMaskCanvasGroup.alpha = 0f;
                        node.transform.localPosition = startLocalPos - new Vector3(0, Screen.height, 0);
                        UIMonoInstance.Instance.ClosePanel(this.UIPanel);
                    })
                    .SetTarget(this);
            }
            else
            {
                UIMonoInstance.Instance.ClosePanel(this.UIPanel);
            }
        }

        /// <summary>
        /// 设置字体
        /// </summary>
        public static void SetMainFont(TMP_FontAsset mainFont)
        {
            if (mainFont == null)
            {
                Debug.LogError("Main font is invalid.");
                return;
            }

            YangUIManager.MainFont = mainFont;
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
        /// 设置界面的可见性
        /// </summary>
        public virtual void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        #endregion

        #region 生命周期

        public virtual void OnInit(object userData)
        {
            OnInit(userData as T);
        }

        private Vector3 startLocalPos;
        /// <summary>
        /// 界面初始化
        /// </summary>
        /// <param name="pConfirmData">用户自定义数据</param>
        public virtual void OnInit(T pConfirmData)
        {
            if (CachedTransform == null) CachedTransform = transform;

            UIPanel = GetComponent<UIPanel>();
            originalLayer = gameObject.layer;

            cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            cachedCanvas.overrideSorting = true;
            OriginalDepth = cachedCanvas.sortingOrder;

            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            node = transform.Find("WindowRoot");
            startLocalPos = node ? node.transform.localPosition: Vector3.zero;

            bgMask = transform.Find("bgMask")?.GetComponent<RectTransform>();
            bgMaskCanvasGroup = bgMask ? bgMask.gameObject.GetOrAddComponent<CanvasGroup>() : null;
            
            RectTransform trans = GetComponent<RectTransform>();
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.anchoredPosition = Vector2.zero;
            trans.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 界面打开
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public virtual void OnOpen(object userData)
        {
            Available = true;
            Visible = true;
            windowData = userData as T;
            
            //TODO:多语言处理
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (YangUIManager.MainFont) texts[i].font = YangUIManager.MainFont;
                if (!string.IsNullOrEmpty(texts[i].text))
                {
                    texts[i].text = texts[i].text.ToString(); //多语言
                }
            }
            
            DOTween.Kill(this,true);
            //默认动画
            if (node && needOpenAni)
            {
                // DOTween.Sequence()
                //     .AppendCallback(() =>
                //     {
                //         canvasGroup.alpha = 0.8f;
                //         node.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                //     })
                //     .Append(node.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
                //     .Join(canvasGroup.DOFade(1f, 0.3f))
                //     .OnComplete(() =>
                //     {
                //         canvasGroup.alpha = 1f;
                //         node.transform.localScale = Vector3.one;
                //     })
                //     .SetTarget(this);
                
                DOTween.Sequence()
                    .AppendCallback(() =>
                    {
                        bgMaskCanvasGroup.alpha = 0.8f;
                        node.transform.localPosition = startLocalPos - new Vector3(0, Screen.height, 0);
                    })
                    .Append(node.DOLocalMove(startLocalPos, aniTime).SetEase(Ease.Linear))
                    .Join(bgMaskCanvasGroup.DOFade(1f, aniTime))
                    .OnComplete(() =>
                    {
                        bgMaskCanvasGroup.alpha = 1f;
                        node.transform.localPosition = startLocalPos;
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
            Available = false;
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
        public virtual void OnLostFocus()
        {
        }

        /// <summary>
        /// 界面遮挡恢复
        /// </summary>
        public virtual void OnReFocus()
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
        /// <param name="groupDepth">界面组深度</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度</param>
        public virtual void OnDepthChanged(int groupDepth, int depthInUIGroup)
        {
            int deltaDepth = UGUIGroupHelper.DepthFactor * groupDepth + DepthFactor * depthInUIGroup  + OriginalDepth;

            //清除已销毁的
            cachedCanvasDic = cachedCanvasDic
                .Where(kvp => kvp.Key != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
            Canvas[] tempList = GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < tempList.Length; i++)
            {
                Canvas tempItem = tempList[i];
                if (!cachedCanvasDic.ContainsKey(tempItem))
                {
                    cachedCanvasDic.Add(tempItem,tempItem.sortingOrder);
                }
            }
            
            foreach (var item in cachedCanvasDic)
            {
                item.Key.sortingOrder = item.Value + deltaDepth;
            }
        }

        /// <summary>
        /// 界面回收
        /// </summary>
        public virtual void OnRecycle()
        {
        }

        #endregion 生命周期
    }
}