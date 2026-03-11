using System;

namespace _Scripts.Lobby.UI
{
    [Flags]
    public enum QuickMenuType
    {
        Exit = 1 << 0,
        Leave = 1 << 1,
        Setting = 1 << 2,
        Tutorial = 1 << 3,
        Customize = 1 << 4
    }
}