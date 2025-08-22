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
        public RectTransform sliderParent;
        public Image imgSlider;
        public Image imgHandler;
        public TextMeshProUGUI text;

        [OnValueChanged("Refersh")] [Range(0, 1)]
        public float currentProgress;

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
        /// <param name="value"></param>
        /// <param name="maxValue"></param>
        /// <param name="aniTime"></param>
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

        private void Refersh(float newProgress)
        {
            SetProgress(newProgress);
        }
#endif
    }
}