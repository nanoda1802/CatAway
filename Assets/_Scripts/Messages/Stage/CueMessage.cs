using _Scripts.Stage.UI.Pop;

namespace _Scripts.Messages.Stage
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