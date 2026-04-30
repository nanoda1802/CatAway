using System.Collections.Generic;
using _Scripts.Stage._Data;
using _Scripts.Stage._Data.UI;
using _Scripts.Stage._Messages;
using _Scripts.Stage.Player;
using _Scripts.Stage.UI;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Stage
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
            
            if (!HasWaiter)
            {
                this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            }
            
            _waiters.Add(msg.TagetId, new RespawnWaiter(msg.TagetId, _data.RespawnWaitTime, msg.RespawnPoint));
            
            ShowCardRpc(msg.TagetId, msg.RespawnPoint, msg.DespawnTime);
        }

        private void RemoveWaiter(ulong waiterId)
        {
            if (!IsServer) return;
            
            _waiters.Remove(waiterId, out var waiter);
            
            if (!HasWaiter)
            {
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
        }

        [Rpc(SendTo.Everyone)]
        private void ShowCardRpc(ulong targetId, Vector3 worldPos, float despawnTime)
        {
            if (!_inactiveCards.TryDequeue(out var card)) return;
            
            _activeCards.Add(targetId, card);
            
            card.SetPos(worldPos)
                .Activate(despawnTime);
        }

        [Rpc(SendTo.Everyone)]
        private void HideCardRpc(ulong targetId)
        {
            if (!_activeCards.Remove(targetId, out var card)) return;

            _inactiveCards.Enqueue(card);
            
            card.Deactivate();
        }
    }
}