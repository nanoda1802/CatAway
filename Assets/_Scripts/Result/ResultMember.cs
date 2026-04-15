using _Scripts.Result._Messages;
using _Scripts.Shared._Data;
using _Scripts.Shared._Messages;
using _Scripts.Stage._Enums;
using MessagePipe;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

namespace _Scripts.Result
{
    public class ResultMember : NetworkBehaviour
    {
         // Components
        private NetworkTransform _netTr;
        private SkinnedMeshRenderer _renderer;
        // Dependency
        private RoomStatus _roomStatus;
        private AvatarData _avatarData;
        private PlayerStatus _playerStatus;
        private IPublisher<ShowResultMemberCardMessage> _showCardPub;
        private IPublisher<HideMemberCardMessage> _hideCardPub;
        private IPublisher<MoveMemberCardMessage> _moveCardPub;
        private IPublisher<UpdateMemberNameMessage> _updateNamePub;
        // Network
        private readonly NetworkVariable<int> _sharedAvatarIndex 
            = new (-1, writePerm : NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<FixedString32Bytes> _sharedNickname
            = new (writePerm : NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<Team> _sharedTeam
            = new (writePerm : NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<ulong> _sharedAceId
            = new (writePerm : NetworkVariableWritePermission.Server);
        // Caching
        private MaterialPropertyBlock _matPropBlock;
        // Property
        private Vector3 CurPos => transform.position;
        private int AvatarIndex => _sharedAvatarIndex.Value;
        private string Nickname => _sharedNickname.Value.Value;
        private ulong AceId => _sharedAceId.Value;
        private Team CurTeam  => _sharedTeam.Value;

        [Inject]
        private void Construct(
            RoomStatus roomStatus,
            AvatarData avatarData,
            PlayerStatus playerStatus,
            IPublisher<ShowResultMemberCardMessage> showCardPub,
            IPublisher<HideMemberCardMessage> hideCardPub,
            IPublisher<MoveMemberCardMessage> moveCardPub,
            IPublisher<UpdateMemberNameMessage> updateNamePub,
            ISubscriber<AvatarMessage> avatarSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _netTr = GetComponent<NetworkTransform>();
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _matPropBlock = new MaterialPropertyBlock();
            
            _roomStatus = roomStatus;
            _avatarData = avatarData;
            _playerStatus = playerStatus;
            
            _showCardPub = showCardPub;
            _hideCardPub = hideCardPub;
            _moveCardPub = moveCardPub;
            _updateNamePub = updateNamePub;
            
            avatarSub
               .Subscribe(SetAvatar)
               .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _sharedAvatarIndex.Value = _playerStatus.AvatarIndex;
                _sharedNickname.Value = string.IsNullOrEmpty(_playerStatus.Nickname)? $"Player{OwnerClientId}" : _playerStatus.Nickname;
            }

            if (IsServer)
            {
                _sharedTeam.Value = _roomStatus.GetMemberById(OwnerClientId).Team;
                _sharedAceId.Value = _roomStatus.StageResult.AcePlayerId;
            }

            _sharedAvatarIndex.OnValueChanged += OnAvatarIndexChanged;
            _sharedNickname.OnValueChanged += OnNicknameChanged;
            
            base.OnNetworkSpawn();
        }

        protected override void OnNetworkPostSpawn()
        {
            ShowCard();
            
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, AvatarIndex); 
            
            base.OnNetworkPostSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            if (IsServer)
            {
                HideCardRpc();
            }

            _sharedAvatarIndex.OnValueChanged = null;
            _sharedNickname.OnValueChanged = null;
            
            base.OnNetworkDespawn();
        }

        public void RePosition(Vector3 newPos, Quaternion newRot)
        {
            if (!IsServer) return;
            _netTr.Teleport(newPos, newRot, Vector3.one);
            MoveCardRpc(newPos, newRot);
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

        private void OnNicknameChanged(FixedString32Bytes prev, FixedString32Bytes cur)
        {
            if (prev == cur) return;
            var msg = new UpdateMemberNameMessage(OwnerClientId, cur);
            _updateNamePub.Publish(msg);
        }

        private void ShowCard()
        {
            var msg = new ShowResultMemberCardMessage(
                    this.OwnerClientId,
                    CurTeam,
                    Nickname,
                    CurPos,
                    AceId == OwnerClientId
                );
            
            _showCardPub.Publish(msg);
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