namespace _Scripts.Room._Messages
{
    public readonly struct RoomToastMessage
    {
        public string Notice { get; }

        public RoomToastMessage(string notice)
        {
            Notice = notice;
        }
    }
}