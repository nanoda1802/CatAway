using _Scripts.Stage._Enums;

namespace _Scripts.Room._Messages
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