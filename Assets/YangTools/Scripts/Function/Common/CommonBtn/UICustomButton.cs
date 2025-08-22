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
        [LabelText("点击音效")] 
        public bool needClickAudio = true;
        [LabelText("音效名称")] [ShowIf("ClickAudio")]
        public string clipName;
        [LabelText("点击动画")] 
        public bool needAni = true;
        [LabelText("点击间隔")] 
        public float clickInterval = 0.2f;

        private Button targetButton;
        private Image btnImgBg;
        private Action callback;
        
        private Vector3 startScale;
        private float lastClickTime;
        
        private static readonly string DefaultAudioName = "ClickDefaultAudio";
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

        public void OnPointerDown(PointerEventData eventData)
        {
            DOTween.Kill(gameObject);
            if (needAni)
            {
                transform.DOScale(startScale * 0.93f, 0.2f).SetTarget(gameObject);
            }
        }

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
        private void OnValidate()
        {
            targetButton = GetComponent<Button>();
            targetButton.transition = Selectable.Transition.None;
        }
#endif
    }
}