namespace _Scripts.Shared._Messages
{
    public readonly struct AvatarMessage
    {
        public int AvatarIndex { get; }

        public AvatarMessage(int avatarIndex)
        {
            AvatarIndex = avatarIndex;
        }
    }
}