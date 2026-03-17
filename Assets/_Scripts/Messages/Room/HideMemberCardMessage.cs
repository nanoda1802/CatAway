namespace _Scripts.Messages.Room
{
    public readonly struct HideMemberCardMessage
    {
        public ulong MemberId { get; }

        public HideMemberCardMessage(ulong memberId)
        {
            MemberId = memberId;
        }
    }
}