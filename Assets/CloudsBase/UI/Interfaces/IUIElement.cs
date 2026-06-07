using UnityEngine;

namespace Clouds.Ultilities
{
    public interface IUISetData
    {
        GameObject UIObj { get; }
        float Delay {get;}
        float TimePlay { get; }
    }
}