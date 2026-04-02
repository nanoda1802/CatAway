using System.Collections.Generic;
using _Scripts._Messages.Stage;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Data.UI;
using _Scripts.Scene_Stage.Player;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage
{
    public class RespawnHandler : NetworkBehaviour, INetworkUpdateSystem
    {
        private IObjectResolver _resolver;
        private RespawnCard _cardPrefab;
        private RespawnCardData _data;
        private PlayerSyncer _playerPrefab;

        private readonly Dictionary<ulong, RespawnCard> _activeCards = new();
        private readonly Queue<RespawnCard> _inactiveCards = new();
        private readonly Dictionary<ulong, RespawnWaiter> _waiters = new(4);
        private readonly Queue<ulong> _respawnQueue = new();
        
        private bool HasWaiter => _waiters.Count > 0;
        
        [Inject]
        private void Construct(
            IObjectResolver resolver,
            PlayerSyncer playerPrefab,
            RespawnCard cardPrefab,
            RespawnCardData data,
            ISubscriber<PlayerDespawnMessage> despawnSub, 
            DisposableBagBuilder disposableBagBuilder)
        {
            _resolver = resolver;
            _playerPrefab = playerPrefab;
            _cardPrefab = cardPrefab;
            _data = data;

            InitCardPool();
            
            despawnSub
                .Subscribe(AddWaiter)
                .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkPreDespawn()
        {
            this.UnregisterNetworkUpdate();

            foreach (var card in _activeCards.Values)
            {
                card.Deactivate();
            }

            base.OnNetworkPreDespawn();
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            foreach (var pair in _waiters)
            {
                var waiter = _waiters[pair.Key];
                bool timerDone = waiter.UpdateTimer();
                if (timerDone) _respawnQueue.Enqueue(waiter.ClientId);
            }

            while (_respawnQueue.Count > 0)
            {
                if (!_respawnQueue.TryDequeue(out ulong waiterId)) continue;
                RemoveWaiter(waiterId);
            }
        }

        private void InitCardPool()
        {
            for (int i = 0; i < 4; i++)
            {
                var card = _resolver.Instantiate(_cardPrefab);
                card.gameObject.SetActive(false);
                _inactiveCards.Enqueue(card);
            }
        }

        private void AddWaiter(PlayerDespawnMessage msg)
        {
            if (!IsServer) return;
            
            Debug.Log($"[Despawn] player_{msg.TagetId}의 디스폰 메세지 수신");

            if (!HasWaiter)
            {
                Debug.Log($"[Despawn] 기존 대기자 없는 관계로 업데이트 등록");
                this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            }
            
            _waiters.Add(msg.TagetId, new RespawnWaiter(msg.TagetId, _data.RespawnWaitTime, msg.RespawnPoint));
            
            ShowCardRpc(msg.TagetId, msg.RespawnPoint, msg.DespawnTime);
        }

        private void RemoveWaiter(ulong waiterId)
        {
            if (!IsServer) return;
            
            Debug.Log($"[Respawn] player_{waiterId} 대기 순번 종료");
            
            _waiters.Remove(waiterId, out var waiter);
            
            if (!HasWaiter)
            {
                Debug.Log($"[Respawn] 남은 대기자 없는 관계로 업데이트 등록 해제");
                this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            }
            
            HideCardRpc(waiterId);
            
            RespawnPlayer(waiter.ClientId, waiter.RespawnPoint);
        }
        
        private void RespawnPlayer(ulong clientId, Vector3 respawnPoint)
        {
            var player = Instantiate(_playerPrefab, respawnPoint, Quaternion.identity)
                .ApplySpawnInfo(true);
                
            NetworkManager.PrefabHandler.SetInstantiationData(player.NetObj, new PlayerSpawnPacket(true));
            player.NetObj.SpawnAsPlayerObject(clientId, true);
            
            Debug.Log($"[Respawn] player_{clientId} 리스폰");
        }

        [Rpc(SendTo.Everyone)]
        private void ShowCardRpc(ulong targetId, Vector3 worldPos, float despawnTime)
        {
            if (!_inactiveCards.TryDequeue(out var card)) return;
            
            _activeCards.Add(targetId, card);
            
            card.SetPos(worldPos)
                .Activate(despawnTime);
            
            Debug.Log($"[Respawn] player_{targetId} 위한 리스폰 카드 활성화");
        }

        [Rpc(SendTo.Everyone)]
        private void HideCardRpc(ulong targetId)
        {
            if (!_activeCards.Remove(targetId, out var card)) return;

            _inactiveCards.Enqueue(card);
            
            card.Deactivate();
            
            Debug.Log($"[Respawn] player_{targetId} 위한 리스폰 카드 종료");
        }
    }
}