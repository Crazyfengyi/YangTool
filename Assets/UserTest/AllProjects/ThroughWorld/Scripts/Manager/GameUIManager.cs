/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-14 
*/
using UnityEngine;
using System.Collections;
using YangTools;
using DG.Tweening;
using TMPro;
/// <summary>
/// UI管理器
/// </summary>
public class GameUIManager : MonoSingleton<GameUIManager>
{
    public GameObject tip;

    public void Start()
    {
        tip.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TipShow("测试提示！");
        }
    }
    public void TipShow(string str)
    {
        tip.GetComponentInChildren<TMP_Text>(true).text = str;

        DOTween.Kill(tip, true);
        tip.SetActive(true);
        var defaultPos = tip.transform.localPosition;
        var startPos = defaultPos;
        startPos.x += tip.GetComponent<RectTransform>().rect.width;
        tip.transform.localPosition = startPos;
        DOTween.Sequence()
            .Append(tip.transform.DOLocalMoveX(defaultPos.x, 0.2f).SetEase(Ease.InCubic))
            .AppendInterval(0.5f)
            .Append(tip.transform.DOLocalMoveY(defaultPos.y + 60, 0.3f))
            .Join(tip.GetComponent<CanvasGroup>().DOFade(0, 0.3f))
            .OnComplete(() =>
            {
                tip.SetActive(false);
                tip.transform.localPosition = defaultPos;
                tip.GetComponent<CanvasGroup>().alpha = 1;
            })
            .SetTarget(tip);
    }

    public void UpdatePlayerBar()
    {

    }
}