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
        // Dependency
        private StageHub _stageHub;
        // Property
        public bool HasIngredient => _holderSlot.HasAttachments && HeldIngredient is not null;
        public AttachableSlot HolderSlot => _holderSlot;
        public Ingredient.Ingredient HeldIngredient { get; private set; }

        [Inject]
        public void ConstructCookware(StageHub stageHub)
        {
            _stageHub = stageHub;

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
 
            var ingredient = HeldIngredient;
            if (ingredient.IsCarrying) ingredient.Detach();
            
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            provider.ReleaseIngredient(ingredient);
            ingredient.NetworkObject.Despawn(false);
        }
        #endregion
    }
}