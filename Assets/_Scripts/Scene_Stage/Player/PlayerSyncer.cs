using System.Collections.Generic;
using _Scripts._Messages.Stage;
using _Scripts._Shared.Data;
using _Scripts.Messages.Stage;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Player.Behaviour;
using MessagePipe;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Scene_Stage.Player
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
            Debug.Log($"비헤이비어 들 들어오나요? : {behaviours.Count}");
            
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
                Debug.Log($"플레이어 스폰, owner = true, respawn? {_isRespawn}");
                _sharedAvatarIndex.Value = _playerStatus.AvatarIndex;
            }

            _sharedAvatarIndex.OnValueChanged += OnAvatarIndexChanged;
            
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, _sharedAvatarIndex.Value);
            _respawnPoint = transform.position;
            
            base.OnNetworkSpawn();
        }

        protected override void OnNetworkPostSpawn() // [핵심] 해답은 여기였다 ㅠㅠㅠㅠㅠㅠㅠㅠㅠㅠㅠㅠ 로컬플레이어가 왜 그냥 스폰에선 false고 post에선 true지...? 어쨌든! 다른 비헤이비어들이 아직 스폰되지 않은 상태였고, 비헤이비어의 subscribe 메서드에는 islocalPlayer 여부로 리턴이 걸려있으니, 당연히 작동 안 했던거.....
        {
            Debug.Log($"포스트 플레이어 스폰, owner? {IsOwner}, localplayer? {IsLocalPlayer} , respawn? {_isRespawn}");
            if (IsOwner && _isRespawn) EnableInputs();

            base.OnNetworkPostSpawn();
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

        private void EnableInputs()
        {
            _inputMap.Enable();
                
            foreach (var behaviour in _behaviours)
            {
                behaviour.SubscribeInputEvents(new StartStageMessage());
                Debug.Log($"비헤이비어들 인풋 활성화 중 : {behaviour.GetType().Name}");
            }
        }

        public PlayerSyncer ApplySpawnInfo(bool isRespawn)
        {
            _isRespawn = isRespawn;
            Debug.Log($"[Spawn] ApplySpawnInfo, 리스폰인가요? {isRespawn}");
            
            return this;
        }

        public void Despawn()
        {
            if (!IsServer) return;
            
            // if (_carrierBehaviour.CarriedItem is IDespawnable carriedItem)
            // {
            // Debug.Log($"[Despawn] player_{OwnerClientId}의 carriedItem 디스폰");
            //     carriedItem.Despawn();
            // }

            Debug.Log($"[Despawn] carrier_{OwnerClientId} Drop");
            _carrierBehaviour.Drop();
            
            Debug.Log($"[Despawn] player_{OwnerClientId} 디스폰 메세지 발송");
            var msg = new PlayerDespawnMessage(this.OwnerClientId, _respawnPoint, NetworkManager.ServerTime.TimeAsFloat);
            _despawnPub.Publish(msg);
            
            this.NetObj.Despawn();
        }
    }
}