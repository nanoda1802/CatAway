namespace _Scripts.Shared._Messages
{
    public readonly struct RenameMessage
    {
        public string Nickname { get; }

        public RenameMessage(string nickname)
        {
            Nickname = nickname;
        }
    }
}