using _Scripts.Scene_Stage.Enums;
using _Scripts.Scene_Stage.UI.Board;
using Unity.Netcode;

namespace _Scripts.Messages.StageResult
{
    public struct ResultBoardMessage : ITeamMessage, INetworkSerializable
    {
        private Team _team;
        private bool _isWin;
        private int _score;
        private int _bestScore;
        private float _deliveredRatio;
        
        public Team Team => _team;
        public bool IsWin => _isWin;
        public int Score => _score;
        public int BestCombo => _bestScore;
        public float DeliveredRatio => _deliveredRatio;
        public int Income => (int) (Score * (1 + BestCombo * 0.1f) * (1 + DeliveredRatio));

        public ResultBoardMessage(
            Team team,
            bool isWin,
            int score,
            int bestCombo,
            float deliveredRatio)
        {
            _team = team;
            _isWin = isWin;
            _score = score;
            _bestScore = bestCombo;
            _deliveredRatio = deliveredRatio;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _team);
            serializer.SerializeValue(ref _isWin);
            serializer.SerializeValue(ref _score);
            serializer.SerializeValue(ref _bestScore);
            serializer.SerializeValue(ref _deliveredRatio);
        }
    }
}