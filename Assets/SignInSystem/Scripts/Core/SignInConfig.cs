using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 签到奖励数据
/// </summary>
[Serializable]
public class SignInReward
{
    public string rewardId;
    public int amount = 1;
    public Sprite icon;
}

/// <summary>
/// 单日签到数据
/// </summary>
[Serializable]
public class SignInDayData
{
    public string title;
    public List<SignInReward> rewards = new List<SignInReward>();
}

/// <summary>
/// 签到状态
/// </summary>
public enum SignInDayState
{
    Locked,
    Available,
    Claimed
}

/// <summary>
/// 签到配置资产
/// </summary>
[CreateAssetMenu(fileName = "SignInConfig", menuName = "YangTools/Sign In/Config")]
public class SignInConfig : ScriptableObject
{
    public List<SignInDayData> days = new List<SignInDayData>();
}
