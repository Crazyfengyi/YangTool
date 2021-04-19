using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace YangTools
{
    [RequireComponent(typeof(Image), typeof(RectMask2D))]
    /// <summary>
    /// 文本走马灯效果
    /// </summary>
    public class TextWalkingLantern : MonoBehaviour
    {
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

        /// <summary>
        /// 遮罩框
        /// </summary>
        private Image maskImage;
        /// <summary>
        /// Text组件
        /// </summary>
        public Text m_Text;
        /// <summary>
        /// 开始位置
        /// </summary>
        private float startPos = 0;
        /// <summary>
        /// ?
        /// </summary>
        float startPosBlack;
        /// <summary>
        /// 结束位置
        /// </summary>
        private float moveEndPos = 0;

        private DefaultShow defaultShow = DefaultShow.None;

        private void Awake()
        {
            maskImage = GetComponent<Image>();

            if (m_Text == null)
            {
                m_Text = transform.GetChild(0).GetComponent<Text>();
            }

            m_Text.RegisterDirtyVerticesCallback(OnTextChange);
            SetText(m_Text.text, DefaultShow.中间);
        }

        private void OnDestroy()
        {
            m_Text.UnregisterDirtyVerticesCallback(OnTextChange);
        }
        /// <summary>
        /// 文本循环滚动
        /// </summary>
        /// <returns></returns>
        private IEnumerator TextCycle()
        {
            m_Text.transform.ChangeLocalPosX(startPos);

            yield return new WaitForSeconds(0.3f); //受Time.timeScale时间影响

            float progress = 0f;//进度0~1
            float speed = 0.2f;//速度
            while (progress < 1)
            {
                progress += Time.deltaTime * speed;
                float xPos = Mathf.Lerp(startPos, moveEndPos, progress);
                m_Text.transform.ChangeLocalPosX(xPos);
                yield return null;
            }

            yield return new WaitForSeconds(0.23f); //受Time.timeScale时间影响

            float oldAlpha = m_Text.color.a;
            DOTween.Sequence()
                .Append(m_Text.DOFade(0, 0.13f))
                .AppendCallback(() =>
                {
                    m_Text.transform.ChangeLocalPosX(startPos);
                })
                .Append(m_Text.DOFade(oldAlpha, 0.13f))
                .OnKill(() =>
                {
                    m_Text.ChangeAlpha(oldAlpha);
                    m_Text.transform.ChangeLocalPosX(startPos);
                })
                .SetEase(Ease.Linear)
                .SetTarget(this);

            yield return new WaitForSeconds(0.26f);

            StartCoroutine("TextCycle");
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        /// <param name="str">要显示的字符串</param>
        /// <param name="defaultShowType">默认显示位置(当字符长度每超出框--不用走马灯时的显示位置)</param>
        public void SetText(string str, DefaultShow defaultShowType = DefaultShow.中间)
        {
            m_Text.text = str;
            float maskSize = maskImage.rectTransform.sizeDelta.x;//遮罩x大小
            float difference = m_Text.preferredWidth - maskImage.rectTransform.sizeDelta.x; //字的完美宽度和遮罩的差值

            defaultShow = defaultShowType;

            //是否需要走马灯效果
            if (difference > 0)
            {
                m_Text.rectTransform.pivot = new Vector2(0, 0.5f);
                m_Text.transform.localPosition = new Vector2(-maskSize / 2, 0);

                startPos = -maskSize / 2;
                moveEndPos = startPos - difference;

                StopCoroutine("TextCycle");
                DOTween.Kill(this);
                StartCoroutine("TextCycle");
            }
            else
            {
                switch (defaultShow)
                {
                    case DefaultShow.左边:
                        m_Text.rectTransform.pivot = new Vector2(0, 0.5f);
                        m_Text.transform.localPosition = new Vector2(-maskSize / 2, 0);
                        break;
                    case DefaultShow.中间:
                        m_Text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        m_Text.transform.localPosition = new Vector2(0, 0);
                        break;
                    case DefaultShow.右边:
                        m_Text.rectTransform.pivot = new Vector2(1f, 0.5f);
                        m_Text.transform.localPosition = new Vector2(maskSize / 2, 0);
                        break;
                    default:
                        m_Text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        m_Text.transform.localPosition = new Vector2(0, 0);
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
            return m_Text.text;
        }
    }
}