using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class MyPlayableClip : PlayableAsset, ITimelineClipAsset
{
    public double Time;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TriggerBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
         //behaviour.startTime = Time;
        return playable;
    }
    public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Extrapolation;
}
