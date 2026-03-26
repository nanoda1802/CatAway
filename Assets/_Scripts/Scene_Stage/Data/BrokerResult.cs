namespace _Scripts.Scene_Stage.Data
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