namespace _Scripts.Lobby.UI.Messages.Member
{
    public readonly struct SwitchReadyRequest
    {
        public bool CancelReady { get; }

        public SwitchReadyRequest(bool cancelReady = false)
        {
            CancelReady = cancelReady;
        }
    }
}