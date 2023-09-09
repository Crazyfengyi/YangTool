using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TimelineExtend
{
    [LabelText("显示粒子特效")]
    [Serializable]
    public class EffectTrackClip : PlayableAsset, ITimelineClipAsset
    {
        public EffectTrackBehaviour template = new EffectTrackBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<EffectTrackBehaviour> playable = ScriptPlayable<EffectTrackBehaviour>.Create(graph, template);
            EffectTrackBehaviour clone = playable.GetBehaviour();
            RoleBase role = owner.GetComponent<RoleBase>();
            clone.role = role;

            return playable;
        }
    }
}