using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class BoxTable : NetworkBehaviour, IPlacable
    {
        /* 컴포넌트 */
        private AttachableNode _pivot;
        /* 캐싱 */
        private Carriable _placedItem;
        /* 기타 */
        private TagHandle _itemTag;
        /* 프로퍼티 */
        public Carriable PlacedItem => _placedItem;

        [Inject]
        private void Construct()
        {
            _pivot = GetComponentInChildren<AttachableNode>();
            _itemTag = TagHandle.GetExistingTag("Item");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Throwable throwable) || !throwable.IsThrowing) return;
            if (!other.TryGetComponent(out Carriable carriable) || carriable.IsAttach) return;

            TryPlace(carriable);
        }

        public bool TryPlace(Carriable item)
        {
            if (item == null || !item.IsSpawned) return false;
            if (_pivot.HasAttachments && _placedItem is not null) return CanPlaceAdditionalItem(item);
            
            item.AttachTo(_pivot);
            _placedItem = item;
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            displacedItem = null;
            if (carrier == null || !carrier.IsSpawned) return false;
            if (!_pivot.HasAttachments || _placedItem is null) return false;

            if (carrier.HasAttachments && CanDisplaceAdditionalItem(ref displacedItem)) return true;
            
            displacedItem = _placedItem;
            _placedItem.Detach();
            _placedItem = null;
            
            return true;
        }

        private bool CanPlaceAdditionalItem(Carriable item)
        {
            switch (_placedItem.Type)
            {
                case CarriableType.Plate or CarriableType.Cookware:
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
            if (!_placedItem.NetworkObject.TryGetComponent(out IIngredientHolder cookware)) return false;
            if (!cookware.HasIngredient) return false;
            
            item = cookware.TakeOutIngredient();
            item?.Detach();
            
            return item is not null;
        }
    }
}