using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class BinTable : NetworkBehaviour, IPlacable
    {
        [SF] private IngredientProvider _ingredientProvider;
        
        private TagHandle _itemTag;

        public Carriable PlacedItem => null;
        
        private void Awake()
        {
            _itemTag = TagHandle.GetExistingTag("Item");
        }
        
        // [추후 수정] 주입받도록
        private void Construct(IngredientProvider provider)
        {
            this._ingredientProvider = provider;
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
            return CanHandleItem(carriable);
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            carriable = null;
            return false;
        }

        private bool CanHandleItem(Carriable item)
        {
            switch (item.Type)
            {
                case CarriableType.Plate:
                case CarriableType.Cookware:
                    if (!item.NetworkObject.TryGetComponent(out IIngredientHolder holder)) return false;
                    if (!holder.HasIngredient) return false;
                    holder.ClearHolder();
                    return false;
                
                case CarriableType.Ingredient:
                    if (!item.NetworkObject.TryGetComponent(out Ingredient ingredient)) return false;
                    if (item.IsAttach) item.Detach();
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