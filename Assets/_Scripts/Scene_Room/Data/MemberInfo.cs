using _Scripts.Scene_Stage.Enums;
using Unity.Collections;
using Unity.Netcode;

namespace _Scripts.Scene_Room.Data
{
    public class MemberInfo : INetworkSerializable
    {
        private ulong _clientId;
        private Team _team;
        public ulong ClientId => _clientId;
        public Team Team => _team;

        public MemberInfo(ulong clientId, Team team = Team.None)
        {
            _clientId = clientId;
            _team = team;
        }
        
        public void Apply(Team team)
        {
            _team = team;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _clientId);
            serializer.SerializeValue(ref _team);
        }
    }
}