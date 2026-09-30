/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-20
*/

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
        private CanvasGroup canvasGroup;//缓存的CanvasGroup
        private Canvas cachedCanvas;//缓存的Canvas
        private RectTransform bgMask;
        private CanvasGroup bgMaskCanvasGroup;
        
        private bool visible;//是否显示

        private Transform node;//页面表现节点(动画节点)
        private int originalLayer;//原始层级
        private Sequence transition; //当前页面动画
        private bool isClosing; //是否正在关闭

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
        public int Depth => cachedCanvas ? cachedCanvas.sortingOrder : 0;

        #endregion 对外属性

        #region 功能方法

        /// <summary>
        /// 关闭自身
        /// </summary>
        public void CloseSelfPanel()
        {
            if (isClosing || !Available || !UIPanel || !UIPanel.IsOpening) return;
            isClosing = true;
            StopAllCoroutines();
            StopTransition();
            if (needCloseAni && CanAnimate())
            {
                int serialId = UIPanel.SerialId; //本次关闭的页面版本
                bgMaskCanvasGroup.alpha = 1f;
                transition = DOTween.Sequence()
                    .Append(node.DOLocalMove(startLocalPos - new Vector3(0, Screen.height, 0), aniTime2).SetEase(Ease.Linear))
                    .Join(bgMaskCanvasGroup.DOFade(0f, aniTime2))
                    .OnComplete(() =>
                    {
                        transition = null;
                        if (this && UIPanel && UIPanel.IsOpening && UIPanel.SerialId == serialId) CloseImmediately();
                    })
                    .SetTarget(this)
                    .SetUpdate(true);
            }
            else
            {
                CloseImmediately();
            }
        }

        /// <summary>
        /// 通过所属管理器关闭页面
        /// </summary>
        private void CloseImmediately()
        {
            if (UIPanel && UIPanel.UIGroup is UIGroup group) group.ClosePanel(UIPanel);
        }

        /// <summary>
        /// 缺少动画节点时降级为直接开关
        /// </summary>
        private bool CanAnimate()
        {
            if (node && bgMaskCanvasGroup) return true;
            Debug.LogWarning($"UI动画节点不完整 {Name}");
            return false;
        }

        /// <summary>
        /// 终止动画但不执行旧完成回调
        /// </summary>
        private void StopTransition()
        {
            transition?.Kill(false);
            transition = null;
            DOTween.Kill(this, false);
        }

        /// <summary>
        /// 重置默认动画表现
        /// </summary>
        private void ResetPresentation()
        {
            if (node) node.localPosition = startLocalPos;
            if (bgMaskCanvasGroup) bgMaskCanvasGroup.alpha = 1f;
            if (canvasGroup) canvasGroup.alpha = 1f;
            isClosing = false;
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

        /// <summary>
        /// 将初始化数据转换为当前页面数据类型
        /// </summary>
        public virtual void OnInit(object userData)
        {
            OnInit(userData as T);
        }

        private Vector3 startLocalPos; //动画节点初始位置
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
            StopTransition();
            ResetPresentation();
            visible = false;
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
            
            //默认动画
            if (needOpenAni && CanAnimate())
            {
                bgMaskCanvasGroup.alpha = 0.8f;
                node.localPosition = startLocalPos - new Vector3(0, Screen.height, 0);
                transition = DOTween.Sequence()
                    .Append(node.DOLocalMove(startLocalPos, aniTime).SetEase(Ease.Linear))
                    .Join(bgMaskCanvasGroup.DOFade(1f, aniTime))
                    .OnComplete(() =>
                    {
                        bgMaskCanvasGroup.alpha = 1f;
                        node.transform.localPosition = startLocalPos;
                        transition = null;
                    })
                    .SetTarget(this)
                    .SetUpdate(true);
            }
        }

        /// <summary>
        /// 界面关闭
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发</param>
        /// <param name="userData">用户自定义数据</param>
        public virtual void OnClose(bool isShutdown, object userData)
        {
            StopAllCoroutines();
            StopTransition();
            ResetPresentation();
            gameObject.SetLayerRecursively(originalLayer);
            visible = false;
            SetVisible(false);
            Available = false;
            windowData = null;
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
            //逻辑深度回调保留 实际 Canvas 排序由管理器统一提交
        }

        /// <summary>
        /// 界面回收
        /// </summary>
        public virtual void OnRecycle()
        {
            StopAllCoroutines();
            StopTransition();
            ResetPresentation();
            windowData = null;
            Available = false;
            visible = false;
            SetVisible(false);
        }

        /// <summary>
        /// 销毁时清理动画引用
        /// </summary>
        protected virtual void OnDestroy()
        {
            StopTransition();
            windowData = null;
        }

        #endregion 生命周期
    }
}
