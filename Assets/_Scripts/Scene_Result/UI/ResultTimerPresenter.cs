using _Scripts.Messages.StageResult;
using _Scripts.Scene_Result.Data;
using MessagePipe;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using VContainer;

namespace _Scripts.Scene_Result.UI
{
    public class ResultTimerPresenter : NetworkBehaviour, INetworkUpdateSystem
    {
        private ResultViewData _viewData;
        
        private float _startTime;
        
        private IPublisher<float> _timerPub;
        private IPublisher<LoadSceneMessage> _loadScenePub;

        [Inject]
        private void Construct(
            ResultViewData viewData,
            IPublisher<float> timerPub,
            IPublisher<LoadSceneMessage> loadScenePub,
            ISubscriber<StartResultMessage> startSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _viewData = viewData;
            _timerPub = timerPub;
            _loadScenePub = loadScenePub;
            
            startSub
                .Subscribe(BeginTimer)
                .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkDespawn()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);

            base.OnNetworkDespawn();
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            var elapsedTime = (NetworkManager.ServerTime.TimeAsFloat - _startTime);
            var remainingTime = _viewData.Duration - elapsedTime;
            _timerPub.Publish(remainingTime);
            
            if (remainingTime <= 0) StopTimer();
        }
        
        private void BeginTimer(StartResultMessage msg)
        {
            if (!IsServer) return;
            BeginTimerRpc(NetworkManager.ServerTime.TimeAsFloat);
        }
        
        public void BeginSpareTimer()
        {
            // elapsedTime이 spareTime과 같아지기 위한 startTime!
            _startTime = NetworkManager.ServerTime.TimeAsFloat - (_viewData.Duration - _viewData.SpareTimeAfterSkip);
        }

        public void StopTimer()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            var msg = new LoadSceneMessage("Room", LoadSceneMode.Single);
            _loadScenePub.Publish(msg);
        }
        
        [Rpc(SendTo.Everyone)]
        private void BeginTimerRpc(float startTime)
        {
            _startTime = startTime;
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }
    }
}