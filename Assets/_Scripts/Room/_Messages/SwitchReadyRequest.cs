namespace _Scripts.Room._Messages
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