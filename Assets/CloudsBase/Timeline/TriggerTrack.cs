using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.3f, 0.6f, 0.9f)]
[TrackClipType(typeof(MyPlayableClip))]
[TrackBindingType(typeof(TriggerableEnvironment))]
public class TriggerTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playable = ScriptPlayable<TriggerBehaviour>.Create(graph, inputCount);

        var director = go.GetComponent<PlayableDirector>();
        var binding = director.GetGenericBinding(this) as TriggerableEnvironment;

        var behaviour = playable.GetBehaviour();
        behaviour.TriggerableEnvironment = binding;
        var clips = GetClips();
        foreach (var clip in clips)
        {
            var myPlayableAsset = clip.asset as MyPlayableClip;   
            myPlayableAsset.Time = clip.start;
            behaviour.StarttimeQueue.Enqueue(clip.start);
            behaviour.EndtimeQueue.Enqueue(clip.end);
        }
        foreach(var ele in behaviour.StarttimeQueue) {
            //Debug.Log(ele);
        }
        return playable;    
    }
}
