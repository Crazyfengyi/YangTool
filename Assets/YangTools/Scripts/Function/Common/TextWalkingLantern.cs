using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YangTools.Extend;

namespace YangTools
{
    /// <summary>
    /// 文本走马灯效果
    /// </summary>
    [RequireComponent(typeof(Image), typeof(RectMask2D))]
    public class TextWalkingLantern : MonoBehaviour
    {
        /// <summary>
        /// 遮罩框
        /// </summary>
        private Image maskImage;
        /// <summary>
        /// Text组件
        /// </summary>
        [FormerlySerializedAs("m_Text")]
        public Text mText;
        /// <summary>
        /// 开始位置
        /// </summary>
        private float startPos = 0;
        /// <summary>
        /// 结束位置
        /// </summary>
        private float moveEndPos = 0;

        private DefaultShow defaultShow = DefaultShow.None;

        private void Awake()
        {
            maskImage = GetComponent<Image>();

            if (mText == null)
            {
                mText = transform.GetChild(0).GetComponent<Text>();
            }

            mText.RegisterDirtyVerticesCallback(OnTextChange);
            SetText(mText.text, DefaultShow.中间);
        }

        private void OnDestroy()
        {
            mText.UnregisterDirtyVerticesCallback(OnTextChange);
        }
        /// <summary>
        /// 文本循环滚动
        /// </summary>
        /// <returns></returns>
        private IEnumerator TextCycle()
        {
            mText.transform.ChangeLocalPosX(startPos);

            yield return new WaitForSeconds(0.3f); //受Time.timeScale时间影响

            float progress = 0f;//进度0~1
            float speed = 0.2f;//速度
            while (progress < 1)
            {
                progress += Time.deltaTime * speed;
                float xPos = Mathf.Lerp(startPos, moveEndPos, progress);
                mText.transform.ChangeLocalPosX(xPos);
                yield return null;
            }

            yield return new WaitForSeconds(0.23f); //受Time.timeScale时间影响

            float oldAlpha = mText.color.a;
            DOTween.Sequence()
                .Append(mText.DOFade(0, 0.13f))
                .AppendCallback(() =>
                {
                    mText.transform.ChangeLocalPosX(startPos);
                })
                .Append(mText.DOFade(oldAlpha, 0.13f))
                .OnKill(() =>
                {
                    mText.ChangeAlpha(oldAlpha);
                    mText.transform.ChangeLocalPosX(startPos);
                })
                .SetEase(Ease.Linear)
                .SetTarget(this);

            yield return new WaitForSeconds(0.26f);

            StartCoroutine(nameof(TextCycle));
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        /// <param name="str">要显示的字符串</param>
        /// <param name="defaultShowType">默认显示位置(当字符长度每超出框--不用走马灯时的显示位置)</param>
        public void SetText(string str, DefaultShow defaultShowType = DefaultShow.中间)
        {
            mText.text = str;
            float maskSize = maskImage.rectTransform.sizeDelta.x;//遮罩x大小
            float difference = mText.preferredWidth - maskImage.rectTransform.sizeDelta.x; //字的完美宽度和遮罩的差值

            defaultShow = defaultShowType;

            //是否需要走马灯效果
            if (difference > 0)
            {
                mText.rectTransform.pivot = new Vector2(0, 0.5f);
                mText.transform.localPosition = new Vector2(-maskSize / 2, 0);

                startPos = -maskSize / 2;
                moveEndPos = startPos - difference;

                StopCoroutine(nameof(TextCycle));
                DOTween.Kill(this);
                StartCoroutine(nameof(TextCycle));
            }
            else
            {
                switch (defaultShow)
                {
                    case DefaultShow.左边:
                        mText.rectTransform.pivot = new Vector2(0, 0.5f);
                        mText.transform.localPosition = new Vector2(-maskSize / 2, 0);
                        break;
                    case DefaultShow.中间:
                        mText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        mText.transform.localPosition = new Vector2(0, 0);
                        break;
                    case DefaultShow.右边:
                        mText.rectTransform.pivot = new Vector2(1f, 0.5f);
                        mText.transform.localPosition = new Vector2(maskSize / 2, 0);
                        break;
                    default:
                        mText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        mText.transform.localPosition = new Vector2(0, 0);
                        break;
                }
            }
        }

        /// <summary>
        /// 文本改变回调
        /// </summary>
        private void OnTextChange()
        {
            //SetText(mText.text, defaultShow);
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        public string GetText()
        {
            return mText.text;
        }
    }
    
    /// <summary>
    /// 默认显示
    /// </summary>
    public enum DefaultShow
    {
        None,
        左边,
        中间,
        右边
    }
}