using _Scripts.Stage._Enums;

namespace _Scripts.Stage._Data
{
    public class TeamStatus
    {
        private int _curScore;
        private int _curCombo;
        private int _deliveredOrderCount;

        public Team Team { get; }
        public int TotalOrderCount;
        public int CurScore
        {
            get => _curScore;
            set
            {
                _curScore = (_curScore + value < 0) ? 0 : _curScore + value;
                if (value >= 0) _deliveredOrderCount += 1;
            } 
        }
        public int CurCombo
        {
            get => _curCombo;
            set
            {
                _curCombo = (value < 0) ? 0 : _curCombo + 1;
                if (_curCombo > BestCombo) BestCombo = _curCombo;
            }
        }
        public int BestCombo { get; private set; }
        public float DeliverRatio => (float) _deliveredOrderCount / TotalOrderCount;

        public TeamStatus(Team team)
        {
            Team = team;
            _curScore = 0;
            BestCombo = _curCombo = 0;
            _deliveredOrderCount = 0;
        }
    }
}