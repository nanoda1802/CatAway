using _Scripts.Scene_Room.Enums;
using UnityEngine;

namespace _Scripts.Messages.Room
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