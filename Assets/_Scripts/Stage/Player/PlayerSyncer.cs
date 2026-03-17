using _Scripts.Messages.Stage;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.Player
{
    public class PlayerSyncer : NetworkBehaviour
    {
        // Components
        private SkinnedMeshRenderer _renderer;
        // Data
        private AvatarData _avatarData;
        // Dependency
        private PlayerInput _inputMap;
        // Caching
        private MaterialPropertyBlock _matPropBlock;
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        private readonly NetworkVariable<int> _sharedAvatarIndex 
            = new (-1, writePerm : NetworkVariableWritePermission.Server);

        [Inject]
        private void Construct(
            PlayerInput inputMap,
            AvatarData avatarData,
            SkinnedMeshRenderer meshRenderer,
            ISubscriber<StartStageMessage> startSub)
        {
            _inputMap = inputMap;
            
            _avatarData = avatarData;
            _renderer = meshRenderer;
            
            _matPropBlock = new MaterialPropertyBlock();

            startSub
                .Subscribe(msg => _inputMap.Enable())
                .AddTo(_disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, _sharedAvatarIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _inputMap.Disable();
        }

        public override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }

        public void SetAvatar(int avatarIndex)
        {
            _sharedAvatarIndex.Value = avatarIndex;
            _sharedAvatarIndex.SetDirty(true);
        }
    }
}