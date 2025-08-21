using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using YangTools.Scripts.Core.YangExtend;

public class GuideHandScript : MonoBehaviour
{
    public RectTransform effect;
    public RectTransform hand;

    private Image handImage;
    private void Start()
    {
        handImage = hand.GetComponent<Image>();
    }

    public void Hide()
    {
        DOTween.Kill(gameObject, true);
        effect.gameObject.SetActive(false);
        hand.gameObject.SetActive(false);
    }
    public void PlayClickAni(Vector3 worldPos)
    {
        //Vector3 localPos = YangExtend.WorldPositionToUILocalPosition(UIWindowMgr.Instance.uiCamera, UIWindowMgr.Instance.uiCamera, worldPos, transform.parent);
        //transform.localPosition = localPos;
        effect.gameObject.SetActive(true);
        hand.gameObject.SetActive(true);

        Vector3 startPos = hand.localPosition;
        DOTween.Kill(gameObject, true);
        DOTween.Sequence()
            .Append(effect.DOScale(Vector3.one * 0.8f, 0.5f).SetLoops(-1, LoopType.Yoyo))
            .Join(hand.DOLocalMove(startPos + new Vector3(0, -50, 0), 0.5f).SetLoops(-1, LoopType.Yoyo))
            .SetTarget(gameObject)
            .OnComplete(() =>
            {
                effect.localScale = Vector3.one;
                hand.localPosition = startPos;
                effect.gameObject.SetActive(false);
                hand.gameObject.SetActive(false); 
            });
    }

    public void PlayTouchAni(Vector3 startWorldPos,Vector3 toWorldPos)
    {
        // Vector3 startLocalPos = YangExtend.WorldPositionToUILocalPosition(UIWindowMgr.Instance.uiCamera, UIWindowMgr.Instance.uiCamera, startWorldPos, transform.parent);
        // transform.localPosition = startLocalPos;
        effect.gameObject.SetActive(false);
        hand.gameObject.SetActive(true);

        //Vector3 toLocalPos = YangExtend.WorldPositionToUILocalPosition(UIWindowMgr.Instance.uiCamera, UIWindowMgr.Instance.uiCamera, toWorldPos, hand.parent);
        Vector3 toLocalPos = Vector3.zero;
        Vector3 startPos = hand.localPosition;
        DOTween.Kill(gameObject, true);
        DOTween.Sequence()
            .Append(hand.DOLocalMove(toLocalPos, 1f))
            .AppendInterval(0.2f)
            .Append(handImage.DOFade(0f, 0.3f))
            .SetLoops(-1, LoopType.Restart)
            .SetTarget(gameObject)
            .OnComplete(() =>
            {
                var old =  handImage.color;
                old.a = 1f;
                handImage.color = old;
                hand.localPosition = startPos;
                effect.gameObject.SetActive(false);
                hand.gameObject.SetActive(false); 
            });
    }
}
