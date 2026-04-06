using System;
using System.Collections.Generic;
using _Scripts.Messages.StageResult;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Stage.Data;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace _Scripts.Scene_Result.UI
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
            
            Debug.Log($"<color=cyan>[resultBoardPresenter]</color> ace? {_resultInfo.AcePlayerId} / team? {_resultInfo.ResultByTeam != null}");
            
            foreach (var team in _resultInfo.ResultByTeam)
            {
                Debug.Log($"<color=cyan>[resultBoardPresenter]</color> team? {team != null} / which? {team?.Team}");
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