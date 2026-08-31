using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 独立签到系统管理器
/// 使用 PlayerPrefs 保存签到进度
/// </summary>
public sealed class SignInManager : MonoBehaviour
{
    /// <summary>
    /// 存储已签到次数的键名
    /// </summary>
    private const string SignInCountKey = "SignInSystem.SignInCountKey3";
    /// <summary>
    /// 存储上次签到日期的键名
    /// </summary>
    private const string LastSignInDateKey = "SignInSystem.LastSignInDateKey3";
    /// <summary>
    /// 存储签到位掩码的键名
    /// </summary>
    private const string SignInMaskKey = "SignInSystem.SignInMaskKey3";

    // 签到管理器单例实例
    private static SignInManager instance;

    // 签到配置资产
    [SerializeField] private SignInConfig config;

    // 运行时使用的签到数据
    private List<SignInDayData> runtimeDays;

    /// <summary>
    /// 外部项目注入的签到数据
    /// </summary>
    private List<SignInDayData> injectedDays;

    /// <summary>
    /// 当前签到管理器实例
    /// </summary>
    public static SignInManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindFirstObjectByType<SignInManager>();
            if (instance == null)
            {
                GameObject managerObject = new GameObject("SignInManager");
                instance = managerObject.AddComponent<SignInManager>();
            }

            return instance;
        }
    }

    /// <summary>
    /// 签到配置列表
    /// </summary>
    public IReadOnlyList<SignInDayData> Days => runtimeDays;

    /// <summary>
    /// 签到成功事件 参数为天数 奖励倍率和奖励道具信息
    /// </summary>
    public event Action<int, int, IReadOnlyList<SignInReward>> SignedIn;

    /// <summary>
    /// 签到数据变更事件
    /// </summary>
    public event Action SignInDataChanged;

    /// <summary>
    /// 初始化单例并加载签到配置
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfig();
    }

    /// <summary>
    /// 获取指定日期的签到状态
    /// </summary>
    public SignInDayState GetState(int dayIndex)
    {
        if (!IsValidDay(dayIndex))
        {
            return SignInDayState.Locked;
        }

        if (IsClaimed(dayIndex))
        {
            return SignInDayState.Claimed;
        }

        return dayIndex == GetClaimedCount() ? SignInDayState.Available : SignInDayState.Locked;
    }

    /// <summary>
    /// 获取当前可领取的签到序号
    /// </summary>
    public int GetCurrentDayIndex()
    {
        int dayIndex = GetClaimedCount();
        return IsValidDay(dayIndex) ? dayIndex : -1;
    }

    /// <summary>
    /// 获取指定天数的奖励列表
    /// </summary>
    public IReadOnlyList<SignInReward> GetRewards(int dayIndex)
    {
        return IsValidDay(dayIndex) && runtimeDays[dayIndex].rewards != null
            ? runtimeDays[dayIndex].rewards
            : Array.Empty<SignInReward>();
    }

    /// <summary>
    /// 注入外部项目提供的签到数据
    /// 传入空数据时恢复配置资产数据
    /// </summary>
    public bool InjectSignInData(IReadOnlyList<SignInDayData> days)
    {
        injectedDays = days != null && days.Count > 0 ? CloneDays(days) : null;
        LoadConfig();
        SignInDataChanged?.Invoke();
        return injectedDays != null;
    }

    /// <summary>
    /// 领取指定天数奖励
    /// </summary>
    public bool ClaimDay(int dayIndex, int rewardMultiplier = 1)
    {
        if (GetState(dayIndex) != SignInDayState.Available || HasClaimedToday())
        {
            return false;
        }

        rewardMultiplier = Mathf.Max(1, rewardMultiplier);
        int claimedCount = GetClaimedCount() + 1;
        int claimedMask = PlayerPrefs.GetInt(SignInMaskKey, 0);
        claimedMask |= 1 << dayIndex;
        PlayerPrefs.SetInt(SignInCountKey, claimedCount);
        PlayerPrefs.SetInt(SignInMaskKey, claimedMask);
        PlayerPrefs.SetString(LastSignInDateKey, DateTime.Now.ToString("yyyyMMdd"));
        PlayerPrefs.Save();
        SignedIn?.Invoke(dayIndex, rewardMultiplier, GetRewards(dayIndex));
        return true;
    }

    /// <summary>
    /// 领取当前可用签到奖励
    /// </summary>
    public bool ClaimCurrentDay(int rewardMultiplier = 1)
    {
        return ClaimDay(GetCurrentDayIndex(), rewardMultiplier);
    }

    /// <summary>
    /// 判断今天是否已经签到
    /// </summary>
    public bool HasClaimedToday()
    {
        return string.Equals(PlayerPrefs.GetString(LastSignInDateKey, string.Empty),
            DateTime.Now.ToString("yyyyMMdd"), StringComparison.Ordinal);
    }

    /// <summary>
    /// 清除本地签到进度
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(SignInCountKey);
        PlayerPrefs.DeleteKey(LastSignInDateKey);
        PlayerPrefs.DeleteKey(SignInMaskKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 加载资源配置
    /// 配置缺失时生成默认七日配置
    /// </summary>
    private void LoadConfig()
    {
        if (injectedDays != null && injectedDays.Count > 0)
        {
            runtimeDays = injectedDays;
            return;
        }

        config = config != null ? config : Resources.Load<SignInConfig>("SignInConfig");
        runtimeDays = config != null && config.days != null && config.days.Count > 0
            ? config.days
            : CreateDefaultDays();
    }

    /// <summary>
    /// 读取已签到天数
    /// </summary>
    private int GetClaimedCount()
    {
        int dayCount = runtimeDays != null ? runtimeDays.Count : 0;
        return Mathf.Clamp(PlayerPrefs.GetInt(SignInCountKey, 0), 0, dayCount);
    }

    /// <summary>
    /// 判断指定天数是否已领取
    /// </summary>
    private bool IsClaimed(int dayIndex)
    {
        return (PlayerPrefs.GetInt(SignInMaskKey, 0) & (1 << dayIndex)) != 0;
    }

    /// <summary>
    /// 判断天数索引是否有效
    /// </summary>
    private bool IsValidDay(int dayIndex)
    {
        return runtimeDays != null && dayIndex >= 0 && dayIndex < runtimeDays.Count;
    }

    /// <summary>
    /// 创建默认七日签到配置
    /// </summary>
    private static List<SignInDayData> CreateDefaultDays()
    {
        List<SignInDayData> days = new List<SignInDayData>();
        for (int i = 0; i < 7; i++)
        {
            days.Add(new SignInDayData
            {
                title = $"第{i + 1}天",
                rewards = new List<SignInReward>
                {
                    new SignInReward {rewardId = "Coin", amount = (i + 1) * 100}
                }
            });
        }

        return days;
    }

    /// <summary>
    /// 深拷贝外部签到数据
    /// </summary>
    private static List<SignInDayData> CloneDays(IReadOnlyList<SignInDayData> sourceDays)
    {
        List<SignInDayData> clonedDays = new List<SignInDayData>(sourceDays.Count);
        for (int i = 0; i < sourceDays.Count; i++)
        {
            SignInDayData sourceDay = sourceDays[i];
            SignInDayData clonedDay = new SignInDayData
            {
                title = sourceDay != null ? sourceDay.title : string.Empty,
                rewards = new List<SignInReward>()
            };

            if (sourceDay != null && sourceDay.rewards != null)
            {
                for (int rewardIndex = 0; rewardIndex < sourceDay.rewards.Count; rewardIndex++)
                {
                    SignInReward sourceReward = sourceDay.rewards[rewardIndex];
                    if (sourceReward == null)
                    {
                        continue;
                    }

                    clonedDay.rewards.Add(new SignInReward
                    {
                        rewardId = sourceReward.rewardId,
                        amount = sourceReward.amount,
                        icon = sourceReward.icon
                    });
                }
            }

            clonedDays.Add(clonedDay);
        }

        return clonedDays;
    }
}
