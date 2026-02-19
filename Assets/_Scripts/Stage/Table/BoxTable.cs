using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class BoxTable : NetworkBehaviour, IPlacable
    {
        [SF] private AttachableNode pivot;
        
        private Carriable _placedItem;
        private TagHandle _itemTag;

        public Carriable PlacedItem => _placedItem;
        
        private void Awake()
        {
            _itemTag = TagHandle.GetExistingTag("Item");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Throwable throwable) || !throwable.IsThrowing) return;
            if (!other.TryGetComponent(out Carriable carriable) || carriable.IsAttach) return;

            if (TryPlace(carriable))
            {
                
            }
        }

        public bool TryPlace(Carriable carriable)
        {
            if (carriable == null || !carriable.IsSpawned) return false;
            if (pivot.HasAttachments && _placedItem is not null) return CanPlaceAdditionalItem(carriable);
            
            if (carriable.IsAttach) carriable.Detach();
            carriable.Attach(pivot);
            _placedItem = carriable;
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            carriable = null;
            if (carrier == null) return false;
            if (!pivot.HasAttachments || _placedItem is null) return false;

            if (carrier.HasAttachments && CanDisplaceAdditionalItem(ref carriable)) return true;
            
            carriable = _placedItem;
            _placedItem.Detach();
            _placedItem = null;
            
            return true;
        }

        private bool CanPlaceAdditionalItem(Carriable item)
        {
            switch (_placedItem.Type)
            {
                case CarriableType.Plate:
                case CarriableType.Cookware:
                    if (item.Type != CarriableType.Ingredient) return false;
                    if (!_placedItem.NetworkObject.TryGetComponent(out IIngredientHolder holder)) return false;
                    return holder.TryAdd(item);
                
                case CarriableType.Ingredient:
                    return false;
                
                default:
                    Debug.LogError($"[{this.OwnerClientId} BoxTable.TryPlace] \"{_placedItem.Type}\"은 존재하지 않는 CarriableType 입니다.");
                    return false;
            }   
        }

        private bool CanDisplaceAdditionalItem(ref Carriable item)
        {
            if (_placedItem is null || !_placedItem.IsSpawned) return false;
            if (_placedItem.Type != CarriableType.Cookware) return false;
            if (!_placedItem.NetworkObject.TryGetComponent(out Cookware cookware)) return false;
            if (!cookware.HasIngredient) return false;
            
            item = cookware.TakeOutCarriable();
            item?.Detach();
            return item is not null;
        }
    }
}