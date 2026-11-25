using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YangTools.Scripts.Core;

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

    /// <summary>
    /// Text文本自动适配
    /// </summary>
    [TypeInfoBox("文字适配")]
    [RequireComponent(typeof(Text))]
    [AddComponentMenu(SettingInfo.MenuPath + "TextAdaptive", 20)]
    public class TextAdaptive : MonoBehaviour
    {
        // 用于识别普通半角空格
        private const char NormalSpace = ' ';
        // 用于替换中文相邻空格的不换行字符
        private const char NoBreakingSpaceCharacter = '\u00A0';

        // Android/iOS通用空格格式(不自动换行的空格)---space(0x0020)替换为No-break-space(0x00A0) 避免中文使用半角空格导致的提前换行问题
        public static readonly string NoBreakingSpace = NoBreakingSpaceCharacter.ToString();
        // 防止while死循环设置的上限
        private const int WhileMax = 10000;

        #region 属性
        // 是否动态更改文本
        [Header("是否动态适配(代码是否会动态更改文字)")]
        public bool isReuse;
        // 是否适配
        [FormerlySerializedAs("m_AdaptiveSize")] [HideInInspector]
        public bool mAdaptiveSize = true;
        // 适配模式 默认纵向适配
        [FormerlySerializedAs("m_StartAxis")] [Header("适配模式")]
        public StartAxisType mStartAxis = StartAxisType.Vertical;
        // 最开始字体大小
        private int mBeginFontSize;
        // 文字框的大小
        private Rect mRectSize;
        // 适配的文字
        private Text mText;
        // 是否已经完成适配
        private bool isAdaptiveOver;
        #endregion

        #region 生命周期
        /// <summary>
        /// 缓存文本组件和初始字号并关闭内置最佳适配
        /// </summary>
        void Awake()
        {
            mText = transform.GetComponent<Text>();
            mText.alignByGeometry = false;
            mText.resizeTextForBestFit = false;
            mBeginFontSize = mText.fontSize;
        }

        /// <summary>
        /// 注册文本顶点变化回调并标记重新适配
        /// </summary>
        void OnEnable()
        {
            mText.RegisterDirtyVerticesCallback(OnTextChange);

            if (isReuse)
            {
                isAdaptiveOver = false;
            }
        }

        /// <summary>
        /// 在布局更新后按选定方向缩小字号直至适配或达到迭代上限
        /// </summary>
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
                        int time = WhileMax;
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
                                Debug.LogError($"TextAdaptive:超过上限{WhileMax}循环次数==>{mText.text}");
                                break;
                            }
                        }
                        break;
                    }
                case StartAxisType.Vertical:
                    {
                        mText.fontSize = mBeginFontSize;
                        int time = WhileMax;
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
                                Debug.LogError($"TextAdaptive:超过上限{WhileMax}循环次数==>{mText.text}");
                                break;
                            }
                        }
                        break;
                    }
            }

            isAdaptiveOver = true;
        }

        /// <summary>
        /// 取消文本顶点变化回调
        /// </summary>
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
            ReplaceChineseSpaces();
            isAdaptiveOver = false;
        }

        /// <summary>
        /// 将中文相邻的半角空格替换为不换行空格 保留英文单词之间的正常换行能力
        /// </summary>
        private void ReplaceChineseSpaces()
        {
            string content = mText.text;
            if (string.IsNullOrEmpty(content) || content.IndexOf(NormalSpace) < 0)
            {
                return;
            }

            char[] characters = null;
            for (int index = 0; index < content.Length; index++)
            {
                if (content[index] != NormalSpace)
                {
                    continue;
                }

                int spaceEndIndex = index;
                while (spaceEndIndex + 1 < content.Length && content[spaceEndIndex + 1] == NormalSpace)
                {
                    spaceEndIndex++;
                }

                bool hasChineseBefore = index > 0 && IsChineseCharacter(content[index - 1]);
                bool hasChineseAfter = spaceEndIndex + 1 < content.Length && IsChineseCharacter(content[spaceEndIndex + 1]);
                if (hasChineseBefore || hasChineseAfter)
                {
                    characters ??= content.ToCharArray();
                    for (int spaceIndex = index; spaceIndex <= spaceEndIndex; spaceIndex++)
                    {
                        characters[spaceIndex] = NoBreakingSpaceCharacter;
                    }
                }

                index = spaceEndIndex;
            }

            if (characters != null)
            {
                mText.text = new string(characters);
            }
        }

        /// <summary>
        /// 判断字符是否属于常用中文字符范围
        /// </summary>
        private static bool IsChineseCharacter(char character)
        {
            return character >= '\u3400' && character <= '\u4DBF'
                   || character >= '\u4E00' && character <= '\u9FFF'
                   || character >= '\uF900' && character <= '\uFAFF';
        }
        #endregion
    }
}
