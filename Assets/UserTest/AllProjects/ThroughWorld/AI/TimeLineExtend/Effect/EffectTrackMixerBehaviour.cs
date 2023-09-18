using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TimelineExtend
{
    public class EffectTrackMixerBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// �ú���������ʱ�ͱ༭ʱ����,����������ֵʱ���ס��һ�㡣
        /// </summary>
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            RoleBase trackBinding = playerData as RoleBase;

            if (!trackBinding)
                return;

            int inputCount = playable.GetInputCount();

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<EffectTrackBehaviour> inputPlayable = (ScriptPlayable<EffectTrackBehaviour>)playable.GetInput(i);
                EffectTrackBehaviour input = inputPlayable.GetBehaviour();
                if (input.effect && inputWeight == 0)
                {
                    input.Destory();
                }
            }

        }
    }
}