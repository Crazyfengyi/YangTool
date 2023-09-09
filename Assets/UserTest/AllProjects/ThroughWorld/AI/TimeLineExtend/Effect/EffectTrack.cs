using Sirenix.OdinInspector;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TimelineExtend
{
    [TrackColor(1f, 0f, 0.9f)]
    [TrackClipType(typeof(EffectTrackClip))]
    [TrackBindingType(typeof(RoleBase))]
    public class EffectTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<EffectTrackMixerBehaviour>.Create(graph, inputCount);
        }
    }

}