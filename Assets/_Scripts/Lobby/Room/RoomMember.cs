using System;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Lobby.Room
{
    public class RoomMember : NetworkBehaviour
    {
        // Components
        private SkinnedMeshRenderer _renderer;
        // Dependency
        private AvatarData _avatarData;
        private RoomSyncer _assignedRoom;
        private IPublisher<SwitchStartMessage> _startSwitchPub;
        private IPublisher<SwitchReadyRespond> _readyRespondPub;
        // Network
        private readonly NetworkVariable<int> _sharedAvatarIndex 
            = new (-1, writePerm : NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> _sharedReadyState 
            = new (writePerm : NetworkVariableWritePermission.Owner);
        // Caching
        private MaterialPropertyBlock _matPropBlock;
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        // Property
        public bool IsHostMember => IsOwnedByServer;
        public bool IsReady => _sharedReadyState.Value; 
        public Vector3 CurPos => transform.position;
        public int AvatarIndex => _sharedAvatarIndex.Value;

        [Inject]
        private void Construct(
            AvatarData avatarData,
            IPublisher<SwitchStartMessage> startSwitchPub,
            IPublisher<SwitchReadyRespond> readyRespondPub,
            ISubscriber<SwitchReadyRequest> readyRequestSub,
            ISubscriber<AvatarMessage> avatarSub)
        {
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _matPropBlock = new MaterialPropertyBlock();
            
            _avatarData = avatarData;
            _startSwitchPub = startSwitchPub;
            _readyRespondPub = readyRespondPub;
            
           readyRequestSub
               .Subscribe(SetReadyState)
               .AddTo(_disposableBagBuilder);
           
           avatarSub
               .Subscribe(SetAvatar)
               .AddTo(_disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _sharedReadyState.Value = IsHostMember; // 호스트가 아닌 경우 이미 false라서 dirty하지 않은 바람에 이벤트가 작동하지 않아유
                _readyRespondPub.Publish(new SwitchReadyRespond(this.OwnerClientId, IsReady, IsOwner)); // 첨에 한 번 명시적으로 보내주기
            }
            
            if (!IsHostMember) _sharedReadyState.OnValueChanged = OnReadyStateChanged;
            _sharedAvatarIndex.OnValueChanged = OnAvatarIndexChanged;
            
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, _sharedAvatarIndex.Value); // 여기도 명시적으로 한 번 바꿔주기
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _sharedReadyState.OnValueChanged = null;
            _sharedAvatarIndex.OnValueChanged = null;
            
            _disposableBagBuilder?.Build().Dispose();
            
            base.OnNetworkDespawn();
        }

        public RoomMember AssignTo(RoomSyncer room)
        {
            _assignedRoom = room;
            return this;
        }

        private void SetAvatar(AvatarMessage msg)
        {
            if (!IsOwner) return;

            _sharedAvatarIndex.Value = msg.AvatarIndex;
        }

        private void OnAvatarIndexChanged(int prevIdx, int newIdx)
        {
            if (newIdx == prevIdx) return;
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, newIdx);
        }

        private void SetReadyState(SwitchReadyRequest req)
        {
            if (!IsOwner || IsHostMember) return;

            var newReadyState = req.CancelReady ? false : !IsReady;
            _sharedReadyState.Value = newReadyState;
        }

        private void OnReadyStateChanged(bool prevState, bool newState)
        {
            if (prevState == newState) return;
            if (IsServer) _startSwitchPub.Publish(new SwitchStartMessage(_assignedRoom.CanStartStage));

            var res = new SwitchReadyRespond(this.OwnerClientId, newState, IsOwner);
            _readyRespondPub.Publish(res);

        }

        [Rpc(SendTo.Owner)]
        public void InitReadyStateRpc()  // WritePerm은 딱 하나 Only 부여할 수 있어서... 갱신 주기가 더 잦은 owner한테 주고, server에서 필요하면 rpc 쏘도록...
        {
            _sharedReadyState.Value = IsHostMember;
        }
    }
}