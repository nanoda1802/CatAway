namespace _Scripts.Lobby.UI.Messages.Room
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