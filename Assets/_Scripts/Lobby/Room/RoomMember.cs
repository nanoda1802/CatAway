using System;
using _Scripts.Lobby.UI.Messages.Member;
using _Scripts.Lobby.UI.Messages.Room;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Lobby.Room
{
    public class RoomMember : NetworkBehaviour
    {
        private RoomSyncer _assignedRoom;

        private readonly NetworkVariable<bool> _sharedReadyState 
            = new (writePerm : NetworkVariableWritePermission.Owner);
        
        private IPublisher<SwitchStartMessage> _startSwitchPub;
        private IPublisher<SwitchReadyRespond> _readyRespondPub;
        private IDisposable _subscription;

        public bool IsHostMember => IsOwnedByServer;
        public bool IsReady => _sharedReadyState.Value; 
        public Vector3 CurPos => transform.position;

        [Inject]
        private void Construct(
            IPublisher<SwitchStartMessage> startSwitchPub,
            IPublisher<SwitchReadyRespond> readyRespondPub,
            ISubscriber<SwitchReadyRequest> readyRequestSub)
        {
            _startSwitchPub = startSwitchPub;
            _readyRespondPub = readyRespondPub;
            
           _subscription = readyRequestSub.Subscribe(req =>
           {
               if (req.CancelReady) SetReadyState(false);
               else SetReadyState(!IsReady);
           });
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _sharedReadyState.Value = IsHostMember; // 호스트가 아닌 경우 이미 false라서 dirty하지 않은 바람에 이벤트가 작동하지 않아유
                _readyRespondPub.Publish(new SwitchReadyRespond(this.OwnerClientId, IsReady, IsOwner)); // 첨에 한 번 명시적으로 보내주기
            }
            
            if (!IsHostMember) _sharedReadyState.OnValueChanged = OnReadyStateChanged;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _sharedReadyState.OnValueChanged = null;
            _subscription?.Dispose();
            
            base.OnNetworkDespawn();
        }

        public RoomMember AssignTo(RoomSyncer room)
        {
            _assignedRoom = room;
            return this;
        }

        private void SetReadyState(bool isReady)
        {
            if (!IsOwner || IsHostMember) return;
            
            _sharedReadyState.Value = isReady;
        }

        private void OnReadyStateChanged(bool prevState, bool newState)
        {
            if (prevState == newState) return;
            if (IsServer) _startSwitchPub.Publish(new SwitchStartMessage(_assignedRoom.CanStartStage));
            
            _readyRespondPub.Publish(new SwitchReadyRespond(this.OwnerClientId, newState, IsOwner));

        }

        [Rpc(SendTo.Owner)]
        public void InitReadyStateRpc()  // WritePerm은 딱 하나 Only 부여할 수 있어서... 갱신 주기가 더 잦은 owner한테 주고, server에서 필요하면 rpc 쏘도록...
        {
            _sharedReadyState.Value = IsHostMember;
        }
    }
}