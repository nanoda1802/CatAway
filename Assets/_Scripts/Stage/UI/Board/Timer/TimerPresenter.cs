using _Scripts.Stage.UI.Board.Order;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board.Timer
{
    public class TimerPresenter : NetworkBehaviour, INetworkUpdateSystem
    {
        private StageData _stageData;
        
        private float _remainingTime;
        
        [SF] private float dirtyThreshold = 0.005f;

        private IPublisher<float> _pub;
        
        private readonly NetworkVariable<float> _sharedTimer = new();

        [Inject]
        private void Construct(
            StageData stageData,
            IPublisher<float> pub)
        {
            _stageData = stageData;
            _pub = pub;
        }

        public override void OnNetworkSpawn()
        {
            _sharedTimer.CheckExceedsDirtinessThreshold = CheckDirtiness;
            _sharedTimer.OnValueChanged = OnTimerChanged;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            _sharedTimer.CheckExceedsDirtinessThreshold = null;
            _sharedTimer.OnValueChanged = null;
            
            base.OnNetworkPreDespawn();
        }

        public override void OnNetworkDespawn() // [임시]
        {
            if (IsServer) StopTimer();
            
            base.OnNetworkDespawn();
        }
        
        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            _remainingTime -= Time.deltaTime;
            _sharedTimer.Value = _remainingTime;
        }

        public void BeginTimer()
        {
            _remainingTime = _stageData.Duration;
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void StopTimer()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void OnTimerChanged(float prev, float cur)
        {
            // board.Apply(next);
            _pub.Publish(cur);
        }

        private bool CheckDirtiness(in float prev, in float cur)
        {
            return Mathf.Abs(prev - cur) > dirtyThreshold;
        }
    }
}