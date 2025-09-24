using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 配置数据保存
/// </summary>
[Serializable]
public abstract class SaveDataWithSO<T> : ScriptableObject where T : ConfigBase
{
    /// <summary>
    /// 字典(运行时用),字典不能序列化
    /// </summary>
    internal Dictionary<int, T> dataDic;
    /// <summary>
    /// 数据列表(序列化用)
    /// </summary>
    [SerializeField]
    [ListDrawerSettings(/*DraggableItems = false,*/ ShowIndexLabels = true, ListElementLabelName = "ConfigName")]
    public List<T> dataList = new List<T>();
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {
        dataDic = new Dictionary<int, T>();
        foreach (T item in dataList)
        {
            dataDic.Add(item.id, item);
        }
    }
    /// <summary>
    /// 获得泛型类型
    /// </summary>
    public virtual Type ConfigType
    {
        get { return typeof(T); }
    }
    /// <summary>
    /// 获取指定id数据
    /// </summary>
    public virtual T TryGet(int id)
    {
        T item;
        if (!dataDic.TryGetValue(id, out item))
        {
            return null;
        }
        return item;
    }
    /// <summary>
    /// 获得List数据
    /// </summary>
    public virtual List<T> GetList()
    {
        List<T> data = dataList.ToList();
        return data;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 清理所有数据
    /// </summary>
    public virtual void ClearData()
    {
        dataList.Clear();
        dataDic.Clear();
    }
    /// <summary>
    /// 添加数据
    /// </summary>
    public void AddData(T config)
    {
        dataList.Add(config);
        dataDic.Add(config.id, config);
    }
    /// <summary>
    /// 移除数据
    /// </summary>
    public void RemoveData(T config)
    {
        dataList.Remove(config);
        dataDic.Remove(config.id);
    }
#endif

}

/// <summary>
/// 配置基类
/// </summary>
[Serializable]
public class ConfigBase
{
    [ReadOnly]
    [SerializeField]
    public int id;
    /// <summary>
    /// 克隆,默认浅克隆
    /// </summary>
    public virtual ConfigBase Clone()
    {
        return (ConfigBase)this.MemberwiseClone();
    }
}