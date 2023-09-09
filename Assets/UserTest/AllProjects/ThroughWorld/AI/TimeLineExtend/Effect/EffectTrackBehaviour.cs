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
        [LabelText("绑定的角色")]
        public RoleBase role;
        [LabelText("特效名称")]
        public string effectName;
        [LabelText("特效是否在自己身上")]
        public bool isInSelf;
        [LabelText("特效点")]
        public ModelPointType effectPoint = ModelPointType.Root;
        [LabelText("偏移值")]
        public Vector3 offsetVector = new Vector3();

        [HideInInspector]
        /// <summary>
        /// 特效
        /// </summary>
        public GameObject effect;
        [HideInInspector]
        public ParticleSystem particleSystem;
        public override void OnGraphStart(Playable playable)
        {
            //Debug.LogError($"测试:OnGraphStart");
        }
        public override void OnGraphStop(Playable playable)
        {
            //Debug.LogError($"测试:OnGraphStop");
        }
        public override void OnPlayableCreate(Playable playable)
        {
            //Debug.LogError($"测试:OnPlayableCreate");
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            //Debug.LogError($"测试:OnBehaviourPlay");
        }
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            //Debug.LogError($"测试:OnBehaviourPause");
            Destory();
        }
        public override void PrepareData(Playable playable, FrameData info)
        {
            //Debug.LogError($"测试:PrepareData");
        }
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            //Debug.LogError($"测试:PrepareFrame");
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
            //Debug.LogError($"测试:OnPlayableDestroy");
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