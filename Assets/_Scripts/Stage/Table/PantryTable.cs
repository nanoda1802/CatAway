using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class PantryTable : NetworkBehaviour, IPlacable
    {
        /* 데이터 */
        [SF] private IngredientType ingredientType;
        /* 컴포넌트 */
        [SF] private MeshFilter sampleMeshFilter;
        [SF] private Transform sampleTransform;
        private IngredientProvider _ingredientProvider;
        /* 프로퍼티 */
        public Carriable PlacedItem => null;
        
        [Inject]
        private void Construct(IngredientProvider provider)
        {
            this._ingredientProvider = provider;
        }

        public override void OnNetworkSpawn()
        {
            (Mesh mesh, Vector3 scale) = _ingredientProvider.GetModelInfo(ingredientType);
                
            sampleMeshFilter.sharedMesh = mesh;
            sampleTransform.localScale = scale;
        
            base.OnNetworkSpawn();
        }

        public bool TryPlace(Carriable item)
        {
            return false;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            displacedItem = null;
            if (carrier == null || carrier.HasAttachments) return false;
        
            var ingredient = _ingredientProvider.GetIngredient(ingredientType, transform.position);
            displacedItem = ingredient?.GetComponentInChildren<Carriable>();
            if (ingredient == null || displacedItem == null) return false;
            
            NetworkManager.PrefabHandler.SetInstantiationData(ingredient.NetworkObject,new IngredientTypeNetData(ingredientType));
            ingredient.NetworkObject.Spawn();
            
            return true;
        }
    }
}