using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace YangTools.MiniMap
{
    public class MiniMapIcon : MonoBehaviour
    {
        [LabelText("图标")]
        public Image icon;
        [FormerlySerializedAs("DeathIcon")] [LabelText("死亡图标")]
        public Sprite deathIcon;
        [LabelText("文字信息")]
        public TMP_Text text;
        [FormerlySerializedAs("CircleAreaRect")]
        [LabelText("范围显示")]
        [SerializeField]
        private RectTransform circleAreaRect;
        [LabelText("延时删除")]
        public float destroyDelay = 5f;

        private float showDelay = 0.1f;
        private CanvasGroup canvasGroup;
        private MiniMapMaskHelper maskHelper;

        private void Awake()
        {
            if (GetComponent<CanvasGroup>() != null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            else
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // m_CanvasGroup.ignoreParentGroups = true;
            canvasGroup.alpha = 0;

            if (circleAreaRect != null)
            {
                circleAreaRect.gameObject.SetActive(false);
            }
        }

        public void SetIcon(Sprite sprite, Color color = default)
        {
            icon.sprite = sprite;
            if (color != default) icon.color = color;
        }

        /// <summary>
        /// 设置文字显示
        /// </summary>
        public void SetTextShow(string str)
        {
            if (text == null) return;
            text.text = str;
        }

        /// <summary>
        /// 显示攻击圆形范围
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="areaColor">颜色</param>
        public RectTransform SetCircleArea(float radius, Color areaColor)
        {
            if (circleAreaRect == null) { return null; }

            maskHelper = transform.root.GetComponentInChildren<MiniMapMaskHelper>();
            maskHelper.SetMaskedIcon(circleAreaRect);
            radius = radius * 10;
            radius = radius * MiniMapUtils.GetMiniMap().iconMultiplier;
            Vector2 tempRadius = new Vector2(radius, radius);
            circleAreaRect.sizeDelta = tempRadius;
            circleAreaRect.GetComponent<Image>().CrossFadeColor(areaColor, 1, true, true);
            circleAreaRect.gameObject.SetActive(true);
            return circleAreaRect;
        }

        /// <summary>
        /// 隐藏攻击范围
        /// </summary>
        public void HideCircleArea()
        {
            circleAreaRect.SetParent(transform);
            circleAreaRect.gameObject.SetActive(false);
        }

        /// <summary>
        /// 延迟显示
        /// </summary>
        public void DelayStartShow(float time)
        {
            showDelay = time;
            StartCoroutine(FadeIcon());
        }

        /// <summary>
        /// 淡出
        /// </summary>
        private IEnumerator FadeIcon()
        {
            yield return new WaitForSeconds(showDelay);
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * 2;
                yield return null;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="immediate">立即删除</param>
        /// <param name="deathSprite">死亡图标</param>
        public void DestroyIcon(bool immediate, Sprite deathSprite = null)
        {
            if (immediate)
            {
                Destroy(gameObject);
            }
            else
            {
                icon.sprite = deathSprite == null ? deathIcon : deathSprite;
                Destroy(gameObject, destroyDelay);
            }
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        public void SetVisibleAlpha()
        {
            canvasGroup.alpha = 1;
        }
    }
}