using System.Collections.Generic;
using System.Linq;
using _Scripts.Result._Data;
using _Scripts.Result._Messages;
using _Scripts.Shared._Data;
using MessagePipe;
using Unity.Netcode;
using VContainer;

namespace _Scripts.Result.UI
{
    public class SkipVotePresenter : NetworkBehaviour
    {
        // Dependency
        private RoomStatus _roomStatus;
        private ResultTimerPresenter _resultTimerPresenter;
        private IPublisher<SkipRespond> _skipPub;
        // Caching
        private bool _hasSkipped;
        private readonly List<ulong> _agreedIdList = new List<ulong>();
        // Network
        private readonly NetworkVariable<SkipVoteStatus> _sharedVoteStatus = new();
        // Property
        private int CurAgreements => _sharedVoteStatus.Value.Agreements;
        private int CurVoterCount => _sharedVoteStatus.Value.VoterCount;
        private bool AllAgreed => _sharedVoteStatus.Value.AllAgreed;

        [Inject]
        private void Construct(
            RoomStatus roomStatus,
            IPublisher<SkipRespond> skipPub,
            ISubscriber<SkipRequest> skipSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _resultTimerPresenter = transform.parent.GetComponentInChildren<ResultTimerPresenter>();
            
            _roomStatus = roomStatus;
            _skipPub = skipPub;
            
            skipSub
                .Subscribe(Vote)
                .AddTo(disposableBagBuilder);
        }


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.OnConnectionEvent += OnConnection;
                _sharedVoteStatus.Value = new SkipVoteStatus(0, _roomStatus.ActiveMembers.Count());
            }
            
            _sharedVoteStatus.OnValueChanged += OnAgreementChanged;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) NetworkManager.OnConnectionEvent -= OnConnection;
            
            _sharedVoteStatus.OnValueChanged -= OnAgreementChanged;
            
            base.OnNetworkDespawn();
        }

        private void Vote(SkipRequest req)
        {
            if (_hasSkipped) return;
            
            AddAgreementRpc(NetworkManager.LocalClientId);
            _hasSkipped = true;
        }

        private void OnAgreementChanged(SkipVoteStatus prev, SkipVoteStatus cur)
        {
            if (prev.Equals(cur)) return;
            var msg = new SkipRespond(CurAgreements, CurVoterCount);
            _skipPub.Publish(msg);
        }
        
        private void OnConnection(NetworkManager netMgr, ConnectionEventData eventData)
        {
            if (!IsServer) return;
            if (eventData.EventType != ConnectionEvent.ClientDisconnected) return;

            bool removed = _agreedIdList.Remove(eventData.ClientId);
            // if (removed)
            // {
            //     _sharedVoteStatus.Value = new SkipVoteStatus(_agreedIdList.Count, CurVoterCount - 1);
            // }
            _sharedVoteStatus.Value = new SkipVoteStatus(_agreedIdList.Count, CurVoterCount - 1);
            
            if (AllAgreed)
            {
                SkipRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void AddAgreementRpc(ulong voterId)
        {
            if (_agreedIdList.Contains(voterId)) return;
            
            _agreedIdList.Add(voterId);
            _sharedVoteStatus.Value = new SkipVoteStatus(_agreedIdList.Count, CurVoterCount);
            
            if (AllAgreed)
            {
                SkipRpc();
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SkipRpc()
        {
            _resultTimerPresenter.BeginSpareTimer();
        }
    }
}