using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 怪物生成器类，用于在游戏中生成怪物，支持连续生成和波次生成两种模式
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [Header("Common")] //通用参数
    [SerializeField] 
    private SpawnMode mode = SpawnMode.Waves; //生成模式：波次或连续
    [SerializeField] 
    [LabelText("怪物生成点列表")]
    private List<MonsterSpawnPoint> spawnPoints = new();
    [SerializeField] 
    private int maxAlive = 100; //最大同时存活的怪物数量
    [SerializeField] 
    private bool autoStart = true; //是否自动开始生成

    [Header("连续生成")]
    [SerializeField] private float continuousInterval = 1.2f; //连续生成的时间间隔
    [SerializeField] private List<SpawnEntry> continuousPool = new(); //连续生成的怪物池

    [Header("波次生成")]
    [SerializeField] private List<WaveSpawnRule> waves = new(); //波次生成规则列表
    [SerializeField] private bool loopWaves; //是否循环波次

    //怪物生成时触发
    public event Action<GameObject> OnMonsterSpawned;
    //波次开始时触发
    public event Action<int> OnWaveStarted;
    //波次完成时触发
    public event Action<int> OnWaveCompleted;
    //所有波次完成时触发
    public event Action OnAllWavesCompleted;

    private readonly List<GameObject> aliveMonsters = new(); // 当前存活的怪物列表
    private Coroutine runner; //运行协程
    private bool isRunning; //是否正在运行

    /// <summary>
    /// 获取当前存活的怪物数量
    /// </summary>
    public int AliveCount
    {
        get
        {
            CleanupDeadRefs(); // 清理无效引用
            return aliveMonsters.Count;
        }
    }

    private void Start()
    {
        if (autoStart)
        {
            StartSpawn(); //自动开始生成
        }
    }

    /// <summary>
    /// 开始生成怪物
    /// </summary>
    public void StartSpawn()
    {
        if (isRunning) return; //如果已经在运行，则直接返回
        if (!ValidateSetup()) return; //验证设置是否有效

        isRunning = true;
        //根据模式选择运行连续生成或波次生成
        runner = StartCoroutine(mode == SpawnMode.Continuous ? RunContinuous() : RunWaves());
    }

    /// <summary>
    /// 停止生成怪物
    /// </summary>
    public void StopSpawn()
    {
        isRunning = false;
        if (runner != null)
        {
            StopCoroutine(runner); // 停止协程
            runner = null;
        }
    }

    /// <summary>
    /// 清空存活怪物列表
    /// </summary>
    public void ClearAliveList()
    {
        aliveMonsters.Clear();
    }

    /// <summary>
    /// 连续生成模式的协程
    /// </summary>
    private IEnumerator RunContinuous()
    {
        var wait = new WaitForSeconds(Mathf.Max(0.01f, continuousInterval)); //设置等待时间
        //上次生成是否成功
        var lastCreateSucceed = true;
        while (isRunning)
        {
            if (!lastCreateSucceed)
            {
                yield break;
            }
            if (AliveCount < maxAlive) //如果当前存活的怪物数量小于最大值
            {
                lastCreateSucceed = TrySpawnOne(continuousPool); //尝试生成一个怪物
            }
            yield return wait; // 等待指定时间
        }
    }

    /// <summary>
    /// 波次生成模式的协程
    /// </summary>
    private IEnumerator RunWaves()
    {
        if (waves.Count == 0) // 如果没有波次设置
        {
            isRunning = false;
            yield break;
        }

        do
        {
            for (int i = 0; i < waves.Count; i++)
            {
                var waveIndex = i + 1; // 当前波次索引
                var wave = waves[i]; // 当前波次规则
                OnWaveStarted?.Invoke(waveIndex); //触发波次开始事件

                if (wave.StartDelay > 0f) // 如果有开始延迟
                {
                    yield return new WaitForSeconds(wave.StartDelay); // 等待延迟时间
                }

                yield return SpawnWave(wave); //生成当前波次的怪物

                if (wave.WaitUntilAllDead) //如果需要等待所有怪物死亡
                {
                    yield return new WaitUntil(() => AliveCount == 0 || !isRunning); // 等待直到所有怪物死亡或停止运行
                }
                
                OnWaveCompleted?.Invoke(waveIndex); //触发波次完成事件
                Debug.Log($"Wave {waveIndex} completed.");

                if (!isRunning) yield break; // 如果停止运行，则退出
                if (wave.NextWaveDelay > 0f) // 如果有下一波次延迟
                {
                    yield return new WaitForSeconds(wave.NextWaveDelay); // 等待延迟时间
                }
            }

            if (!loopWaves) //如果不循环波次
            {
                OnAllWavesCompleted?.Invoke(); //触发所有波次完成事件
                isRunning = false;
                runner = null;
                yield break;
            }
        } while (isRunning); //循环直到停止运行
    }

    /// <summary>
    /// 生成单个波次的协程
    /// </summary>
    private IEnumerator SpawnWave(WaveSpawnRule wave)
    {
        var interval = new WaitForSeconds(Mathf.Max(0.01f, wave.SpawnInterval)); // 设置生成间隔
        int spawned = 0; // 已生成的怪物数量

        while (spawned < wave.Count && isRunning) //直到生成足够数量或停止运行
        {
            if (AliveCount >= maxAlive) //如果达到最大存活数量
            {
                yield return null; //等待一帧
                continue;
            }

            if (TrySpawnOne(wave.Pool)) //尝试生成一个怪物
            {
                spawned++;
            }

            if (spawned < wave.Count) //如果还需要生成更多怪物
            {
                yield return interval; //等待间隔时间
            }
        }
    }

    /// <summary>
    /// 尝试生成一个怪物
    /// </summary>
    private bool TrySpawnOne(List<SpawnEntry> pool)
    {
        if (pool == null || pool.Count == 0) return false; // 如果怪物池为空
        if (spawnPoints == null || spawnPoints.Count == 0) return false; //如果生成点列表为空

        var prefab = PickByWeight(pool); //根据权重选择怪物预制体
        var point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)]; //随机选择一个生成点
        if (prefab == null || point == null) return false; //如果选择失败

        var go = Instantiate(prefab, point.GetPosition(), Quaternion.identity); // 实例化怪物
        aliveMonsters.Add(go); // 添加到存活列表
        OnMonsterSpawned?.Invoke(go); // 触发生成事件
        return true;
    }

    /// <summary>
    /// 根据权重从怪物池中选择一个怪物
    /// </summary>
    private GameObject PickByWeight(List<SpawnEntry> pool)
    {
        int total = 0; // 计算总权重
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && pool[i].Prefab != null && pool[i].Weight > 0)
            {
                total += pool[i].Weight;
            }
        }

        if (total <= 0) return null; // 如果总权重小于等于0，返回null

        int roll = UnityEngine.Random.Range(1, total + 1); // 随机选择一个权重值
        int sum = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            var entry = pool[i];
            if (entry == null || entry.Prefab == null || entry.Weight <= 0) continue;

            sum += entry.Weight; //累加权重
            if (roll <= sum) //如果随机值小于等于当前累加权重
            {
                return entry.Prefab; //返回对应的怪物预制体
            }
        }

        return null; //如果没有找到合适的怪物，返回null
    }

    /// <summary>
    /// 验证生成器设置是否有效
    /// </summary>
    private bool ValidateSetup()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("MonsterSpawner has no spawn points configured.", this);
            return false;
        }
        if (mode == SpawnMode.Continuous)
        {
            if (continuousPool == null || continuousPool.Count == 0)
            {
                Debug.LogWarning("MonsterSpawner continuous pool is empty.", this);
                return false;
            }
        }
        else
        {
            if (waves == null || waves.Count == 0)
            {
                Debug.LogWarning("MonsterSpawner waves are empty.", this);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 清理无效的怪物引用
    /// </summary>
    private void CleanupDeadRefs()
    {
        aliveMonsters.RemoveAll(m => m == null); // 移除所有null引用
    }
}
