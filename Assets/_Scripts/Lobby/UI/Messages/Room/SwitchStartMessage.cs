namespace _Scripts.Lobby.UI.Messages.Room
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