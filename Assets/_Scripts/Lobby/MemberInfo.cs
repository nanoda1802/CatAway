using Unity.Netcode;

namespace _Scripts.Lobby
{
    public class MemberInfo : INetworkSerializable
    {
        private ulong _clientId;
        private int _avatarIndex;

        public ulong ClientId => _clientId;
        public int AvatarIndex => _avatarIndex;

        public MemberInfo(ulong clientId, int avatarIndex)
        {
            _clientId = clientId;
            _avatarIndex = avatarIndex;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _clientId);
            serializer.SerializeValue(ref _avatarIndex);
        }
    }
}