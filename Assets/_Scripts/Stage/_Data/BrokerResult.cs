namespace _Scripts.Stage._Data
{
    public readonly struct BrokerResult
    {
        public readonly bool IsSuccess;
        public readonly string Reason;

        public BrokerResult(bool isSuccess, string reason)
        {
            IsSuccess = isSuccess;
            Reason = reason;
        }
    }
}