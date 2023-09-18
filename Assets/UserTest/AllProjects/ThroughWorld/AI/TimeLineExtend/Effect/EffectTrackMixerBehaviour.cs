using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TimelineExtend
{
    public class EffectTrackMixerBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// 该函数在运行时和编辑时调用,在设置属性值时请记住这一点。
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