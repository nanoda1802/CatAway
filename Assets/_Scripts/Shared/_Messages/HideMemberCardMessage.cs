namespace _Scripts.Shared._Messages
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