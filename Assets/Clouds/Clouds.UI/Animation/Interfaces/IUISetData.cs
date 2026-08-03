using UnityEngine;

namespace Clouds.UI
{
    public interface IUISetData
    {
        GameObject UIObj { get; }
        float Delay {get;}
        float TimePlay { get; }
    }
}
