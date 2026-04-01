using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Enums;
using _Scripts.Stage;
using MessagePipe;
using Unity.Netcode;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.UI.Board.Score
{
    public class ScorePresenter : NetworkBehaviour, ITeamMessage
    {
        [SF] private Team team;

        // [임시] 배율 데이터로 만들기
        [SF] private float deductRatio = -0.5f;
        [SF] private float comboMultiplier = 0.1f;

        private StageStatus _stageStatus;
        private IPublisher<ScoreMessage> _msgPub;
        
        public Team Team => team;
        
        [Inject]
        private void Construct(
            StageStatus stageStatus,
            IPublisher<ScorePresenter> presenterPub,
            IPublisher<ScoreMessage> msgPub,
            IBufferedSubscriber<HubCallMessage> requestSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _stageStatus = stageStatus;
            _msgPub = msgPub;
            
            presenterPub.Publish(this);
            
            requestSub.Subscribe(msg =>
                {
                    if (!msg.IsRequest(this)) return;
                    presenterPub.Publish(this);
                })
                .AddTo(disposableBagBuilder);
        }

        public int UpdateScore(int baseScore, float remainingRatio, ulong clientId = ulong.MaxValue)
        {
            int point = CalculatePoint(baseScore, remainingRatio);

            (int curScore, int curCombo) = _stageStatus.RecordCurScore(team, point, clientId);
            
            UpdateRpc(new ScoreMessage(team, curScore, curCombo, point > 0));

            return point;
        }

        private int CalculatePoint(int score, float ratio)
        {
            return (ratio < 0) 
                ? (int) (score * deductRatio)
                : (int) (score * (1 + ratio + (_stageStatus.GetCurCombo(team) * comboMultiplier)));
        }
        
        [Rpc(SendTo.Everyone)]
        private void UpdateRpc(ScoreMessage message)
        {
            _msgPub.Publish(message);
        }
    }
}