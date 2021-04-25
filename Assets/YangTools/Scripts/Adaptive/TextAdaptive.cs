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
    [AddComponentMenu(SettingInfo.menuMark + "/TextAdaptive", 20)]
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
        public bool m_AdaptiveSize = true;
        /// <summary>
        /// 适配模式：默认纵向适配
        /// </summary>
        [Header("适配模式")]
        public StartAxisType m_StartAxis = StartAxisType.Vertical;
        /// <summary>
        /// 最开始字体大小
        /// </summary>
        private int m_BeginFontSize;
        /// <summary>
        /// 文字框的大小
        /// </summary>
        private Rect m_RectSize;
        /// <summary>
        /// 适配的文字
        /// </summary>
        private Text m_Text;
        /// <summary>
        /// 是否已经完成适配
        /// </summary>
        private bool isAdaptiveOver;
        #endregion

        #region 生命周期
        void Awake()
        {
            m_Text = transform.GetComponent<Text>();
            m_Text.alignByGeometry = false;
            m_Text.resizeTextForBestFit = false;
            m_BeginFontSize = m_Text.fontSize;
        }

        void OnEnable()
        {
            m_Text.RegisterDirtyVerticesCallback(OnTextChange);

            if (isReuse)
            {
                isAdaptiveOver = false;
            }
        }

        void LateUpdate()
        {
            //不适配直接return
            if (!m_AdaptiveSize) return;
            //如果已经适配return
            if (isAdaptiveOver) return;

            m_RectSize = transform.GetComponent<RectTransform>().rect;
            switch (m_StartAxis)
            {
                case StartAxisType.Horizontal:
                    {
                        m_Text.fontSize = m_BeginFontSize;
                        int time = whileMax;
                        while (m_Text.preferredWidth > m_RectSize.width)
                        {
                            m_Text.fontSize -= 1;

                            if (m_Text.fontSize < 1)
                            {
                                Debug.LogError($"TextAdaptive:字体大小缩小到1依然无法适配==>{m_Text.text}");
                                break;
                            }
                            time--;
                            if (time <= 0)
                            {
                                Debug.LogError($"TextAdaptive:超过上限{whileMax}循环次数==>{m_Text.text}");
                                break;
                            }
                        }
                        break;
                    }
                case StartAxisType.Vertical:
                    {
                        m_Text.fontSize = m_BeginFontSize;
                        int time = whileMax;
                        while (m_Text.preferredHeight > m_RectSize.height)
                        {
                            m_Text.fontSize -= 1;

                            if (m_Text.fontSize < 1)
                            {
                                Debug.LogError($"TextAdaptive:字体大小缩小到1依然无法适配==>{m_Text.text}");
                                break;
                            }
                            time--;
                            if (time <= 0)
                            {
                                Debug.LogError($"TextAdaptive:超过上限{whileMax}循环次数==>{m_Text.text}");
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
            m_Text.UnregisterDirtyVerticesCallback(OnTextChange);
        }
        #endregion

        #region 方法
        /// <summary>
        /// 文字改变回调
        /// </summary>
        public void OnTextChange()
        {
            //TODO 中文添加，英文会有问题(会不按单词换行)
            //if (mText.text.Contains(" "))
            //{
            //    mText.text = mText.text.Replace(" ", no_breaking_space);
            //}

            isAdaptiveOver = false;
        }
        #endregion
    }
}
