namespace _Scripts.Messages.Room
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