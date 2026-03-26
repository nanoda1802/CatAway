namespace _Scripts.Messages.Room
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