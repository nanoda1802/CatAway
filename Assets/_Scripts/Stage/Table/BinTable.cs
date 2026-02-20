using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class BinTable : NetworkBehaviour, IPlacable
    {
        /* 컴포넌트 */
        private IngredientProvider _ingredientProvider;
        /* 기타 */
        private TagHandle _itemTag;
        /* 프로퍼티 */
        public Carriable PlacedItem => null;
        
        [Inject]
        private void Construct(IngredientProvider provider)
        {
            _ingredientProvider = provider;
            
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
            return CanHandleItem(item);
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            displacedItem = null;
            return false;
        }

        private bool CanHandleItem(Carriable item)
        {
            switch (item.Type)
            {
                case CarriableType.Plate or CarriableType.Cookware:
                    if (!item.NetworkObject.TryGetComponent(out IIngredientHolder holder)) return false;
                    if (!holder.HasIngredient) return false;
                    holder.ClearHolder();
                    return false;
                
                case CarriableType.Ingredient:
                    if (!item.NetworkObject.TryGetComponent(out Ingredient ingredient)) return false;
                    _ingredientProvider.ReleaseIngredient(ingredient);
                    ingredient.NetworkObject.Despawn(false);
                    return true;
                
                default:
                    Debug.LogError($"[{this.OwnerClientId} BinTable.TryPlace] \"{item.Type}\"은 존재하지 않는 CarriableType 입니다.");
                    return false;   
            }
        }
    }
}