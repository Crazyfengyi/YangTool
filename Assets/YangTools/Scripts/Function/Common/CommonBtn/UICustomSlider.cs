using System.Globalization;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameMain
{
    /// <summary>
    /// 通用滑动块
    /// </summary>
    public class UICustomSlider : MonoBehaviour
    {
        // 计算滑块位置使用的父级矩形
        public RectTransform sliderParent;
        // 显示进度的填充图像
        public Image imgSlider;
        // 显示当前位置的滑块图像
        public Image imgHandler;
        // 显示数值的文本组件
        public TextMeshProUGUI text;

        // 当前归一化进度
        [OnValueChanged("Refersh")] [Range(0, 1)]
        public float currentProgress;

        // 动画中的当前数值
        private float currentValue;

        /// <summary>
        /// 设置进度百分比
        /// </summary>
        /// <param name="value">当前值</param>
        /// <param name="maxValue">最大值</param>
        public void SetProgressPercent(float value, float maxValue)
        {
            if (text) text.text = $"{imgSlider.fillAmount * 100}%";
            SetProgress(value / maxValue);
        }
        /// <summary>
        /// 设置进度(动画)
        /// </summary>
        /// <param name="value">本次设置的数值</param>
        /// <param name="maxValue">用于归一化的最大数值</param>
        /// <param name="aniTime">过渡动画的持续秒数</param>
        public void SetProgressAni(float value, float maxValue, float aniTime)
        {
            currentValue = Mathf.Min(value, currentValue);
            DOTween.To(() => currentValue, (value) =>
            {
                currentValue = value;
                float newProgress = currentValue / maxValue;
                SetProgress(newProgress);
                text.text = $"{currentValue}/{maxValue}";
            }, value, aniTime).OnComplete(() => { currentValue = value; });
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        public void SetProgress(float progress)
        {
            imgSlider.fillAmount = progress;
            currentProgress = progress;
            if (imgHandler)
            {
                imgHandler.transform.localPosition = new Vector3(sliderParent.sizeDelta.x * (progress - 0.5f), 0, 0);
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// 将编辑器中修改的进度同步到滑动条显示
        /// </summary>
        /// <param name="newProgress">归一化显示进度</param>
        private void Refersh(float newProgress)
        {
            SetProgress(newProgress);
        }
#endif
    }
}