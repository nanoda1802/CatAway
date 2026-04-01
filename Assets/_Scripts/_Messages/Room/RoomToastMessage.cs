namespace _Scripts.Messages
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