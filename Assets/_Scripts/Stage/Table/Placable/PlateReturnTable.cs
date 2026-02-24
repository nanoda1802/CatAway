using System;
using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Wrapper;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Placable
{
    public class PlateReturnTable : NetworkBehaviour, IPlacable
    {
        // Data
        [SF] private float plateOffsetY = 0.1f;
        // Dependency
        private AttachableSlot _tableSlot;
        // Caching
        private readonly Stack<Carriable> _returnedPlates = new();
        private IDisposable _subscription;
        // Property
        public Carriable PlacedItem => _returnedPlates.TryPeek(out Carriable item) ? item : null;

        [Inject]
        private void Construct(
            IPublisher<IPlacable> pub,
            IBufferedSubscriber<PublishRequestMessage> sub)
        {
            _tableSlot = this.GetComponentInChildren<AttachableSlot>();
            _tableSlot.OnAttach += OnSlotAttached;
            _tableSlot.OnDetach += OnSlotDetached;
            
            pub.Publish(this);
            
            _subscription = sub.Subscribe(msg =>
            {
                if (!msg.IsRequest(this)) return;
                pub.Publish(this);    
            });
        }

        #region NGO 관련 메서드
        public override void OnNetworkPreDespawn()
        {
            _tableSlot.OnAttach -= OnSlotAttached;
            _tableSlot.OnDetach -= OnSlotDetached;
            
            _subscription?.Dispose();
            
            base.OnNetworkPreDespawn();
        }
        
        private void OnSlotAttached(Carriable item)
        {
            if (!IsServer) return;
            
            _returnedPlates.Push(item);
                
            var newPlatePos = (_returnedPlates.Count - 1) * plateOffsetY * Vector3.up;
            UpdatePositionRpc(item, newPlatePos);
        }

        private void OnSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            
            _returnedPlates.Pop();
        }
        
        [Rpc(SendTo.Everyone)]
        private void UpdatePositionRpc(NetworkBehaviourReference carriableRef, Vector3 localPos)
        {
            if (!carriableRef.TryGet(out Carriable newPlate)) return;
            newPlate.transform.SetLocalPositionAndRotation(localPos, Quaternion.identity);
        }
        #endregion

        #region Placable 관련 메서드
        public void Place(Carriable item)
        {
            if (item.IsCarrying) item.Detach();
            item.Attach(_tableSlot);
        }
        
        public bool CanPlace(Carriable item, out string rejectMessage)
        {
            rejectMessage = null;
            
            if (item == null || !item.IsSpawned)
            {
                rejectMessage = "Item이 null이거나 Spawn되지 않은 상태입니다.";
                return false;
            }

            if (item is not Plate)
            {
                rejectMessage = "Plate가 아닌 아이템은 Place할 수 없습니다.";
                return false;
            }

            return true;
        }

        public bool CanDisPlace(out string rejectMessage)
        {
            rejectMessage = null;
            
            if (PlacedItem == null || !PlacedItem.IsSpawned)
            {
                rejectMessage = "PlacedItem이 null이거나 Spawn되지 않은 상태입니다.";
                return false;
            }
            
            return true;
        }
        #endregion
    }
}