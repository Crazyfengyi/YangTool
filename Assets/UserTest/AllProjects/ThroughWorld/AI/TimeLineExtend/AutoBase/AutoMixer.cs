using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class AutoMixer : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //���
        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            //��ǰclip��Ȩ��
            float inputWeight = playable.GetInputWeight(i);
            //��װ��
            ScriptPlayable<AutoBehaviour> temp = (ScriptPlayable<AutoBehaviour>)playable.GetInput(i);
            //��þ�����
            AutoBehaviour behaviour = temp.GetBehaviour();
            //����һ������߼�
            if (behaviour.GetType() == typeof(DamageCheckBehaviour))
            {
                DamageCheckBehaviour input = (DamageCheckBehaviour)behaviour;
            }
        }
    }
}
