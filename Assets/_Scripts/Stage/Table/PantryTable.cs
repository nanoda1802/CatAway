using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class PantryTable : NetworkBehaviour, IPlacable
    {
        [SF] private IngredientType ingredientType;
        [SF] private IngredientProvider _ingredientProvider;
    
        [SF] private MeshFilter sampleFilter;
        [SF] private Transform sampleTr;

        public Carriable PlacedItem => null;

        // [추후 수정] 주입받도록
        private void Construct(IngredientProvider provider)
        {
            this._ingredientProvider = provider;
        }

        public override void OnNetworkSpawn()
        {
            (Mesh mesh, Vector3 scale) = _ingredientProvider.GetModelInfo(ingredientType);
                
            sampleFilter.sharedMesh = mesh;
            sampleTr.localScale = scale;
        
            base.OnNetworkSpawn();
        }

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
            
            NetworkManager.PrefabHandler.SetInstantiationData(ingredient.NetworkObject,new IngredientTypeNetData(ingredientType));
            ingredient.NetworkObject.Spawn();
            
            return true;
        }
    }
}