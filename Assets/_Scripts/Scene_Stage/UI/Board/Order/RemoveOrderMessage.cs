using _Scripts.Scene_Stage.Enums;
using Unity.Netcode;

namespace _Scripts.Scene_Stage.UI.Board.Order
{
    public struct RemoveOrderMessage : ITeamMessage, INetworkSerializable
    {
        private Team _team;
        private int _targetId;
        private bool _isTimeout;

        public Team Team => _team;
        public int TargetId => _targetId;
        public bool IsTimeout => _isTimeout;
        
        public RemoveOrderMessage(Team team, int targetId,  bool isTimeout)
        {
            _team = team;
            _targetId = targetId;
            _isTimeout = isTimeout;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _team);
            serializer.SerializeValue(ref _targetId);
            serializer.SerializeValue(ref _isTimeout);
        }
    }
}