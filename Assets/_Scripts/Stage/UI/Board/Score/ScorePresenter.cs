using System;
using _Scripts.Stage.Data;
using _Scripts.Stage.UI.Board.Order;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board.Score
{
    public class ScorePresenter : NetworkBehaviour, ITeamMessage
    {
        [SF] private Team team;

        // [임시] 배율 데이터로 만들기
        [SF] private float deductRatio = -0.5f;
        [SF] private float comboMultiplier = 0.1f;

        private IPublisher<ScoreMessage> _pub;
        
        private int _curScore;
        private int _curComboCount;
        
        private IDisposable _subscription;

        public Team Team => team;
        
        [Inject]
        private void Construct(
            IPublisher<ScorePresenter> presenterPub,
            IPublisher<ScoreMessage> pub,
            IBufferedSubscriber<PublishRequestMessage> requestSub)
        {
            _pub = pub;
            
            presenterPub.Publish(this);
            
            _subscription = requestSub.Subscribe(msg =>
            {
                if (!msg.IsRequest(this)) return;
                presenterPub.Publish(this);
            });
        }

        public override void OnNetworkPreDespawn()
        {
            _subscription?.Dispose();
            base.OnNetworkPreDespawn();
        }

        public void UpdateScore(int baseScore, float remainingRatio)
        {
            var point = CalculatePoint(baseScore, remainingRatio);

            _curScore = (_curScore + point < 0) 
                ? 0 
                : _curScore + point;
            
            _curComboCount = (point < 0)
                ? 0
                : _curComboCount + 1;
            
            UpdateRpc(new ScoreMessage(team, _curScore, _curComboCount, point > 0));
        }

        private int CalculatePoint(int score, float ratio)
        {
            return (ratio < 0) 
                ? (int) (score * deductRatio)
                : (int) (score * (1 + ratio + (_curComboCount * comboMultiplier)));
        }
        
        [Rpc(SendTo.Everyone)]
        private void UpdateRpc(ScoreMessage message)
        {
            // board.Apply(packet);
            _pub.Publish(message);
        }
    }
}