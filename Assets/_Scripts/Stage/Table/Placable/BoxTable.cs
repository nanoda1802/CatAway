using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Wrapper;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Placable
{
    public class BoxTable : NetworkBehaviour, IPlacable
    {
        // Component
        private AttachableSlot _tableSlot;
        private PlacementBroker _placementBroker;
        // Caching
        private TagHandle _itemTag;
        // Property
        public Carriable PlacedItem { get; private set; }

        [Inject]
        private void Construct(PlacementBroker placementBroker)
        {
            _placementBroker = placementBroker;

            _itemTag = TagHandle.GetExistingTag("Item");
            
            _tableSlot = this.GetComponentInChildren<AttachableSlot>();
            _tableSlot.OnAttach += OnSlotAttached;
            _tableSlot.OnDetach += OnSlotDetached;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Ingredient ingredient) || !ingredient.IsThrowing) return;

            var result = _placementBroker.AcceptCase(ingredient, this);
            if (result.Reason is not null) Debug.LogWarning($"{result.Reason} [BoxTable{this.NetworkObjectId}_OnTrigger]");
        }

        #region NGO 관련 메서드
        public override void OnNetworkPreDespawn()
        {
            _tableSlot.OnAttach -= OnSlotAttached;
            _tableSlot.OnDetach -= OnSlotDetached;
            base.OnNetworkPreDespawn();
        }

        private void OnSlotAttached(Carriable item)
        {
            item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (IsServer) PlacedItem = item;
        }

        private void OnSlotDetached(Carriable item)
        {
            if (IsServer) PlacedItem = null;
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