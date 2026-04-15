using Unity.Netcode;

namespace _Scripts.Stage._Data
{
    public struct PlayerSpawnPacket : INetworkSerializable
    {
        private bool _isRespawn;
        
        public bool IsRespawn => _isRespawn;

        public PlayerSpawnPacket(bool isRespawn)
        {
            _isRespawn = isRespawn;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _isRespawn);
        }
    }
}