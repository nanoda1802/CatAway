using Unity.Netcode;

namespace _Scripts.Stage.UI.Board.Order
{
    public struct RemoveOrderMessage : ITeamMessage, INetworkSerializable
    {
        private Team _team;
        private int _targetId;

        public Team Team => _team;
        public int TargetId => _targetId;
        
        public RemoveOrderMessage(Team team, int targetId)
        {
            _team = team;
            _targetId = targetId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _team);
            serializer.SerializeValue(ref _targetId);
        }
    }
}