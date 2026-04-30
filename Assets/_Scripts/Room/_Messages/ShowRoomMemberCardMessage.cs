using _Scripts.Room._Enums;
using UnityEngine;

namespace _Scripts.Room._Messages
{
    public readonly struct ShowRoomMemberCardMessage
    {
        public ulong MemberId { get; }
        public string MemberName { get; }
        public MemberIconType MemberType { get; }
        public Vector3 SpawnPoint { get; }

        public ShowRoomMemberCardMessage(
            ulong memberId,
            string memberName,
            MemberIconType memberType,
            Vector3 spawnPoint)
        {
            MemberId = memberId;
            MemberName = memberName;
            MemberType = memberType;
            SpawnPoint = spawnPoint;
        }
    }
}