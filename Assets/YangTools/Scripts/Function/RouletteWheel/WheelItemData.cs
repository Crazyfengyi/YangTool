using UnityEngine;

/// <summary>
/// 转盘项目数据类，用于存储转盘上每个项目的信息
/// </summary>
[System.Serializable]
public class WheelItemData
{
    /// <summary>
    /// 项目唯一标识符
    /// </summary>
    [SerializeField] private string id;
    /// <summary>
    /// 项目显示名称
    /// </summary>
    [SerializeField] private string displayName;
    /// <summary>
    /// 项目图标
    /// </summary>
    [SerializeField] private Sprite icon;
    /// <summary>
    /// 项目权重，用于控制转盘中的概率分布
    /// 默认值为1
    /// </summary>
    [SerializeField] private int weight = 1;
    /// <summary>
    /// 获取项目ID
    /// </summary>
    public string Id => id;
    /// <summary>
    /// 获取项目显示名称
    /// </summary>
    public string DisplayName => displayName;
    /// <summary>
    /// 获取项目图标
    /// </summary>
    public Sprite Icon => icon;
    /// <summary>
    /// 获取项目权重
    /// </summary>
    public int Weight => weight;
    /// <summary>
    /// 项目附加数据，可存储任意类型的数据
    /// </summary>
    public object Payload { get; private set; }
    /// <summary>
    /// 构造函数，用于创建转盘项目数据实例
    /// </summary>
    /// <param name="id">项目唯一标识符</param>
    /// <param name="displayName">项目显示名称</param>
    /// <param name="icon">项目图标，可选参数，默认为null</param>
    /// <param name="weight">项目权重，可选参数，默认为1</param>
    /// <param name="payload">项目附加数据，可选参数，默认为null</param>
    public WheelItemData(string id, string displayName, Sprite icon = null, int weight = 1, object payload = null)
    {
        this.id = id;
        this.displayName = displayName;
        this.icon = icon;
        this.weight = weight;
        Payload = payload;
    }
}
