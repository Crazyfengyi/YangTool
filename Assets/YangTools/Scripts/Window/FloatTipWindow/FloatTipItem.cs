using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FloatTipItem : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public Image bg;
    public TextMeshProUGUI tipText;

    /// <summary>
    /// 播放飘字动画。
    /// </summary>
    /// <param name="text">飘字文本。</param>
    /// <param name="anchoredPosition">起始锚点坐标。</param>
    /// <param name="color">文本颜色。</param>
    /// <param name="onComplete">播放完成回调。</param>
    public void Play(string text, Vector2 anchoredPosition, Action onComplete)
    {
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.localScale = Vector3.one * 1.12f;

        tipText.text = text;
        tipText.raycastTarget = false;

        Vector2 endPosition = anchoredPosition + new Vector2(0f, 90f);
        DOTween.Sequence()
            .SetTarget(gameObject)
            .Join(rectTransform.DOAnchorPos(endPosition, 0.75f).SetEase(Ease.OutCubic))
            .Join(rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
            .Join(canvasGroup.DOFade(0f, 0.75f).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}
