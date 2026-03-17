using _Scripts.Stage.Data;

namespace _Scripts.Messages.Room
{
    public readonly struct SwitchModeRespond
    {
        public StageMode Mode { get; }

        public SwitchModeRespond(StageMode mode)
        {
            Mode = mode;
        }
    }
}