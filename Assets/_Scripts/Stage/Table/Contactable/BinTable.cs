using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.Table.Contactable
{
    public class BinTable : NetworkBehaviour, IContactable
    {
        // Dependency
        private StageHub _stageHub;
        private ContactBroker _contactBroker;
        // Caching
        private TagHandle _itemTag;
        
        [Inject]
        private void Construct(
            StageHub stageHub,
            ContactBroker contactBroker)
        {
            _stageHub =  stageHub;
            _contactBroker = contactBroker;
            
            _itemTag = TagHandle.GetExistingTag("Item");
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Ingredient ingredient) || !ingredient.IsThrowing) return;

            var result = _contactBroker.AcceptCase(ingredient, this);
            if (result.Reason is not null) Debug.LogWarning($"{result.Reason} [BinTable{this.NetworkObjectId}_OnTrigger]");
        }

        #region Contactable 관련 메서드
        public bool TryContact(Carriable item, out string failMessage)
        {
            failMessage = null;
            
            if (item == null || !item.IsSpawned)
            {
                failMessage = "접촉할 Item이 없거나, Spawn되지 않은 상태입니다.";
                return false;
            }

            if (item is Ingredient)
            {
                return true;
            }

            if (item is IIngredientHolder { HasIngredient: true })
            {
                return true;
            }

            failMessage = "이미 비워진 상태의 Holder입니다.";
            return false;
        }

        public void RespondTo(Ingredient ingredient)
        {
            if (ingredient.IsCarrying) ingredient.Detach();
            
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            provider.ReleaseIngredient(ingredient);
            ingredient.NetworkObject.Despawn(false);
        }

        public void RespondTo(Plate plate)
        {
            plate.ClearHolder();
        }

        public void RespondTo(Cookware cookware)
        {
            cookware.ClearHolder();
        }
        #endregion
    }
}