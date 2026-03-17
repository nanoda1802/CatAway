using _Scripts.Stage.Data;
using Unity.Netcode;

namespace _Scripts.Stage.UI.Board.Score
{
    public struct ScoreMessage : ITeamMessage, INetworkSerializable
    {
        private Team _team;
        private int _scoreValue;
        private int _comboValue;
        private bool _hasPoint;

        public Team Team => _team;
        public int ScoreValue => _scoreValue;
        public int ComboValue => _comboValue;
        public bool HasPoint => _hasPoint;
        
        public ScoreMessage(
            Team team,
            int scoreValue,
            int comboValue,
            bool hasPoint)
        {
            _team = team;
            _scoreValue = scoreValue;
            _comboValue = comboValue;
            _hasPoint = hasPoint;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _team);
            serializer.SerializeValue(ref _scoreValue);
            serializer.SerializeValue(ref _comboValue);
            serializer.SerializeValue(ref _hasPoint);
        }
    }
}