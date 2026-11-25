using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YangTools.Scripts.Core.ResourceManager;
using YangTools.Scripts.Core.YangAudio;

namespace GameMain
{
    /// <summary>
    /// 通用按钮
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UICustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // 点击音效
        [LabelText("点击音效")] 
        public bool needClickAudio = true;
        // 音效名称
        [LabelText("音效名称")] [ShowIf("ClickAudio")]
        public string clipName;
        // 点击动画
        [LabelText("点击动画")] 
        public bool needAni = true;
        // 点击间隔
        [LabelText("点击间隔")] 
        public float clickInterval = 0.2f;

        // 接收 Unity 点击事件的按钮
        private Button targetButton;
        // 按钮背景图像
        private Image btnImgBg;
        // 通用按钮点击回调
        private Action callback;
        
        // 按压前的初始缩放
        private Vector3 startScale;
        // 上次点击的非缩放时间
        private float lastClickTime;
        
        // 默认按钮音效名称
        private static readonly string DefaultAudioName = "ClickDefaultAudio";
        /// <summary>
        /// 初始化按钮引用 初始缩放与点击事件
        /// </summary>
        private void Awake()
        {
            targetButton = GetComponent<Button>();
            btnImgBg = targetButton.GetComponent<Image>();
            startScale = transform.localScale;
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public void AddListener(Action onClick)
        {
            if (targetButton == null)
            {
                Debug.LogError("目标按钮=NULL");
                return;
            }
            callback = onClick;
            targetButton.onClick.RemoveListener(OnBtnClick);
            targetButton.onClick.AddListener(OnBtnClick);
        }

        /// <summary>
        /// 按钮点击
        /// </summary>
        private void OnBtnClick()
        {
            var realtimeSinceStartup = Time.realtimeSinceStartup;
            //点击间隔
            if (realtimeSinceStartup - lastClickTime < clickInterval) return;
            if (needClickAudio)
            {
                //是空就用默认按钮音效
                if (string.IsNullOrEmpty(clipName))
                {
                    clipName = DefaultAudioName;
                }
                YangAudioManager.Instance.PlaySoundAudio(clipName);
            }
            lastClickTime = realtimeSinceStartup;
            callback?.Invoke();
        }

#region 点击

        /// <summary>
        /// 按配置播放按钮按压缩放
        /// </summary>
        /// <param name="eventData">本次指针交互数据</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            DOTween.Kill(gameObject);
            if (needAni)
            {
                transform.DOScale(startScale * 0.93f, 0.2f).SetTarget(gameObject);
            }
        }

        /// <summary>
        /// 按配置恢复按钮缩放
        /// </summary>
        /// <param name="eventData">本次指针交互数据</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            DOTween.Kill(gameObject);
            if (needAni)
            {
                transform.DOScale(startScale, 0.2f).SetTarget(gameObject);
            }
        }

#endregion
        
#region 其它表现
        /// <summary>
        /// 设置置灰
        /// </summary>
        public async void SetGray(bool isGray)
        {
            if (btnImgBg)
            {
                if (isGray)
                {
                    var grayMaterial = await ResourceManager.LoadAssetAsync<Material>("UIGray_Static");
                    btnImgBg.material = grayMaterial;
                }
                else
                {
                    btnImgBg.material = null;
                }
            }
        }
#endregion

#if UNITY_EDITOR
        private bool ClickAudio => needClickAudio;
        /// <summary>
        /// 在编辑器校验时补充按钮相关组件引用
        /// </summary>
        private void OnValidate()
        {
            targetButton = GetComponent<Button>();
            targetButton.transition = Selectable.Transition.None;
        }
#endif
    }
}