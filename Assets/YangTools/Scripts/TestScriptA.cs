using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YangTools.Scripts.Core.YangExtend;

public class TestScriptA : MonoBehaviour
{
    IEnumerator Start()
    {
        // YangExtend.AddEventListener<DefaultEventMsg>(gameObject, (msg) =>
        // {
        //     Debug.LogError($"收到事件：{msg.Name}--{msg.Args}");
        // });
        // Debug.LogError("测试1");
        // yield return new WaitForSeconds(1);
        // Debug.LogError("测试2");
        // YangExtend.SendEvent<DefaultEventMsg>(new DefaultEventMsg());
        // DefaultEventMsg temp = new DefaultEventMsg();
        // temp.SendSelf();
        
        yield return new WaitForSeconds(1);
    }
}
