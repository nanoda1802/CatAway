using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Contactable
{
    public class PantryTable : NetworkBehaviour, IContactable
    {
        // Data
        [SF] private IngredientType ingredientType;
        // Dependency
        private IngredientProvider _ingredientProvider;
        // Component
        [SF] private MeshFilter sampleMeshFilter;
        [SF] private Transform sampleTransform;
        
        [Inject]
        private void Construct(IngredientProvider provider)
        {
            _ingredientProvider = provider;
        }

        public override void OnNetworkSpawn()
        {
            (Mesh mesh, Vector3 scale) = _ingredientProvider.GetModelInfo(ingredientType);
            
            sampleMeshFilter.sharedMesh = mesh;
            sampleTransform.localScale = scale;

            base.OnNetworkSpawn();
        }
        
        #region Contactable 관련 메서드
        public bool TryContact(Carriable item, out string failMessage)
        {
            failMessage = null;
            
            if (item is not null)
            {
                failMessage = "이미 Carry 중인 아이템이 있습니다.";
                return false;
            }
            
            return true;
        }

        public void RespondTo(CarrierBehaviour carrier)
        {
            var ingredient = _ingredientProvider.GetIngredient(ingredientType, transform.position);
            NetworkManager.PrefabHandler.SetInstantiationData(ingredient.NetworkObject,new IngredientTypeNetData(ingredientType));
            ingredient.NetworkObject.Spawn();
            
            carrier.Pick(ingredient);
        }
        #endregion
    }
}