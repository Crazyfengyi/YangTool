using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YangTools;
using YangTools.Log;
/// <summary>
/// 资源管理器
/// </summary>
public class GameResourceManager : MonoSingleton<GameResourceManager>
{
    public BuffConfigSave buffConfigSave;
    public new void Awake()
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

}
