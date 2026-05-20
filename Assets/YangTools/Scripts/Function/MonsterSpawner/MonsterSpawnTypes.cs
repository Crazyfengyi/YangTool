using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 生成模式
/// </summary>
public enum SpawnMode
{
    [LabelText("连续")]
    Continuous,
    [LabelText("波次")]
    Waves
}

/// <summary>
/// 生成条目类，用于定义可生成对象的预制体及其权重
/// </summary>
[Serializable]
public class SpawnEntry
{
    /// <summary>
    /// 要生成的游戏对象预制体
    /// </summary>
    public GameObject Prefab;
    /// <summary>
    /// 生成权重，用于控制生成的概率
    /// 使用Min特性确保最小值为1
    /// </summary>
    [Min(1)] public int Weight = 1;
}

/// <summary>
/// 波次生成规则类，用于定义一波敌人的生成参数和行为
/// </summary>
[Serializable]
public class WaveSpawnRule
{
    /// <summary>
    /// 每波生成的敌人数量，默认值为10
    /// </summary>
    [Min(1)] public int Count = 10;
    /// <summary>
    /// 敌人生成的时间间隔（秒），默认值为0.5秒
    /// </summary>
    [Min(0.01f)] public float SpawnInterval = 0.5f;
    /// <summary>
    /// 开始生成前的延迟时间（秒），默认值为0秒
    /// </summary>
    [Min(0f)] public float StartDelay = 0f;
    /// <summary>
    /// 当前波次结束后到下一波次开始前的延迟时间（秒），默认值为3秒
    /// </summary>
    [Min(0f)] public float NextWaveDelay = 3f;
    /// <summary>
    /// 是否等待当前波次所有敌人都被消灭后再开始下一波，默认值为true
    /// </summary>
    public bool WaitUntilAllDead = true;

    /// <summary>
    /// 敌人生成池，包含要生成的敌人类型和数量等信息
    /// </summary>
    public List<SpawnEntry> Pool = new();
}
