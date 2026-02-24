using _Scripts.Stage.Item.Ingredient;
using _Scripts.Wrapper;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Cookware
{
    public class Cookware : Carriable, IIngredientHolder
    {
        // Data
        [SF] private IngredientType holdableIngredientType = IngredientType.Patty;
        // Component
        private AttachableSlot _holderSlot;
        private IngredientProvider _ingredientProvider;
        // Property
        public bool HasIngredient => _holderSlot.HasAttachments && HeldIngredient is not null;
        public AttachableSlot HolderSlot => _holderSlot;
        public Ingredient.Ingredient HeldIngredient { get; private set; }

        [Inject]
        public void ConstructCookware(IngredientProvider provider)
        {
            this._ingredientProvider = provider;

            _holderSlot = GetComponentInChildren<AttachableSlot>();
            
            _holderSlot.OnAttach += OnSlotAttached;
            _holderSlot.OnDetach += OnSlotDetached;
        }

        #region NGO 관련 메서드
        public override void OnNetworkPreDespawn()
        {
            _holderSlot.OnAttach -= OnSlotAttached;
            _holderSlot.OnDetach -= OnSlotDetached;
            
            base.OnNetworkPreDespawn();
        }

        private void OnSlotAttached(Carriable item)
        {
            if (item is not Ingredient.Ingredient ingredient) return;
            ingredient.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (HasAuthority) HeldIngredient = ingredient;
        }

        private void OnSlotDetached(Carriable item)
        {
            if (HasAuthority) HeldIngredient = null;
        }
        #endregion

        #region IngredientHolder 관련 메서드
        public bool CanHold(Ingredient.Ingredient ingredient, out string rejectMessage)
        {
            rejectMessage = null;

            if (ingredient == null || !ingredient.IsSpawned)
            {
                rejectMessage = "Item이 null이거나 Spawn되지 않은 상태입니다.";
                return false;
            }
            
            if (this.HasIngredient)
            {
                rejectMessage = "이미 Hold 중인 Ingredient가 있는 Cookware입니다.";    
                return false;
            }
            
            if (!IsAvailableType(ingredient.Type))
            {
                rejectMessage = "Hold할 수 없는 Type의 Ingredient입니다.";
                return false;
            }

            if (ingredient.IsMaxPrepped)
            {
                rejectMessage = "더 이상 조리할 수 없는 Ingredient는 Hold할 수 없습니다.";
                return false;
            }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            
<<<<<<< Updated upstream
            if (carriable.IsAttach) carriable.Detach();
            
            _holdingQueue.Enqueue(carriable);
            carriable.Attach(pivot);
=======
>>>>>>> Stashed changes
=======
            
>>>>>>> Stashed changes
=======
            
>>>>>>> Stashed changes
            return true;
        }

        public void Hold(Ingredient.Ingredient ingredient)
        {
            if (ingredient.IsCarrying) ingredient.Detach();
            ingredient.Attach(_holderSlot);
        }

        private bool IsAvailableType(IngredientType type)
        {
            return holdableIngredientType.HasFlag(type);
        }
        
        public void ClearHolder()
        {
            if (!HasAuthority || !HasIngredient) return;
            
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            ClearCookwareRpc();
        }

        public Carriable TakeOutCarriable()
        {
            return _holdingQueue.TryDequeue(out var carriable) ? carriable : null;
        }

        [Rpc(SendTo.Everyone)]
        private void ClearCookwareRpc()
        {
            // 뭐 모델 동기화라든지... 해줄 거 있으면 해주기
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            var ingredient = HeldIngredient;
            if (ingredient.IsCarrying) ingredient.Detach();
            
            _ingredientProvider.ReleaseIngredient(ingredient);
            ingredient.NetworkObject.Despawn(false);
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        }
        #endregion
    }
}