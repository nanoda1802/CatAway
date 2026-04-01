using _Scripts._Shared;
using _Scripts.Messages.Stage;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Enums;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace _Scripts.Scene_Stage.UI.Board.Timer
{
    public class TimerPresenter : NetworkBehaviour, INetworkUpdateSystem
    {
        private StageData _stageData;
        private StageHub _stageHub;
        
        private float _startTime;
        
        private IPublisher<float> _timerPub;
        private IPublisher<EndStageMessage> _endPub;
        
        [Inject]
        private void Construct(
            StageData stageData,
            StageHub  stageHub,
            IPublisher<float> timerPub,
            IPublisher<EndStageMessage> endPub,
            ISubscriber<StartStageMessage> startSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _stageData = stageData;
            _stageHub = stageHub;
            
            _timerPub = timerPub;
            _endPub = endPub;

            startSub
                .Subscribe(BeginTimer)
                .AddTo(disposableBagBuilder);
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!IsSpawned) return;
            
            var elapsedTime = (NetworkManager.ServerTime.TimeAsFloat - _startTime);
            var remainingTime = _stageData.Duration - elapsedTime;
            _timerPub.Publish(remainingTime);
            
            if (remainingTime <= 0) StopTimer();
        }
        
        private void BeginTimer(StartStageMessage msg)
        {
            if (!IsServer) return;
            BeginTimerRpc(NetworkManager.ServerTime.TimeAsFloat);
        }

        private void StopTimer()
        {
            if (IsServer)
            {
                var cuePresenter = _stageHub.FetchCuePresenter();
                cuePresenter?.DisplayCue(CueType.End).Forget();
            }
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            var msg = new EndStageMessage();
            _endPub.Publish(msg);
        }

        [Rpc(SendTo.Everyone)]
        private void BeginTimerRpc(float startTime)
        {
            _startTime = startTime;
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }
    }
}