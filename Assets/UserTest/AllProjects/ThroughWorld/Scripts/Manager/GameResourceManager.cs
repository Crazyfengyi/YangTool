using UnityEngine;
using YangTools;
using YangTools.Log;

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

    public BuffConfig GetBuffConfig(int id = 0)
    {
        BuffConfig data = buffConfigSave.TryGet(id);
        if (data == null)
        {
            Debuger.ToError($"BUFF加载失败:{id}");
        }
        return data;
    }

    public GameObject ResoruceLoad(string str)
    {
        GameObject obj = Resources.Load<GameObject>(str);
        return obj;
    }

    public T ResoruceLoad<T>(string str) where T : Object
    {
        T obj = Resources.Load<T>(str);
        return obj;
    }
}