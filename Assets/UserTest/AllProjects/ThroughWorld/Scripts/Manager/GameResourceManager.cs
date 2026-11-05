using UnityEngine;
using YangTools;
using YangTools.Log;
using YangTools.Scripts.Core;

/// <summary>
/// 资源管理器
/// </summary>
public class GameResourceManager : MonoSingleton<GameResourceManager>
{
    public BuffConfigSave buffConfigSave;

    protected override void Awake()
    {
        base.Awake();
        buffConfigSave?.Init();
    }

    /// <summary>
    /// 根据编号获取BUFF配置
    /// </summary>
    /// <param name="id">BUFF编号</param>
    /// <returns>找到的BUFF配置</returns>
    public BuffConfig GetBuffConfig(int id = 0)
    {
        if (buffConfigSave == null)
        {
            Debuger.ToError("BUFF配置资源未配置");
            return null;
        }

        BuffConfig data = buffConfigSave.TryGet(id);
        if (data == null)
        {
            Debuger.ToError($"BUFF加载失败:{id}");
        }
        return data;
    }

    public GameObject ResourceLoad(string str)
    {
        GameObject obj = Resources.Load<GameObject>(str);
        return obj;
    }

    public T ResourceLoad<T>(string str) where T : Object
    {
        T obj = Resources.Load<T>(str);
        return obj;
    }
}
