using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace YangTools
{
    /// <summary>
    /// 适配模式
    /// </summary>
    public enum StartAxisType
    {
        /// <summary>
        /// 横向适配
        /// </summary>
        [InspectorName("横向适配")]
        Horizontal,
        /// <summary>
        /// 纵向适配
        /// </summary>
        [InspectorName("纵向适配")]
        Vertical
    }

    [TypeInfoBox("文字适配")]
    [RequireComponent(typeof(Text))]
    /// <summary>
    /// Text文本自动适配
    /// </summary>
    public class TextAdaptive : MonoBehaviour
    {
        /// <summary>
        /// Android/iOS通用空格格式(不自动换行的空格)---space(0x0020)替换为No-break-space(0x00A0)，避免中文使用半角空格导致的提前换行问题
        /// </summary>
        public static readonly string no_breaking_space = "\u00A0";
        /// <summary>
        /// 防止while死循环设置的上限
        /// </summary>
        private static readonly int whileMax = 10000;

        #region 属性
        /// <summary>
        /// 是否动态更改文本
        /// </summary>
        [Header("是否动态适配(代码是否会动态更改文字)")]
        public bool isReuse;
        /// <summary>
        /// 是否适配
        /// </summary>
        [HideInInspector]
        public bool mAdaptiveSize = true;
        /// <summary>
        /// 适配模式：默认纵向适配
        /// </summary>
        [Header("适配模式")]
        public StartAxisType mStartAxis = StartAxisType.Vertical;
        /// <summary>
        /// 最开始字体大小
        /// </summary>
        private int mBeginFontSize;
        /// <summary>
        /// 文字框的大小
        /// </summary>
        private Rect mRectSize;
        /// <summary>
        /// 适配的文字
        /// </summary>
        private Text mText;
        /// <summary>
        /// 是否已经完成适配
        /// </summary>
        private bool isAdaptiveOver;
        #endregion

        #region 生命周期
        void Awake()
        {
            mText = transform.GetComponent<Text>();
            mText.alignByGeometry = false;
            mText.resizeTextForBestFit = false;
            mBeginFontSize = mText.fontSize;
        }

        void OnEnable()
        {
            mText.RegisterDirtyVerticesCallback(OnTextChange);

            if (isReuse)
            {
                isAdaptiveOver = false;
            }
        }

        void LateUpdate()
        {
            //不适配直接return
            if (!mAdaptiveSize) return;
            //如果已经适配return
            if (isAdaptiveOver) return;

            mRectSize = transform.GetComponent<RectTransform>().rect;
            switch (mStartAxis)
            {
                case StartAxisType.Horizontal:
                    {
                        mText.fontSize = mBeginFontSize;
                        int time = whileMax;
                        while (mText.preferredWidth > mRectSize.width)
                        {
                            mText.fontSize -= 1;

                            if (mText.fontSize < 1)
                            {
                                Debug.LogError($"TextAdaptive:字体大小缩小到1依然无法适配==>{mText.text}");
                                break;
                            }
                            time--;
                            if (time <= 0)
                            {
                                Debug.LogError($"TextAdaptive:超过上限{whileMax}循环次数==>{mText.text}");
                                break;
                            }
                        }
                        break;
                    }
                case StartAxisType.Vertical:
                    {
                        mText.fontSize = mBeginFontSize;
                        int time = whileMax;
                        while (mText.preferredHeight > mRectSize.height)
                        {
                            mText.fontSize -= 1;

                            if (mText.fontSize < 1)
                            {
                                Debug.LogError($"TextAdaptive:字体大小缩小到1依然无法适配==>{mText.text}");
                                break;
                            }
                            time--;
                            if (time <= 0)
                            {
                                Debug.LogError($"TextAdaptive:超过上限{whileMax}循环次数==>{mText.text}");
                                break;
                            }
                        }
                        break;
                    }
            }

            isAdaptiveOver = true;
        }

        private void OnDisable()
        {
            mText.UnregisterDirtyVerticesCallback(OnTextChange);
        }
        #endregion

        #region 方法
        /// <summary>
        /// 文字改变回调
        /// </summary>
        public void OnTextChange()
        {
            //TODO 中文添加，英文添加会有问题(会不按单词换行)
            //if (mText.text.Contains(" "))
            //{
            //    mText.text = mText.text.Replace(" ", no_breaking_space);
            //}

            isAdaptiveOver = false;
        }
        #endregion
    }
}
