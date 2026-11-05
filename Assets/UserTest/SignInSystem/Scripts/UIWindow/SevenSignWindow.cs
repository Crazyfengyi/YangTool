using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SignInSystem;

/// <summary>
/// 签到窗口
/// </summary>
public class SevenSignWindow : MonoBehaviour
{
    public Button closeBtn;
    public List<ItemUI_DailySign> signUIList;
    public GameObject btnNode;
    public Button getBtn;
    public Button adsGetBtn;

    /// <summary>
    /// 当前订阅的签到管理器
    /// </summary>
    private SignInManager signInManager;

    /// <summary>
    /// 收集签到项
    /// </summary>
    private void Awake()
    {
        if (signUIList == null || signUIList.Count == 0)
        {
            signUIList = new List<ItemUI_DailySign>(GetComponentsInChildren<ItemUI_DailySign>(true));
        }
    }

    /// <summary>
    /// 绑定窗口按钮事件
    /// </summary>
    private void Start()
    {
        closeBtn?.onClick.AddListener(OnCloseBtn);
        getBtn?.onClick.AddListener(SignDay);
        adsGetBtn?.onClick.AddListener(AdsSignDay);
    }

    /// <summary>
    /// 窗口启用时刷新显示
    /// </summary>
    private void OnEnable()
    {
        signInManager = SignInManager.Instance;
        signInManager.SignInDataChanged += OnSignInDataChanged;
        Refresh();
    }

    /// <summary>
    /// 窗口禁用时取消数据变更订阅
    /// </summary>
    private void OnDisable()
    {
        if (signInManager != null)
        {
            signInManager.SignInDataChanged -= OnSignInDataChanged;
        }
    }

    /// <summary>
    /// 签到数据变更后刷新窗口
    /// </summary>
    private void OnSignInDataChanged()
    {
        Refresh();
    }

    /// <summary>
    /// 关闭按钮
    /// </summary>
    public void OnCloseBtn()
    {
        gameObject.SetActive(false);
    }
    /// <summary>
    /// 普通签到
    /// </summary>
    public void SignDay()
    {
        if (SignInManager.Instance.ClaimCurrentDay())
        {
            Debug.Log("签到成功");
            Refresh();
        }
        else
        {
            Debug.Log("今天已经签到或当前没有可领取奖励");
        }
    }

    /// <summary>
    /// 双倍签到入口
    /// </summary>
    public void AdsSignDay()
    {
        if (SignInManager.Instance.ClaimCurrentDay(2))
        {
            Debug.Log("双倍签到成功");
            Refresh();
        }
        else
        {
            Debug.Log("今天已经签到或当前没有可领取奖励");
        }
    }

    /// <summary>
    /// 刷新签到窗口显示
    /// </summary>
    public void Refresh()
    {
        if (SignInManager.Instance.Days == null)
        {
            return;
        }

        int currentDayIndex = SignInManager.Instance.GetCurrentDayIndex();
        bool canSignToday = currentDayIndex >= 0 && !SignInManager.Instance.HasClaimedToday();

        for (int i = 0; i < signUIList.Count; i++)
        {
            if (i < SignInManager.Instance.Days.Count)
            {
                signUIList[i].RefreshItemUI(SignInManager.Instance.Days[i],
                    SignInManager.Instance.GetState(i), i);
            }

            signUIList[i].SetSignAnimation(canSignToday && i == currentDayIndex);
        }

        if (btnNode != null)
        {
            btnNode.SetActive(canSignToday);
        }
    }
}
