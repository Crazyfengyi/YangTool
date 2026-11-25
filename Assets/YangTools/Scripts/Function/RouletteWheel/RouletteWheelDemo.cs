using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 为转盘设置演示奖项并提供测试旋转入口
/// </summary>
public class RouletteWheelDemo : MonoBehaviour
{
    // 演示使用的转盘组件
    [SerializeField] private RouletteWheel rouletteWheel;
    // 初始化的演示奖项数量
    [SerializeField] private int testItemCount = 8;

    /// <summary>
    /// 取得转盘组件并初始化演示条目列表
    /// </summary>
    private void Start()
    {
        if (rouletteWheel == null)
        {
            rouletteWheel = GetComponent<RouletteWheel>();
        }
        if (rouletteWheel != null)
        {
            rouletteWheel.SetItems(CreateDemoItems());
        }
    }

    /// <summary>
    /// 启动加权随机旋转并记录测试结果
    /// </summary>
    [Button("Spin Random")]
    public void SpinRandom()
    {
        if (rouletteWheel != null)
        {
            rouletteWheel.SpinRandom(item => Debug.Log($"Roulette random result: {item.DisplayName}", this));
        }
    }

    /// <summary>
    /// 旋转到首个条目并记录测试结果
    /// </summary>
    [Button("Spin To First")]
    public void SpinToFirst()
    {
        if (rouletteWheel != null)
        {
            rouletteWheel.SpinToIndex(0, item => Debug.Log($"Roulette fixed result: {item.DisplayName}", this));
        }
    }
    /// <summary>
    /// 创建测试数据
    /// </summary>
    private List<WheelItemData> CreateDemoItems()
    {
        var result = new List<WheelItemData>();
        int count = Mathf.Max(1, testItemCount);
        for (int i = 0; i < count; i++)
        {
            int index = i + 1;
            result.Add(new WheelItemData(index.ToString(), $"奖励{index}", null, index));
        }
        return result;
    }
}
