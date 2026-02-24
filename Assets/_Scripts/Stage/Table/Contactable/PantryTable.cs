using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Contactable
{
    public class PantryTable : NetworkBehaviour, IContactable
    {
<<<<<<< Updated upstream:Assets/_Scripts/Stage/Table/PantryTable.cs
        [SF] private IngredientType ingredientType;
        [SF] private IngredientProvider _ingredientProvider;
    
        [SF] private MeshFilter sampleFilter;
        [SF] private Transform sampleTr;

        public Carriable PlacedItem => null;

        // [추후 수정] 주입받도록
=======
        // Data
        [SF] private IngredientType ingredientType;
        // Dependency
        private IngredientProvider _ingredientProvider;
        // Component
        [SF] private MeshFilter sampleMeshFilter;
        [SF] private Transform sampleTransform;
        
        [Inject]
>>>>>>> Stashed changes:Assets/_Scripts/Stage/Table/Contactable/PantryTable.cs
        private void Construct(IngredientProvider provider)
        {
            _ingredientProvider = provider;
        }

        public override void OnNetworkSpawn()
        {
            (Mesh mesh, Vector3 scale) = _ingredientProvider.GetModelInfo(ingredientType);
                
            sampleFilter.sharedMesh = mesh;
            sampleTr.localScale = scale;
        
            base.OnNetworkSpawn();
        }

<<<<<<< Updated upstream:Assets/_Scripts/Stage/Table/PantryTable.cs
        public bool TryPlace(Carriable carriable)
        {
            return false;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            carriable = null;
            if (carrier == null || carrier.HasAttachments) return false;
        
            var ingredient = _ingredientProvider.GetIngredient(ingredientType, transform.position);
            carriable = ingredient?.GetComponentInChildren<Carriable>();
            if (ingredient == null || carriable == null) return false;
=======
        #region Contactable 관련 메서드
        public bool TryContact(Carriable item, out string failMessage)
        {
            failMessage = null;
>>>>>>> Stashed changes:Assets/_Scripts/Stage/Table/Contactable/PantryTable.cs
            
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