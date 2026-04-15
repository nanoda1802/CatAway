
using _Scripts.Stage._Enums;

namespace _Scripts.Stage._Messages
{
    public readonly struct CueMessage
    {
        public CueType Type { get; }
        public float Duration { get; }

        public CueMessage(CueType type, float duration)
        {
            Type = type;
            Duration = duration;
        }
    }
}