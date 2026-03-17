namespace _Scripts.Messages
{
    public readonly struct NoticeMessage
    {
        public string Notice { get; }

        public NoticeMessage(string notice)
        {
            Notice = notice;
        }
    }
}