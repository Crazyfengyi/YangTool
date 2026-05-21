using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff控制器类，用于管理游戏中的增益效果
/// 继承自MonoBehaviour，可附加到游戏对象上
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class BuffController : MonoBehaviour
{
    // 存储当前激活的增益效果及其对应的协程
    private readonly Dictionary<BuffDefinition, Coroutine> activeBuffs = new Dictionary<BuffDefinition, Coroutine>();
    // 角色属性引用
    private CharacterStats stats;

    /// <summary>
    /// 游戏对象初始化时调用
    /// 获取角色属性组件引用
    /// </summary>
    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    /// <summary>
    /// 应用增益效果
    /// </summary>
    /// <param name="buff">增益定义</param>
    /// <param name="source">增益来源</param>
    public void ApplyBuff(BuffDefinition buff, object source)
    {
        //检查增益定义和角色属性是否有效
        if (buff == null || stats == null) return;
        //如果增益已存在
        if (activeBuffs.TryGetValue(buff, out Coroutine existing))
        {
            //如果增益不支持刷新持续时间，则直接返回
            if (!buff.RefreshDuration) return;
            //停止现有的增益协程
            StopCoroutine(existing);
            //移除现有增益的修饰符
            RemoveBuffModifiers(buff);
        }

        //添加增益修饰符
        AddBuffModifiers(buff);
        //启动增益生命周期协程
        activeBuffs[buff] = StartCoroutine(BuffLifetime(buff));
    }

    /// <summary>
    /// 移除增益效果
    /// </summary>
    /// <param name="buff">要移除的增益定义</param>
    public void RemoveBuff(BuffDefinition buff)
    {
        //检查增益定义是否有效
        if (buff == null) return;
        //如果增益存在
        if (activeBuffs.TryGetValue(buff, out Coroutine routine))
        {
            //停止增益协程
            StopCoroutine(routine);
            //从激活增益字典中移除
            activeBuffs.Remove(buff);
            //移除增益修饰符
            RemoveBuffModifiers(buff);
        }
    }

    /// <summary>
    /// 增益生命周期协程
    /// </summary>
    /// <param name="buff">增益定义</param>
    /// <returns>协程返回枚举</returns>
    private IEnumerator BuffLifetime(BuffDefinition buff)
    {
        //如果增益持续时间大于0，则等待指定时间
        if (buff.Duration > 0f)
        {
            yield return new WaitForSeconds(buff.Duration);
        }
        //移除增益
        activeBuffs.Remove(buff);
        RemoveBuffModifiers(buff);
    }

    /// <summary>
    /// 添加增益修饰符
    /// </summary>
    /// <param name="buff">增益定义</param>
    private void AddBuffModifiers(BuffDefinition buff)
    {
        //遍历增益的所有修饰符
        for (int i = 0; i < buff.Modifiers.Count; i++)
        {
            StatModifier modifier = buff.Modifiers[i];
            //将修饰符添加到角色属性中
            stats.AddModifier(modifier.Stat, modifier.Type, modifier.Value, buff);
        }
    }

    /// <summary>
    /// 移除增益修饰符
    /// </summary>
    /// <param name="buff">增益定义</param>
    private void RemoveBuffModifiers(BuffDefinition buff)
    {
        //从角色属性中移除指定增益的所有修饰符
        stats.RemoveModifiersFrom(buff);
    }
}
