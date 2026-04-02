using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Scene_Stage.Data
{
    public struct PlayerSpawnPacket : INetworkSerializable
    {
        // private int _avatarIdx;
        private bool _isRespawn;
        
        // public int AvatarIdx => _avatarIdx;
        public bool IsRespawn => _isRespawn;

        public PlayerSpawnPacket(bool isRespawn)
        {
            // _avatarIdx = avatarIdx;
            _isRespawn = isRespawn;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // serializer.SerializeValue(ref _avatarIdx);
            serializer.SerializeValue(ref _isRespawn);
        }
    }
}