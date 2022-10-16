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
using YangTools.ObjectPool;
using YangTools.Extend;
using YangTools.Log;
/// <summary>
/// UI管理器
/// </summary>
public class GameUIManager : MonoSingleton<GameUIManager>
{
    #region 提示
    public GameObject tipNodePrefab;
    public Transform tipsParent;

    private List<TipData> tipDatas = new List<TipData>();
    public int maxTipsShowCount = 10;

    private float interval = 0f;
    private float timer = 0;
    private int currentCount = 0;

    private const float ANI_TIME_PRE = 0.2f;
    private const float ANI_TIME_WAIT = 0.5f;
    private const float ANI_TIME_POST = 0.3f;
    private const float ANI_TIME_MOVE = 0.2f;
    #endregion

    #region 飘分
    public GameObject scorePrefab;//飘字
    public Transform scoreParent;
    #endregion

    public void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        Debuger.IsForceLog = true;
    }
    public void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //AddTipsShow("测试提示！");
            //AddScoreShow(Vector3.zero, "-100");
        }

        timer += Time.deltaTime;
        if (timer >= interval && tipDatas.Count > 0 && currentCount <= maxTipsShowCount)
        {
            timer = 0;
            TipShow(tipDatas[0]);
            tipDatas.RemoveAt(0);
        }
    }

    #region 提示
    public void AddTipsShow(string str)
    {
        tipDatas.Add(new TipData(str));
    }
    public void TipShow(TipData data)
    {
        currentCount++;

        TipObjectPoolItem poolItem = YangObjectPool.Get<TipObjectPoolItem>();
        GameObject obj = poolItem.obj;
        obj.transform.SetParent(tipsParent);
        obj.transform.SetAsLastSibling();
        tipsParent.gameObject.RefreshAllContentSizeFitter();

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
                YangObjectPool.Recycle(poolItem);
                obj.GetComponent<LayoutElement>().preferredHeight = 100;
            })
            .SetTarget(tip);
    }
    #endregion

    #region 飘分
    public void AddScoreShow(Vector3 worldPos, string text, Color color = default)
    {
        ScoreData scoreData = new ScoreData();
        scoreData.worldPos = worldPos;
        scoreData.worldText = text;
        scoreData.textColor = color == default ? Color.white : color;
        StartScoreUIShow(scoreData);
    }
    public void StartScoreUIShow(ScoreData scoreData)
    {
        //飘分对象
        Vector3 pos = WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, null, scoreData.worldPos, scoreParent);
        ScoreObjectPoolItem poolItem = YangObjectPool.Get<ScoreObjectPoolItem>();
        GameObject score = poolItem.obj;
        score.transform.SetParent(scoreParent);
        score.transform.SetAsLastSibling();
        score.transform.localPosition = pos;
        //文字设置
        TMP_Text scoreText = score.transform.GetChild(0).gameObject.GetComponentInChildren<TMP_Text>(true);
        scoreText.text = scoreData.worldText;
        scoreText.color = scoreData.textColor;

        DOTween.Kill(score, true);
        var defaultPos = scoreText.transform.localPosition;
        var startPos = defaultPos;
        scoreText.transform.localPosition = startPos;

        DOTween.Sequence()
            .Append(scoreText.transform.DOLocalMoveY(defaultPos.y + 120, 0.6f).SetEase(Ease.InCubic))
            .OnUpdate(() =>
            {
                Vector3 pos = WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, null, scoreData.worldPos, scoreParent);
                score.transform.localPosition = pos;
            })
            .OnComplete(() =>
            {
                scoreText.transform.localPosition = defaultPos;
                YangObjectPool.Recycle(poolItem);
            })
            .SetTarget(score);
    }
    /// <summary>
    /// 世界坐标转UI的局部坐标
    /// </summary>
    /// <param name="WorldCamara">场景相机</param>
    /// <param name="UICamara">UI相机</param>
    /// <param name="worldPos">世界坐标</param>
    /// <param name="targetParent">目标节点</param>
    public Vector3 WorldPositionToUILocalPosition(Camera WorldCamara, Camera UICamara, Vector3 worldPos, Transform targetParent)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(WorldCamara, worldPos);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent.GetComponent<RectTransform>(), screenPoint, UICamara, out Vector2 localPoint))
        {
            return localPoint;
        }
        return default;
    }
    #endregion
}
/// <summary>
/// 对象池提示对象
/// </summary>
public class TipObjectPoolItem : IPoolItem<TipObjectPoolItem>
{
    public bool IsInPool { get; set; }
    public GameObject obj;
    public TipObjectPoolItem Create()
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad("TipsNode"));
        TipObjectPoolItem temp = new TipObjectPoolItem();
        temp.obj = tempObj;
        return temp;
    }
    public void OnGet(TipObjectPoolItem t)
    {
        t.obj.DefualtGameObjectOnGet();
    }
    public void OnRecycle(TipObjectPoolItem t)
    {
        t.obj.DefualtGameObjectRecycle();
    }
    public void OnDestroy(TipObjectPoolItem t)
    {
        t.obj.DefualtGameObjectDestory();
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


/// <summary>
/// 对象池飘分对象
/// </summary>
public class ScoreObjectPoolItem : IPoolItem<ScoreObjectPoolItem>
{
    public bool IsInPool { get; set; }
    public GameObject obj;
    public ScoreObjectPoolItem Create()
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad("WordUI"));
        ScoreObjectPoolItem temp = new ScoreObjectPoolItem();
        temp.obj = tempObj;
        return temp;
    }
    public void OnGet(ScoreObjectPoolItem t)
    {
        t.obj.DefualtGameObjectOnGet();
    }
    public void OnRecycle(ScoreObjectPoolItem t)
    {
        t.obj.DefualtGameObjectRecycle();
    }
    public void OnDestroy(ScoreObjectPoolItem t)
    {
        t.obj.DefualtGameObjectDestory();
    }
}
/// <summary>
/// 飘分数据
/// </summary>
public class ScoreData
{
    public Vector3 worldPos;
    public string worldText;
    public Color textColor;
}