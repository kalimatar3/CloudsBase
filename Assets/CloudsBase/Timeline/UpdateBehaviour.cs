using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UpdateBehaviour : PlayableBehaviour 
{
    public TimeLineUpdateObj timeLineUpdateObj;
    public Queue<Vector2> LisplayTime =  new Queue<Vector2>();
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        // double currentTime = playable.GetTime();
        // double duration = playable.GetDuration();
        // Vector2 VectorTime = LisplayTime.Peek();
        // if(currentTime >= VectorTime.x && currentTime <= VectorTime.y) {
        //     this.timeLineUpdateObj.UpdateinTimLine();
        // }
        // else if(currentTime > VectorTime.y) {
        //     LisplayTime.Dequeue();
        // }
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if(LisplayTime.Count <=0) return;
        double currentTime = playable.GetTime();
        double duration = playable.GetDuration();
        Vector2 VectorTime = LisplayTime.Peek();
        if(currentTime >= VectorTime.x && currentTime <= VectorTime.y) {
            this.timeLineUpdateObj.UpdateinTimLine();
        }
        else if(currentTime > VectorTime.y) {
            LisplayTime.Dequeue();
        }
    }
}