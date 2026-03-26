namespace _Scripts.Messages
{
    public readonly struct RoomNoticeMessage
    {
        public string Notice { get; }

        public RoomNoticeMessage(string notice)
        {
            Notice = notice;
        }
    }
}