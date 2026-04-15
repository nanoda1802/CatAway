namespace _Scripts.Room._Messages
{
    public readonly struct SwitchStartMessage
    {
        public bool CanStart { get; }

        public SwitchStartMessage(bool canStart)
        {
            CanStart = canStart;
        }
    }
}