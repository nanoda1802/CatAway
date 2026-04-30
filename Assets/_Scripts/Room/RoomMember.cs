using _Scripts.Room._Enums;
using _Scripts.Room._Messages;
using _Scripts.Shared._Data;
using _Scripts.Shared._Messages;
using MessagePipe;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

namespace _Scripts.Room
{
    public class RoomMember : NetworkBehaviour
    {
        // Components
        private NetworkTransform _netTr;
        private SkinnedMeshRenderer _renderer;
        private Animator _animator;
        // Dependency
        private AvatarData _avatarData;
        private PlayerStatus _playerStatus;
        private IPublisher<ShowRoomMemberCardMessage> _showCardPub;
        private IPublisher<HideMemberCardMessage> _hideCardPub;
        private IPublisher<MoveMemberCardMessage> _moveCardPub;
        private IPublisher<SwitchReadyRespond> _readyRespondPub;
        private IPublisher<UpdateMemberNameMessage> _updateNamePub;
        // Network
        private readonly NetworkVariable<int> _sharedAvatarIndex 
            = new (-1, writePerm : NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> _sharedReadyState 
            = new (writePerm : NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<FixedString32Bytes> _sharedNickname
            = new (writePerm : NetworkVariableWritePermission.Owner);
        // Caching
        private RoomMemberSyncer _memberSyncer;
        private MaterialPropertyBlock _matPropBlock;
        private readonly int _dragAnimHash = Animator.StringToHash("Drag");
        // Property
        public bool IsHostMember => IsOwnedByServer;
        public bool IsReady => _sharedReadyState.Value; 
        public int AvatarIndex => _sharedAvatarIndex.Value;
        public string Nickname => _sharedNickname.Value.Value;

        public Vector3 CurPos
        {
            get => transform.position;
            set => transform.position = new Vector3(value.x, value.y, CurPos.z);
        }

        [Inject]
        private void Construct(
            AvatarData avatarData,
            PlayerStatus playerStatus,
            IPublisher<ShowRoomMemberCardMessage> showCardPub,
            IPublisher<HideMemberCardMessage> hideCardPub,
            IPublisher<MoveMemberCardMessage> moveCardPub,
            IPublisher<SwitchReadyRespond> readyRespondPub,
            ISubscriber<SwitchReadyRequest> readyRequestSub,
            IPublisher<UpdateMemberNameMessage> updateNamePub,
            ISubscriber<RenameMessage> renameSub,
            ISubscriber<AvatarMessage> avatarSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _netTr = GetComponent<NetworkTransform>();
            _animator = GetComponentInChildren<Animator>();
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _matPropBlock = new MaterialPropertyBlock();
            
            _avatarData = avatarData;
            _playerStatus = playerStatus;
            
            _showCardPub = showCardPub;
            _hideCardPub = hideCardPub;
            _moveCardPub = moveCardPub;
            _readyRespondPub = readyRespondPub;
            _updateNamePub = updateNamePub;
            
           readyRequestSub
               .Subscribe(SetReadyState)
               .AddTo(disposableBagBuilder);
           
           renameSub
               .Subscribe(SetNickname)
               .AddTo(disposableBagBuilder);
           
           avatarSub
               .Subscribe(SetAvatar)
               .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _sharedNickname.Value = string.IsNullOrEmpty(_playerStatus.Nickname)? $"Player{OwnerClientId}" : _playerStatus.Nickname;
                _sharedAvatarIndex.Value = _playerStatus.AvatarIndex;
                _sharedReadyState.Value = IsHostMember;
            }

            if (!IsHostMember) _sharedReadyState.OnValueChanged += OnReadyStateChanged;
            _sharedAvatarIndex.OnValueChanged += OnAvatarIndexChanged;
            _sharedNickname.OnValueChanged += OnNicknameChanged;
            
            base.OnNetworkSpawn();
        }

        protected override void OnNetworkPostSpawn()
        {
            ShowCard();
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, AvatarIndex);
            _updateNamePub.Publish(new UpdateMemberNameMessage(this.OwnerClientId, Nickname));
            if (!IsHostMember) _readyRespondPub.Publish(new SwitchReadyRespond(this.OwnerClientId, IsReady, IsOwner));
            
            base.OnNetworkPostSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                HideCardRpc();
            }
            
            _sharedReadyState.OnValueChanged = null;
            _sharedAvatarIndex.OnValueChanged = null;
            _sharedNickname.OnValueChanged = null;
            
            base.OnNetworkDespawn();
        }

        public RoomMember AssignTo(RoomMemberSyncer syncer)
        {
            _memberSyncer = syncer;
            return this;
        }

        public void SetNickname(RenameMessage msg)
        {
            if (!IsOwner) return;
            _sharedNickname.Value = msg.Nickname;
        }

        private void OnNicknameChanged(FixedString32Bytes prev, FixedString32Bytes cur)
        {
            if (prev == cur) return;
            _updateNamePub.Publish(new UpdateMemberNameMessage(this.OwnerClientId, cur));
        }

        public void SetAvatar(AvatarMessage msg)
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

            var res = new SwitchReadyRespond(this.OwnerClientId, newState, IsOwner);
            _readyRespondPub.Publish(res);

        }

        public RoomMember StartDrag()
        {
            transform.rotation = Quaternion.identity;
            _animator.SetBool(_dragAnimHash, true);
            return this;
        }

        public void MoveTo(Vector3 pos, Quaternion rot)
        {
            _netTr.Teleport(pos, rot,Vector3.one);
            _animator.SetBool(_dragAnimHash, false);
            MoveCardRpc(pos,rot);
        }

        private void ShowCard()
        {
            var iconType = MemberIconType.NonReady;
            
            if (IsHostMember) 
                iconType = MemberIconType.Host;
            else if (IsReady) 
                iconType = MemberIconType.Ready;
            
            var msg = new ShowRoomMemberCardMessage(
                this.OwnerClientId,
                Nickname,
                iconType,
                CurPos
            );
            
            _showCardPub.Publish(msg);
        }

        [Rpc(SendTo.Owner)]
        public void InitReadyStateRpc() 
        {
            _sharedReadyState.Value = IsHostMember;
        }
        
        [Rpc(SendTo.Everyone)]
        private void HideCardRpc()
        {
            var msg = new HideMemberCardMessage(this.OwnerClientId);
            _hideCardPub.Publish(msg);
        }
        
        [Rpc(SendTo.Everyone)]
        private void MoveCardRpc(Vector3 newPos, Quaternion newRot) 
        {
            var msg = new MoveMemberCardMessage(
                this.OwnerClientId,
                newPos,
                newRot
            );
            
            _moveCardPub.Publish(msg);
        }
    }
}