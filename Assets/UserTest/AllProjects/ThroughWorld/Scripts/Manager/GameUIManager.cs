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
using Sirenix.OdinInspector;
using YangTools.UGUI;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// UI管理器
/// </summary>
public class GameUIManager : MonoSingleton<GameUIManager>
{
    #region UI
    public Button returnBtn;
    public Canvas uiCanvas;

    public GameObject seceneLoading;
    [SerializeField]
    private Camera uiCamera;
    public Camera UICamera => uiCamera;

    #endregion

    #region 提示
    public GameObject tipNodePrefab;
    public Transform tipsParent;

    private List<TipData> tipDatas = new List<TipData>();
    public int maxTipsShowCount = 10;

    private float interval = 0f;
    private float timer = 0;
    private int currentCount = 0;

    private const float ANI_TIME_PRE = 0.36f;
    private const float ANI_TIME_WAIT = 0.66f;
    private const float ANI_TIME_POST = 0.36f;
    private const float ANI_TIME_MOVE = 0.36f;
    #endregion

    #region 飘分
    public GameObject scorePrefab;//飘字
    public Transform scoreParent;
    #endregion

    #region 血条
    public Transform hpBarParent;
    [HideInEditorMode]
    [ShowInInspector]
    private static List<HPBarObjectPoolItem> allHPBar = new List<HPBarObjectPoolItem>();
    #endregion

    #region 生命周期
    protected override void Awake()
    {
        base.Awake();
        Debuger.IsForceLog = true;

        returnBtn.gameObject.SetActive(false);
        returnBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SceneLoad("GameMain");
        });
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddTipsShow("测试提示！", TipsReDoType.None);
            //AddScoreShow(Vector3.zero, "-100");
            //UICommonTool.Instance.ShowTip("测试....");
            //UICommonTool.Instance.SetLoadingShow(true);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            //UICommonTool.Instance.SetLoadingShow(false);
            UICommonTool.Instance.ShowConfirmPanel("测试", okBtnCallBack: () => { }, cancelBtnCallBack: () => { },
            isCountDownSelect: true);

        }

        timer += Time.deltaTime;
        if (timer >= interval && tipDatas.Count > 0 && currentCount <= maxTipsShowCount)
        {
            timer = 0;
            TipShow(tipDatas[0]);
            tipDatas.RemoveAt(0);
        }

        for (int i = 0; i < allHPBar.Count; i++)
        {
            if (allHPBar[i] != null)
            {
                allHPBar[i]?.UpdatePos();
            }
        }
    }
    public void LateUpdate()
    {
        for (int i = 0; i < allHPBar.Count; i++)
        {
            if (allHPBar[i] != null)
            {
                allHPBar[i]?.UpdatePos();
            }
        }
    }

    /// <summary>
    /// 场景切换
    /// </summary>
    public void OnSceneChangeStart(string sceneName)
    {
        returnBtn.gameObject.SetActive(sceneName != "GameMain");
    }
    #endregion

    #region 提示
    /// <summary>
    ///  添加提示
    /// </summary>
    /// <param name="str">提示文字</param>
    /// <param name="tipsReDoType">重复处理(上一个提示文字相同时处理情况)</param>
    public void AddTipsShow(string str, TipsReDoType tipsReDoType = TipsReDoType.None)
    {
        //舍弃和上一个重复的提示
        if (tipsReDoType == TipsReDoType.Discard)
        {
            if (tipsNodeList.Count > 0 && tipsNodeList[0].GetComponentInChildren<TMP_Text>(true).text == str)
            {
                return;
            }
        }

        tipDatas.Add(new TipData(str));
    }

    private List<GameObject> tipsNodeList = new List<GameObject>();
    private void TipShow(TipData data)
    {
        currentCount++;

        TipObjectPoolItem poolItem = YangObjectPool.Get<TipObjectPoolItem>();
        GameObject obj = poolItem.obj;
        obj.transform.SetParent(tipsParent);
        obj.transform.SetAsLastSibling();
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        tipsParent.gameObject.RefreshAllContentSizeFitter();

        GameObject tipObj = obj.transform.GetChild(0).gameObject;
        tipObj.GetComponentInChildren<TMP_Text>(true).text = data.tipStr;
        tipsNodeList.Add(tipObj);

        DOTween.Kill(tipObj, true);
        tipObj.SetActive(true);
        var defaultPos = tipObj.transform.localPosition;
        var startPos = defaultPos;
        startPos.x += tipObj.GetComponent<RectTransform>().rect.width;
        tipObj.transform.localPosition = startPos;
        DOTween.Sequence()
            .Append(tipObj.transform.DOLocalMoveX(defaultPos.x, ANI_TIME_PRE).SetEase(Ease.InCubic))
            .AppendInterval(ANI_TIME_WAIT * tipDatas.Count + Mathf.Max(0, currentCount - 1) * ANI_TIME_MOVE)
            .Append(tipObj.transform.DOLocalMoveX(startPos.x, ANI_TIME_POST))
            .Join(tipObj.GetComponent<CanvasGroup>().DOFade(0, ANI_TIME_POST))
            .Append(obj.GetComponent<LayoutElement>().DOPreferredSize(new Vector2(0, 0), ANI_TIME_MOVE))
            .OnComplete(() =>
            {
                currentCount--;
                tipsNodeList.Remove(tipObj);
                tipObj.SetActive(false);
                tipObj.transform.localPosition = defaultPos;
                tipObj.GetComponent<CanvasGroup>().alpha = 1;
                YangObjectPool.Recycle(poolItem);
                obj.GetComponent<LayoutElement>().preferredHeight = 100;
            })
            .SetTarget(tipObj);
    }
    #endregion

    #region 飘分
    /// <summary>
    /// 添加飘分
    /// </summary>
    /// <param name="worldPos">世界坐标</param>
    /// <param name="text">文字</param>
    /// <param name="color">颜色</param>
    public void AddScoreShow(Vector3 worldPos, string text, Color color = default, ScoreAniType scoreAniType = ScoreAniType.Ani2)
    {
        ScoreData scoreData = new ScoreData();
        scoreData.worldPos = worldPos;
        scoreData.worldText = text;
        scoreData.textColor = color == default ? Color.white : color;
        scoreData.scoreAniType = scoreAniType;
        StartScoreUIShow(scoreData);
    }
    private void StartScoreUIShow(ScoreData scoreData)
    {
        //飘分对象
        Vector3 pos = YangExtend.WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, GameUIManager.Instance.uiCanvas.worldCamera, scoreData.worldPos, scoreParent);
        ScoreObjectPoolItem poolItem = YangObjectPool.Get<ScoreObjectPoolItem>();
        GameObject score = poolItem.obj;
        score.transform.SetAsLastSibling();
        score.transform.localPosition = pos;
        //文字设置
        TMP_Text scoreText = score.transform.GetChild(0).gameObject.GetComponentInChildren<TMP_Text>(true);
        scoreText.text = scoreData.worldText;
        scoreText.color = scoreData.textColor;

        DOTween.Kill(score, true);
        var defaultPos = scoreText.transform.localPosition;
        scoreText.transform.localPosition = defaultPos + new Vector3(Random.Range(-12f, 12f), Random.Range(-6f, 6f), 0);
        CanvasGroup canvasGroup = scoreText.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        scoreText.transform.localScale = Vector3.one;

        switch (scoreData.scoreAniType)
        {
            case ScoreAniType.Ani2:
                {
                    DOTween.Sequence()
                        .Append(scoreText.transform.DOLocalMoveY(defaultPos.y + 120, 1f))
                        .Join(canvasGroup.DOFade(1f, 1f))
                        .AppendInterval(0.2f)
                        .Join(canvasGroup.DOFade(0.3f, 0.2f))
                        .OnUpdate(() =>
                        {
                            Vector3 pos = YangExtend.WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, GameUIManager.Instance.uiCanvas.worldCamera, scoreData.worldPos, scoreParent);
                            score.transform.localPosition = pos;
                        })
                        .OnComplete(() =>
                        {
                            scoreText.transform.localPosition = defaultPos;
                            scoreText.transform.localScale = Vector3.one;
                            YangObjectPool.Recycle(poolItem);
                        })
                        .SetTarget(score)
                        .SetEase(Ease.Linear);

                }
                break;
            case ScoreAniType.Ani3:
                {
                    canvasGroup.alpha = 0;
                    DOTween.Sequence()
                        .Append(scoreText.transform.DOLocalMoveY(defaultPos.y + 120, 0.25f))
                        .Join(canvasGroup.DOFade(1, 0.2f))
                        .AppendInterval(0.5f)
                        .Append(scoreText.transform.DOLocalMoveY(defaultPos.y + 160, 0.15f))
                        .Append(canvasGroup.DOFade(0, 0.15f))
                        .OnUpdate(() =>
                        {
                            Vector3 pos = YangExtend.WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, GameUIManager.Instance.uiCanvas.worldCamera, scoreData.worldPos, scoreParent);
                            score.transform.localPosition = pos;
                        })
                        .OnComplete(() =>
                        {
                            scoreText.transform.localPosition = defaultPos;
                            scoreText.transform.localScale = Vector3.one;
                            YangObjectPool.Recycle(poolItem);
                        })
                        .SetTarget(score)
                        .SetEase(Ease.Linear);
                }
                break;
            case ScoreAniType.Ani1:
            default:
                {
                    DOTween.Sequence()
                        .Append(scoreText.transform.DOScale(0.8f, 0.2f))
                        .Append(scoreText.transform.DOLocalMoveY(defaultPos.y + 6, 0.2f))
                        .Join(scoreText.transform.DOScale(1f, 0.2f))
                        .Append(scoreText.transform.DOScale(0.6f, 0.2f).SetEase(Ease.InCubic))
                        .Append(canvasGroup.DOFade(0, 0.2f))
                        .OnUpdate(() =>
                        {
                            Vector3 pos = YangExtend.WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, GameUIManager.Instance.uiCanvas.worldCamera, scoreData.worldPos, scoreParent);
                            score.transform.localPosition = pos;
                        })
                        .OnComplete(() =>
                        {
                            scoreText.transform.localPosition = defaultPos;
                            scoreText.transform.localScale = Vector3.one;
                            YangObjectPool.Recycle(poolItem);
                        })
                        .SetTarget(score);
                }
                break;
        }
    }
    #endregion

    #region 血条
    /// <summary>
    /// 添加血条
    /// </summary>
    /// <param name="_target">跟随目标</param>
    /// <param name="text">文字</param>
    /// <param name="color">颜色</param>
    public HPBarObjectPoolItem GetHPBar(Transform _target, string text, Color color = default)
    {
        HPBarData data = new HPBarData();
        data.target = _target;
        data.text = text;
        data.textColor = color == default ? Color.white : color;
        data.hpValue = 1f;
        data.mpValue = 1f;

        return CreateHPBar(data);
    }
    /// <summary>
    /// 创建血条
    /// </summary>
    private HPBarObjectPoolItem CreateHPBar(HPBarData hpBarData)
    {
        Vector3 pos = YangExtend.WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, GameUIManager.Instance.uiCanvas.worldCamera, hpBarData.target.position, hpBarParent);
        HPBarObjectPoolItem poolItem = YangObjectPool.Get<HPBarObjectPoolItem>();
        poolItem.InitData(hpBarData);
        GameObject hpBar = poolItem.obj;
        hpBar.transform.SetAsLastSibling();
        hpBar.transform.localPosition = pos;
        allHPBar.Add(poolItem);

        return poolItem;
    }
    /// <summary>
    /// 回收血条
    /// </summary>
    public void RecycleHPBar(HPBarObjectPoolItem poolItem)
    {
        allHPBar.Remove(poolItem);
        YangObjectPool.Recycle(poolItem);
    }
    #endregion

    #region 场景加载
    /// <summary>
    /// 开始场景加载
    /// </summary>
    public void StartSeceneLoading()
    {
        seceneLoading.gameObject.SetActive(true);
    }
    /// <summary>
    /// 结束场景加载
    /// </summary>
    public void EndSeceneLoading()
    {
        seceneLoading.gameObject.SetActive(false);
    }
    #endregion
}

#region 提示
/// <summary>
/// 对象池提示对象
/// </summary>
public class TipObjectPoolItem : IPoolItem<TipObjectPoolItem>
{
    public string PoolKey { get; set; }
    public bool IsInPool { get; set; }
    public GameObject obj;
    public TipObjectPoolItem()
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad("UI/UICommon/TipsNode"), Vector3.zero, Quaternion.identity);
        obj = tempObj;
    }
    public TipObjectPoolItem(string arg)
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad("UI/UICommon/TipsNode"), Vector3.zero, Quaternion.identity);
        obj = tempObj;
    }
    public void OnGet()
    {
        obj.DefualtGameObjectOnGet(GameUIManager.Instance.tipsParent.transform);
    }
    public void OnRecycle()
    {
        obj.DefualtGameObjectRecycle();
    }
    public void OnDestroy()
    {
        obj.DefualtGameObjectDestory();
    }
}
/// <summary>
/// 提示信息
/// </summary>
public class TipData
{
    /// <summary>
    /// 提示文字
    /// </summary>
    public string tipStr;
    public TipData(string _str)
    {
        tipStr = _str;
    }
}

/// <summary>
/// 重复处理
/// </summary>
public enum TipsReDoType
{
    /// <summary>
    /// 默认(允许重复)
    /// </summary>
    None,
    /// <summary>
    /// 舍弃
    /// </summary>
    Discard,
}
#endregion

#region 飘分 
/// <summary>
/// 对象池飘分对象
/// </summary>
public class ScoreObjectPoolItem : IPoolItem<ScoreObjectPoolItem>
{
    public string PoolKey { get; set; }
    public bool IsInPool { get; set; }

    public GameObject obj;
    public ScoreObjectPoolItem()
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad("UI/UICommon/WordUI"), GameUIManager.Instance.scoreParent);
        obj = tempObj;
    }
    public void OnGet()
    {
        obj.DefualtGameObjectOnGet(GameUIManager.Instance.scoreParent);

    }
    public void OnRecycle()
    {
        obj.DefualtGameObjectRecycle();
    }
    public void OnDestroy()
    {
        obj.DefualtGameObjectDestory();
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
    public ScoreAniType scoreAniType;
}
/// <summary>
/// 飘分动画
/// </summary>
public enum ScoreAniType
{
    None,
    /// <summary>
    /// 缩放向上--抄蛋壳特工队
    /// </summary>
    Ani1,
    /// <summary>
    /// 向上飘
    /// </summary>
    Ani2,
    /// <summary>
    /// 向上飘--中间停顿
    /// </summary>
    Ani3,
}
#endregion

#region 血条
/// <summary>
/// 对象池血条对象
/// </summary>
public class HPBarObjectPoolItem : IPoolItem<HPBarObjectPoolItem>
{
    public string PoolKey { get; set; }
    public bool IsInPool { get; set; }
    public GameObject obj;
    public TMP_Text text;
    public Slider hpSlider;
    public Slider mpSlider;

    public Vector3 targetPos;

    private HPBarData hpBarData;
    public HPBarObjectPoolItem()
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad("UI/UICommon/HPBarUI"), GameUIManager.Instance.hpBarParent);
        obj = tempObj;
        //设置显示
        text = obj.transform.GetChild(0).gameObject.GetComponentInChildren<TMP_Text>(true);
        hpSlider = obj.transform.GetChild(1).gameObject.GetComponentInChildren<Slider>(true);
        mpSlider = obj.transform.GetChild(2).gameObject.GetComponentInChildren<Slider>(true);
    }
    public void InitData(HPBarData _hpBarData)
    {
        hpBarData = _hpBarData;
        text.text = hpBarData.text;
        hpSlider.value = hpBarData.hpValue;
        mpSlider.value = hpBarData.mpValue;
    }
    public void UpdatePos()
    {
        targetPos = YangExtend.WorldPositionToUILocalPosition(CameraManager.Instance.PlayerCamera, GameUIManager.Instance.uiCanvas.worldCamera, hpBarData.target.position, GameUIManager.Instance.hpBarParent);
        obj.transform.localPosition = Vector3.Lerp(obj.transform.localPosition, targetPos, Time.deltaTime * 10);
    }
    /// <summary>
    /// 控制显隐
    /// </summary>
    public void ToogleHPBar(bool isShow)
    {
        obj.SetActive(isShow);
    }
    /// <summary>
    /// 更新数据显示 
    /// </summary>
    public void UpdateData(GameActor gameActor)
    {
        if (gameActor is RoleBase role)
        {
            hpBarData.hpValue = role.HealthControl.GetPercentHp();
            text.text = hpBarData.text + $"  血量:{role.HealthControl.GetCurrentHp()}";
            hpSlider.value = hpBarData.hpValue;
            mpSlider.value = hpBarData.mpValue;
        }
    }
    public void OnGet()
    {
        obj.DefualtGameObjectOnGet(GameUIManager.Instance.hpBarParent);
    }
    public void OnRecycle()
    {
        obj.DefualtGameObjectRecycle();
    }
    public void OnDestroy()
    {
        obj.DefualtGameObjectDestory();
    }
}
/// <summary>
/// 血条数据
/// </summary>
public class HPBarData
{
    public Transform target;
    public string text;
    public Color textColor;
    public float hpValue;
    public float mpValue;
}
#endregion