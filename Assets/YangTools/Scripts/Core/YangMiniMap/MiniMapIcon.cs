using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YangTools.MiniMap
{
    public class MiniMapIcon : MonoBehaviour
    {
        [LabelText("图标")]
        public Image icon;
        [LabelText("死亡图标")]
        public Sprite DeathIcon;
        [LabelText("文字信息")]
        public TMP_Text text;
        [LabelText("范围显示")]
        [SerializeField]
        private RectTransform CircleAreaRect;
        [LabelText("延时删除")]
        public float destroyDelay = 5f;

        private float showDelay = 0.1f;
        private CanvasGroup canvasGroup;
        private MiniMapMaskHelper MaskHelper;

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

            if (CircleAreaRect != null)
            {
                CircleAreaRect.gameObject.SetActive(false);
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
        /// <param name="AreaColor">颜色</param>
        public RectTransform SetCircleArea(float radius, Color AreaColor)
        {
            if (CircleAreaRect == null) { return null; }

            MaskHelper = transform.root.GetComponentInChildren<MiniMapMaskHelper>();
            MaskHelper.SetMaskedIcon(CircleAreaRect);
            radius = radius * 10;
            radius = radius * MiniMapUtils.GetMiniMap().iconMultiplier;
            Vector2 tempRadius = new Vector2(radius, radius);
            CircleAreaRect.sizeDelta = tempRadius;
            CircleAreaRect.GetComponent<Image>().CrossFadeColor(AreaColor, 1, true, true);
            CircleAreaRect.gameObject.SetActive(true);
            return CircleAreaRect;
        }

        /// <summary>
        /// 隐藏攻击范围
        /// </summary>
        public void HideCircleArea()
        {
            CircleAreaRect.SetParent(transform);
            CircleAreaRect.gameObject.SetActive(false);
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
        /// <param name="inmediate">立即删除</param>
        /// <param name="deathSprite">死亡图标</param>
        public void DestroyIcon(bool inmediate, Sprite deathSprite = null)
        {
            if (inmediate)
            {
                Destroy(gameObject);
            }
            else
            {
                icon.sprite = deathSprite == null ? DeathIcon : deathSprite;
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