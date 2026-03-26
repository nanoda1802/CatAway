namespace _Scripts._Messages.Shared
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