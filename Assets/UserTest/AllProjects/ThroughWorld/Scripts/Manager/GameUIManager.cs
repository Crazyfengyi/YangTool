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
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// UI管理器
/// </summary>
public class GameUIManager : MonoSingleton<GameUIManager>
{
    public GameObject tipNodePrefab;
    public Transform tipsParent;

    private List<GameObject> tipsList = new List<GameObject>();
    private List<TipData> tipDatas = new List<TipData>();

    private float interval = 0.1f;
    private float timer = 0;
    private int currentCount = 0;

    private const float ANI_TIME_PRE = 0.2f;
    private const float ANI_TIME_WAIT = 0.5f;
    private const float ANI_TIME_POST = 0.3f;
    private const float ANI_TIME_MOVE = 1f;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddTipsShow("测试提示！");
        }

        timer += Time.deltaTime;
        if (timer >= interval && tipDatas.Count > 0)
        {
            timer = 0;
            TipShow(tipDatas[0]);
            tipDatas.RemoveAt(0);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void AddTipsShow(string str)
    {
        tipDatas.Add(new TipData(str));
    }
    public void TipShow(TipData data)
    {
        currentCount++;
        GameObject obj = GameObject.Instantiate(tipNodePrefab, tipsParent);
        GameObject tip = obj.transform.GetChild(0).gameObject;
        tip.GetComponentInChildren<TMP_Text>(true).text = data.tipStr;

        DOTween.Kill(tip, true);
        tip.SetActive(true);
        var defaultPos = tip.transform.localPosition;
        var startPos = defaultPos;
        startPos.x += tip.GetComponent<RectTransform>().rect.width;
        tip.transform.localPosition = startPos;
        DOTween.Sequence()
            .Append(tip.transform.DOLocalMoveX(defaultPos.x, ANI_TIME_PRE).SetEase(Ease.InCubic))
            .AppendInterval(ANI_TIME_WAIT * tipDatas.Count + Mathf.Max(0, currentCount - 1) * ANI_TIME_MOVE)
            .Append(tip.transform.DOLocalMoveX(startPos.x, ANI_TIME_POST))
            .Join(tip.GetComponent<CanvasGroup>().DOFade(0, ANI_TIME_POST))
            .Append(obj.GetComponent<LayoutElement>().DOPreferredSize(new Vector2(0, 0), ANI_TIME_MOVE))
            .OnComplete(() =>
            {
                currentCount--;
                tip.SetActive(false);
                tip.transform.localPosition = defaultPos;
                tip.GetComponent<CanvasGroup>().alpha = 1;
                Destroy(obj);
            })
            .SetTarget(tip);
    }
    public void UpdatePlayerBar()
    {

    }
}
/// <summary>
/// 提示信息
/// </summary>
public class TipData
{
    public string tipStr;
    public TipData(string str)
    {
        tipStr = str;
    }
}