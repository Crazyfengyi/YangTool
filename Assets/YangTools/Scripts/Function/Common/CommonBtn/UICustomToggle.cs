using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    [RequireComponent(typeof(UICustomButton))]
    public class UICustomToggle : MonoBehaviour
    {
        public GameObject imgOnGo;
        public GameObject imgOffGo;
        public UICustomButton btnToggle;
        public Action<bool> OnToggleClickCallback { get; set; }
        public Action<bool> OnToggleChangeCallback { get; set; }
        [LabelText("是否激活")]
        [OnValueChanged("OnToggleChange")]
        public bool isOn;

        private void Awake()
        {
            btnToggle.AddListener(OnToggleClick);
        }
        /// <summary>
        /// 设置Toggle
        /// </summary>
        public void SetToggle(bool _isOn)
        {
            isOn = _isOn;
            OnToggleChangeCallback?.Invoke(isOn);
            RefreshShow();
        }
        /// <summary>
        /// 按钮点击
        /// </summary>
        private void OnToggleClick()
        {
            RefreshShow();
            OnToggleClickCallback?.Invoke(isOn);
            OnToggleChangeCallback?.Invoke(isOn);
        }
        
        private void RefreshShow()
        {
            if (isOn)
            {
                imgOnGo.gameObject.SetActive(true);
                imgOffGo.gameObject.SetActive(false);
            }
            else
            {
                imgOnGo.gameObject.SetActive(false);
                imgOffGo.gameObject.SetActive(true);
            }
        }

#if UNITY_EDITOR

        private void OnToggleChange(bool ison)
        {
            RefreshShow();
        }
        private void OnValidate()
        {
            btnToggle = transform.GetComponent<UICustomButton>();
        }
#endif
    }
}