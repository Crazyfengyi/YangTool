using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameMain
{
    /// <summary>
    /// 以按钮和两组外观显示开关状态并派发状态回调
    /// </summary>
    [RequireComponent(typeof(UICustomButton))]
    public class UICustomToggle : MonoBehaviour
    {
        // 开启状态的显示对象
        public GameObject imgOnGo;
        // 关闭状态的显示对象
        public GameObject imgOffGo;
        // 接收开关点击的通用按钮
        public UICustomButton btnToggle;
        public Action<bool> OnToggleClickCallback { get; set; }
        public Action<bool> OnToggleChangeCallback { get; set; }
        // 是否激活
        [LabelText("是否激活")]
        [OnValueChanged("OnToggleChange")]
        public bool isOn;

        /// <summary>
        /// 向通用按钮注册开关点击回调
        /// </summary>
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
        
        /// <summary>
        /// 根据当前开关状态切换选中与未选中物体
        /// </summary>
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

        /// <summary>
        /// 在编辑器修改开关状态时刷新外观
        /// </summary>
        /// <param name="ison">编辑器传入的开关状态 当前显示读取组件字段</param>
        private void OnToggleChange(bool ison)
        {
            RefreshShow();
        }
        /// <summary>
        /// 在编辑器校验时取得同物体的通用按钮
        /// </summary>
        private void OnValidate()
        {
            btnToggle = transform.GetComponent<UICustomButton>();
        }
#endif
    }
}