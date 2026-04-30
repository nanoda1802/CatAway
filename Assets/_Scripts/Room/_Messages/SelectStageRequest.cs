namespace _Scripts.Room._Messages
{
    public readonly struct SelectStageRequest
    {
        public bool ToLeft { get; }

        public SelectStageRequest(bool toLeft)
        {
            ToLeft = toLeft;
        }
    }
}