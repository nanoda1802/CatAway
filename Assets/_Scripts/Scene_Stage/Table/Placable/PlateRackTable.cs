using System;
using System.Collections.Generic;
using _Scripts._Wrapper;
using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Plate;
using _Scripts.Stage;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Table.Placable
{
    public class PlateRackTable : NetworkBehaviour, IPlacable
    {
        // Data
        [SF] private float plateOffsetY = 0.12f;
        // Component
        private AttachableSlot _tableSlot;
        // Caching
        private readonly Stack<Carriable> _washedPlates = new();
        private IDisposable _subscription;
        // Property
        public Carriable PlacedItem => _washedPlates.TryPeek(out Carriable item) ? item : null;
        
        [Inject]
        private void Construct(
            IPublisher<IPlacable> pub,
            IBufferedSubscriber<HubCallMessage> sub)
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
            
            _washedPlates.Push(item);
                
            var newPlatePos = (_washedPlates.Count - 1) * plateOffsetY * Vector3.up;
            UpdatePositionRpc(item, newPlatePos);
        }
        
        private void OnSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            
            _washedPlates.Pop();
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

            if (item is not Plate plate)
            {
                rejectMessage = "Plate가 아닌 아이템은 Place할 수 없습니다.";
                return false;
            }

            if (!plate.IsWellPrepped || plate.HasIngredient)
            {
                rejectMessage = "깨끗한 Plate만 Place할 수 있습니다.";
                return false;
            }

            if (PlacedItem is Plate { HasIngredient: true })
            {
                rejectMessage = "최상단 접시에 재료가 플레이팅 돼있습니다.";
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