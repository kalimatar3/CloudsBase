using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

public class TriggerBehaviour : PlayableBehaviour
{
    protected TriggerableEnvironment triggerableEnvironment;
    public TriggerableEnvironment TriggerableEnvironment {
        get {return triggerableEnvironment;}
        set { triggerableEnvironment = value;}
    }
    private bool hasTriggered = false;
    public Queue<double> StarttimeQueue = new Queue<double>();
    public Queue<double> EndtimeQueue = new Queue<double>();
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        double currentTime = playable.GetTime();
        double duration = playable.GetDuration();
        if(StarttimeQueue.Count > 0)
        {
        if (!hasTriggered && currentTime >= StarttimeQueue.Peek() && currentTime <= duration)
            {
                StarttimeQueue.Dequeue();
                hasTriggered = true;
                TriggerableEnvironment?.Trigger();
            }    
        }
        if(EndtimeQueue.Count > 0) {
            if (hasTriggered && currentTime >= EndtimeQueue.Peek()  && currentTime <= duration)
            {
                TriggerableEnvironment?.TriggerOut();
                EndtimeQueue.Dequeue();
                hasTriggered = false;
            }
        }
    }
}
