using Unity.Collections;
using UnityEngine;

namespace _Scripts._Messages.Stage
{
    public readonly struct PlayerDespawnMessage
    {
        public ulong TagetId { get; }
        public Vector3 RespawnPoint { get; }
        public float DespawnTime { get; }

        public PlayerDespawnMessage(
            ulong tagetId,
            Vector3 respawnPoint,
            float despawnTime)
        {
            TagetId = tagetId;
            RespawnPoint = respawnPoint;
            DespawnTime = despawnTime;
        }
    }
}