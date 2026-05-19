using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class RouletteWheelDemo : MonoBehaviour
{
    [SerializeField] private RouletteWheel rouletteWheel;
    [SerializeField] private int testItemCount = 8;

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

    [Button("Spin Random")]
    public void SpinRandom()
    {
        if (rouletteWheel != null)
        {
            rouletteWheel.SpinRandom(item => Debug.Log($"Roulette random result: {item.DisplayName}", this));
        }
    }

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
