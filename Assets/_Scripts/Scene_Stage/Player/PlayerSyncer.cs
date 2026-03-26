using _Scripts._Shared.Data;
using _Scripts.Messages.Stage;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Scene_Stage.Player
{
    public class PlayerSyncer : NetworkBehaviour
    {
        // Components
        private SkinnedMeshRenderer _renderer;
        // Data
        private AvatarData _avatarData;
        // Dependency
        private PlayerInput _inputMap;
        private PlayerStatus _playerStatus;
        // Caching
        private MaterialPropertyBlock _matPropBlock;
        
        private readonly NetworkVariable<int> _sharedAvatarIndex 
            = new (-1, writePerm : NetworkVariableWritePermission.Owner);

        [Inject]
        private void Construct(
            PlayerInput inputMap,
            PlayerStatus playerStatus,
            AvatarData avatarData,
            SkinnedMeshRenderer meshRenderer,
            ISubscriber<StartStageMessage> startSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _inputMap = inputMap;
            _playerStatus = playerStatus;
            
            _avatarData = avatarData;
            _renderer = meshRenderer;
            
            _matPropBlock = new MaterialPropertyBlock();

            startSub
                .Subscribe(msg => _inputMap.Enable())
                .AddTo(disposableBagBuilder);

            endSub
                .Subscribe(msg => _inputMap.Disable())
                .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _sharedAvatarIndex.Value = _playerStatus.AvatarIndex;
            }
            
            _sharedAvatarIndex.OnValueChanged += OnAvatarIndexChanged;
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, _sharedAvatarIndex.Value);
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _inputMap?.Dispose();
            _sharedAvatarIndex.OnValueChanged = null;

            base.OnNetworkDespawn();
        }
        
        private void OnAvatarIndexChanged(int prevIdx, int newIdx)
        {
            if (newIdx == prevIdx) return;
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, newIdx);
        }
    }
}