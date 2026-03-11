namespace _Scripts.Lobby.UI.Messages.Member
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