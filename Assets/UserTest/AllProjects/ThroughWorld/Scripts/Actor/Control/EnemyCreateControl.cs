/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-08-04 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using System.Collections.Generic;

/// <summary>
/// 怪物波数生成控制器
/// </summary>

public class EnemyCreateControl : MonoBehaviour
{
    public List<WaveData> waveDatas;
    public EnemyCreateType enemyCreateType;
    private WaveData currentWave;//当前波数
    private int currentWaveIndex;

    private int waitSpawnNum;//当前波还剩多少未生成
    private int currentAliveNum;//当前存活敌人数
    private float timer;
    private int nextSpawnTime;//下一波生成时间

    public void Start()
    {
        nextSpawnTime = 10;
        currentWaveIndex = 0;
        SetWave(0);
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= nextSpawnTime && enemyCreateType == EnemyCreateType.时间波数生成)
        {
            timer = 0;
            if (waitSpawnNum <= 0)
            {
                SetNextWave();
            }
        }

        if (waitSpawnNum > 0)
        {
            waitSpawnNum--;
            currentAliveNum++;

            //EnemyCreateData[] date = currentWave.MonsterCreateData;
            //生成怪物
            Monster monster = GameActorManager.Instance.CreateMonster();
            monster.OnDieAction += EnemyDeath;

            GameUIManager.Instance.AddTipsShow("新的怪物生成");
        }
    }

    /// <summary>
    /// 怪物死亡回调
    /// </summary>
    private void EnemyDeath()
    {
        currentAliveNum--;
        if (currentAliveNum <= 0 && enemyCreateType == EnemyCreateType.波数怪清完生成)
        {
            SetNextWave();
        }
    }
    /// <summary>
    /// 设置下一波生成
    /// </summary>
    public void SetNextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex < waveDatas.Count)
        {
            SetWave(currentWaveIndex);
        }
    }
    public void SetWave(int index)
    {
        Debug.LogError($"当前波数:{currentWaveIndex}");
        currentWave = waveDatas[index];
        waitSpawnNum = waveDatas[index].enemyNum;
    }
}

[Serializable]
/// <summary>
/// 波数信息
/// </summary>
public class WaveData
{
    public int enemyNum;//生成数
    public float timeSpawnInterval;//生成间隔
    public EnemyCreateData[] MonsterCreateData;

    public WaveData()
    {

    }
}
/// <summary>
/// 敌人生成信息
/// </summary>
public struct EnemyCreateData
{
    public int monsterID;//ID
    public int monsterNum;//数量
}

/// <summary>
/// 敌人生成类型
/// </summary>
public enum EnemyCreateType
{
    None,
    直接生成,
    时间波数生成,
    波数怪清完生成,
}