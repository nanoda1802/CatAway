using System;
using _Scripts.Result._Messages;
using _Scripts.Shared._Data;
using _Scripts.Stage._Data;
using MessagePipe;
using Unity.Netcode;
using VContainer;

namespace _Scripts.Result.UI
{
    public class ResultBoardPresenter : NetworkBehaviour
    {
        private StageResultInfo _resultInfo;
        
        private IPublisher<ResultBoardMessage> _resultPub;

        [Inject]
        private void Construct(
            RoomStatus roomStatus,
            IPublisher<ResultBoardMessage> resultPub,
            ISubscriber<StartResultMessage> startSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _resultInfo = roomStatus.StageResult;
            _resultPub = resultPub;
            
            startSub
                .Subscribe(PresentCards)
                .AddTo(disposableBagBuilder);
        }

        private void PresentCards(StartResultMessage startMsg)
        {
            int winScore = Int32.MinValue;
            
            foreach (var team in _resultInfo.ResultByTeam)
            {
                if (winScore > team.CurScore) continue;
                winScore = team.CurScore;
            }

            foreach (var team in _resultInfo.ResultByTeam)
            {
                var msg = new ResultBoardMessage(
                        team.Team,
                        winScore == team.CurScore,
                        team.CurScore,
                        team.BestCombo,
                        team.DeliverRatio
                    );
                
                SendMessageRpc(msg);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SendMessageRpc(ResultBoardMessage msg)
        {
            _resultPub.Publish(msg);
        }
    }
}