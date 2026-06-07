using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.1f, 0.7f, 0.9f)]
[TrackClipType(typeof(MyPlayableClip))]
[TrackBindingType(typeof(TimeLineUpdateObj))]
public class UpdateableTrack : TrackAsset {
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playable = ScriptPlayable<UpdateBehaviour>.Create(graph,inputCount);
        var director = go.GetComponent<PlayableDirector>();
        var binding = director.GetGenericBinding(this) as TimeLineUpdateObj;
        var behaviour = playable.GetBehaviour();
        behaviour.timeLineUpdateObj = binding;
        var clips = GetClips();
        foreach (var clip in clips)
        {
            var myPlayableAsset = clip.asset as MyPlayableClip;   
            myPlayableAsset.Time = clip.start;
            behaviour.LisplayTime.Enqueue(new Vector2((float)clip.start,(float)clip.end));
            Debug.Log("TIME : " +new Vector2((float)clip.start,(float)clip.end));
        }
        return playable;
    }
}