using System;
using System.Collections.Generic;
using _Scripts.Messages.Stage;
using _Scripts.Stage.Data;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.UI.Board.Score;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Board.Order
{
    public class OrderPresenter : NetworkBehaviour, INetworkUpdateSystem, ITeamMessage
    {
        [SF] private Team team;
        // Data
        private StageData _stageData;
        // Dependency
        private StageHub _stageHub;
        // Caching
        private IPublisher<AddOrderMessage> _addPub;
        private IPublisher<RemoveOrderMessage> _removePub;
        private readonly Stack<OrderStatus> _inactiveOrderStack = new();
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        // Status
        private readonly List<OrderStatus> _activeOrderList = new();
        private int _nextOrderId;
        private float _lastUpdateTime;
        // Property
        private bool HasOrder => _activeOrderList.Count > 0;
        public Team Team => team;
        
        [Inject]
        private void Construct(
            StageData stageData,
            StageHub stageHub,
            IPublisher<AddOrderMessage> addPub,
            IPublisher<RemoveOrderMessage> removePub,
            IPublisher<OrderPresenter> presenterPub,
            IBufferedSubscriber<PublishRequestMessage> requestSub,
            ISubscriber<StartStageMessage> startSub)
        {
            _stageData = stageData;
            _stageHub = stageHub;
            _addPub = addPub;
            _removePub = removePub;

            presenterPub.Publish(this);
            
            requestSub
                .Subscribe(msg =>
                {
                    if (!msg.IsRequest(this)) return;
                    presenterPub.Publish(this);
                    
                }).AddTo(_disposableBagBuilder);
            
            startSub
                .Subscribe(msg =>
                {
                    if (!IsServer) return;
                    BeginOrder();
                })
                .AddTo(_disposableBagBuilder);
          
            _nextOrderId = 0;
            
            for (int i = 0; i < stageData.OrderInfo.MaxActiveOrderCount * 2; i++)
            {
                _inactiveOrderStack.Push(new OrderStatus());
            }
        }

        public override void OnNetworkSpawn()
        {
            _lastUpdateTime = NetworkManager.ServerTime.TimeAsFloat;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            this.UnregisterAllNetworkUpdates();
            base.OnNetworkPreDespawn();
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            switch (updateStage)
            {
                case NetworkUpdateStage.EarlyUpdate:
                    UpdateExpireTimers();
                    return;
                
                case NetworkUpdateStage.Update:
                    CheckAddTiming();
                    return;
                
                default:
                    break;
            }
        }

        public override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }
        
        public void BeginOrder()
        {
            if (!IsServer) return;
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void CheckAddTiming()
        {
            if ((NetworkManager.ServerTime.TimeAsFloat - _lastUpdateTime) < _stageData.OrderInfo.NewOrderInterval) return;
            if (_activeOrderList.Count >= _stageData.OrderInfo.MaxActiveOrderCount) return;
            
            AddOrder();
            _lastUpdateTime = NetworkManager.ServerTime.TimeAsFloat;
        }

        public bool CheckRecipe(IngredientType recipe)
        {
            OrderStatus matchOrder = null;
            
            foreach (var activeOrder in _activeOrderList)
            {
                if (recipe != activeOrder.Recipe) continue;
                
                matchOrder = activeOrder;
                break;
            }

            if (matchOrder is null) return false;
            
            // 득점
            var scorePresenter = _stageHub.FetchScorePresenter(team);
            scorePresenter.UpdateScore(matchOrder.BaseScore, matchOrder.RemainingRatio);
            RemoveOrder(matchOrder);
            Debug.Log($"Check Recipe Success -> {matchOrder.Id}");
            return true;
        }

        private void UpdateExpireTimers()
        {
            var scorePresenter = _stageHub.FetchScorePresenter(team);
            
            for (int i = 0; i < _activeOrderList.Count; i++)
            {
                var expired = _activeOrderList[i].UpdateTimer();
                if (!expired) continue;
                
                // 감점
                var targetOrder = _activeOrderList[i];
                scorePresenter.UpdateScore(targetOrder.BaseScore, -1);
                RemoveOrder(targetOrder);
                Debug.Log($"Order Expired -> {targetOrder.Id}");
            }
        }

        private void AddOrder()
        {
            if (!HasOrder)
            {
                this.RegisterNetworkUpdate(NetworkUpdateStage.EarlyUpdate);
            }
            
            int rndIdx = Random.Range(0, _stageData.OrderInfo.MenuList.Length);
            var randomMenu = _stageData.OrderInfo.MenuList[rndIdx];

            Debug.Log($"[new order - {team}] {randomMenu}");
            
            var orderStatus = _inactiveOrderStack.Pop().InitStatus(_nextOrderId, randomMenu);
            _activeOrderList.Add(orderStatus);
            
            AddRpc(new AddOrderMessage(team, _nextOrderId++,orderStatus.Recipe,orderStatus.Duration,NetworkManager.ServerTime.TimeAsFloat));
        }

        private void RemoveOrder(OrderStatus target)
        {
            if(!_activeOrderList.Remove(target)) return;
            
            RemoveRpc(new RemoveOrderMessage(team, target.Id));
            
            _inactiveOrderStack.Push(target);

            _lastUpdateTime = NetworkManager.ServerTime.TimeAsFloat;
            
            if (!HasOrder)
            {
                this.UnregisterNetworkUpdate(NetworkUpdateStage.EarlyUpdate);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void AddRpc(AddOrderMessage message)
        {
            _addPub.Publish(message);
        }

        [Rpc(SendTo.Everyone)]
        private void RemoveRpc(RemoveOrderMessage message)
        {
            _removePub.Publish(message);
        }
    }
}