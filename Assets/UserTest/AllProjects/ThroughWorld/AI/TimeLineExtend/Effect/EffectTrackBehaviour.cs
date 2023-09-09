using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using CMF;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TimelineExtend
{
    [Serializable]
    public class EffectTrackBehaviour : PlayableBehaviour
    {
        [HideInEditorMode]
        [LabelText("�󶨵Ľ�ɫ")]
        public RoleBase role;
        [LabelText("��Ч����")]
        public string effectName;
        [LabelText("��Ч�Ƿ����Լ�����")]
        public bool isInSelf;
        [LabelText("��Ч��")]
        public ModelPointType effectPoint = ModelPointType.Root;
        [LabelText("ƫ��ֵ")]
        public Vector3 offsetVector = new Vector3();

        [HideInInspector]
        /// <summary>
        /// ��Ч
        /// </summary>
        public GameObject effect;
        [HideInInspector]
        public ParticleSystem particleSystem;
        public override void OnGraphStart(Playable playable)
        {
            //Debug.LogError($"����:OnGraphStart");
        }
        public override void OnGraphStop(Playable playable)
        {
            //Debug.LogError($"����:OnGraphStop");
        }
        public override void OnPlayableCreate(Playable playable)
        {
            //Debug.LogError($"����:OnPlayableCreate");
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            //Debug.LogError($"����:OnBehaviourPlay");
        }
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            //Debug.LogError($"����:OnBehaviourPause");
            Destory();
        }
        public override void PrepareData(Playable playable, FrameData info)
        {
            //Debug.LogError($"����:PrepareData");
        }
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            //Debug.LogError($"����:PrepareFrame");
        }
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            //RoleBase trackBinding = playerData as RoleBase;
            //if (!trackBinding)
            //{
            //    trackBinding = playable.GetHandle();
            //}
            //if (!trackBinding) return;

            RoleBase trackBinding = role;

            if (effect == null)
            {
                if (isInSelf)
                {
                    effect = trackBinding.PlayEffectAtSelf(effectName, effectPoint);
                }
                else
                {
                    effect = trackBinding.PlayEffect(effectName, effectPoint);
                }

                effect.transform.localPosition += offsetVector;
                particleSystem = effect.GetComponentInChildren<ParticleSystem>(true);
            }

            if (particleSystem != null)
            {
                //float progress = (float)(playable.GetTime() / playable.GetDuration());

#if UNITY_EDITOR
                particleSystem.Simulate(0.02f, true, false, false);
#else
               //particleSystem.Simulate(info.deltaTime, true, false, false);
#endif
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            //Debug.LogError($"����:OnPlayableDestroy");
            Destory();
        }

        public void Destory()
        {
            if (effect)
            {
                UnityEngine.Object.DestroyImmediate(effect);
                effect = null;
                particleSystem = null;
            }
        }
    }
}