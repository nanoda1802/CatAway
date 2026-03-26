using _Scripts.Scene_Stage.Enums;
using UnityEngine;

namespace _Scripts.Messages.StageResult
{
    public readonly struct ShowResultMemberCardMessage
    {
        public ulong MemberId { get; }
        public Team Team { get; }
        public string Name { get; }
        public Vector3 SpawnPoint { get; }
        public bool IsAce { get; }

        public ShowResultMemberCardMessage(
            ulong memberId,
            Team team,
            string name,
            Vector3 spawnPoint,
            bool isAce)
        {
            MemberId = memberId;
            Team = team;
            Name = name;
            SpawnPoint = spawnPoint;
            IsAce = isAce;
        }
    }
}