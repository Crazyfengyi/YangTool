using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class AutoMixer : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //混合
        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            //当前clip的权重
            float inputWeight = playable.GetInputWeight(i);
            //包装类
            ScriptPlayable<AutoBehaviour> temp = (ScriptPlayable<AutoBehaviour>)playable.GetInput(i);
            //获得具体类
            AutoBehaviour behaviour = temp.GetBehaviour();
            //其中一个混合逻辑
            if (behaviour.GetType() == typeof(DamageCheckBehaviour))
            {
                DamageCheckBehaviour input = (DamageCheckBehaviour)behaviour;
            }
        }
    }
}
