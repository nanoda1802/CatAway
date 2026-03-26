using _Scripts.Scene_Stage.Enums;

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