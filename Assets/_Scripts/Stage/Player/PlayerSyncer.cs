using System.Collections.Generic;
using _Scripts.Shared._Data;
using _Scripts.Stage._Messages;
using _Scripts.Stage.Player.Behaviour;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.Player
{
    public class PlayerSyncer : NetworkBehaviour, IDespawnable
    {
        // Components
        private SkinnedMeshRenderer _renderer;
        private CarrierBehaviour _carrierBehaviour;
        // Data
        private AvatarData _avatarData;
        // Dependency
        private PlayerInput _inputMap;
        private PlayerStatus _playerStatus;
        private IReadOnlyList<IBehaviourWithInput>  _behaviours;
        private IPublisher<PlayerDespawnMessage> _despawnPub;
        // Caching
        private Vector3 _respawnPoint;
        private MaterialPropertyBlock _matPropBlock;
        
        private bool _isRespawn;
        private readonly NetworkVariable<int> _sharedAvatarIndex 
            = new (-1, writePerm : NetworkVariableWritePermission.Owner);

        public NetworkObject NetObj { get; private set; }

        [Inject]
        private void Construct(
            PlayerInput inputMap,
            PlayerStatus playerStatus,
            AvatarData avatarData,
            SkinnedMeshRenderer meshRenderer,
            CarrierBehaviour carrierBehaviour,
            IReadOnlyList<IBehaviourWithInput> behaviours,
            IPublisher<PlayerDespawnMessage> despawnPub,
            ISubscriber<StartStageMessage> startSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            NetObj = GetComponent<NetworkObject>();
            _matPropBlock = new MaterialPropertyBlock();

            _carrierBehaviour = carrierBehaviour;
            _behaviours = behaviours;
            
            _inputMap = inputMap;
            _playerStatus = playerStatus;
            
            _avatarData = avatarData;
            _renderer = meshRenderer;
            
            _despawnPub = despawnPub;

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
            _respawnPoint = transform.position;
            
            base.OnNetworkSpawn();
        }

        protected override void OnNetworkPostSpawn()
        {
            if (IsOwner && _isRespawn) EnableInputs();

            base.OnNetworkPostSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            if (IsOwner) DisableInputs();
            
            _sharedAvatarIndex.OnValueChanged = null;

            base.OnNetworkDespawn();
        }
        
        private void OnAvatarIndexChanged(int prevIdx, int newIdx)
        {
            if (newIdx == prevIdx) return;
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, newIdx);
        }

        private void EnableInputs()
        {
            _inputMap.Enable();
                
            foreach (var behaviour in _behaviours)
            {
                behaviour.SubscribeInputEvents(new StartStageMessage());
            }
        }

        private void DisableInputs()
        {
            _inputMap.Disable();
                
            foreach (var behaviour in _behaviours)
            {
                behaviour.UnsubscribeInputEvents(new EndStageMessage());
            }
        }

        public PlayerSyncer ApplySpawnInfo(bool isRespawn)
        {
            _isRespawn = isRespawn;
            
            return this;
        }

        public void Despawn()
        {
            if (!IsServer) return;

            _carrierBehaviour.Drop();
            
            var msg = new PlayerDespawnMessage(this.OwnerClientId, _respawnPoint, NetworkManager.ServerTime.TimeAsFloat);
            _despawnPub.Publish(msg);
            
            this.NetObj.Despawn(true);
        }
    }
}