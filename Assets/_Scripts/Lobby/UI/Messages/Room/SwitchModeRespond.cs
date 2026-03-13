using _Scripts.Stage;
using UnityEngine;

namespace _Scripts.Lobby.UI.Messages.Room
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