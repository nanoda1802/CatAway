using Unity.Collections;

namespace _Scripts._Messages.Room
{
    public readonly struct UpdateMemberNameMessage
    {
        public ulong MemberId { get; }
        public string Nickname { get; }

        public UpdateMemberNameMessage(ulong memberId, FixedString32Bytes nickname)
        {
            MemberId = memberId;
            Nickname = nickname.Value;
        }
    }
}