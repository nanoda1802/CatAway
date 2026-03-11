using _Scripts.Lobby.UI.Room;
using UnityEngine;

namespace _Scripts.Lobby.UI.Messages.Member
{
    public readonly struct ShowMemberCardMessage
    {
        public ulong MemberId { get; }
        public MemberIconType MemberType { get; }
        public Vector3 SpawnPoint { get; }

        public ShowMemberCardMessage(ulong memberId, MemberIconType memberType, Vector3 spawnPoint)
        {
            MemberId = memberId;
            MemberType = memberType;
            SpawnPoint = spawnPoint;
        }
    }
}