namespace _Scripts.Lobby.UI.Messages.Member
{
    public readonly struct SwitchReadyRespond
    {
        public ulong MemberId { get; }
        public bool IsReady { get; }
        public bool ToMe { get; }

        public SwitchReadyRespond(ulong memberId, bool isReady, bool toMe)
        {
            MemberId = memberId;
            IsReady = isReady;
            ToMe = toMe;
        }
    }
}