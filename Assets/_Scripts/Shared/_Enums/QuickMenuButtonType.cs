using System;

namespace _Scripts.Shared._Enums
{
    [Flags]
    public enum QuickMenuButtonType
    {
        Exit = 1 << 0,
        Leave = 1 << 1,
        Setting = 1 << 2,
        Tutorial = 1 << 3,
        Customize = 1 << 4,
        Skip =  1 << 5,
        Rename = 1 << 6,
    }
}