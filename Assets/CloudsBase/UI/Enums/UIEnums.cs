namespace Clouds.Ultilities
{
    public enum MOVEEFFECT { Custom, FromBelow, FromAbove, FromLeft, FromRight, ToAbove, ToBelow, ToLeft, ToRight }
    public enum TRIGGEREFFECT { Move, Rotate, Scale, Shake, Punch, Fade, Color, Nothing }
    
    public enum CONTINUOSEFFECT
    {
        FillText,
        ChangeOpacity,
    }
    public enum ACTIVATETYPE { Sequence, Delay, Continuously }
}