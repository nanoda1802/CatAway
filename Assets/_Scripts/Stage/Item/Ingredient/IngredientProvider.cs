using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Ingredient
{
    public class IngredientProvider : NetworkBehaviour, INetworkPrefabInstanceHandler
    {
        [SF] private Item.Ingredient.Ingredient prefab;
        [SF] private int defaultCapacity = 15;
        [SF] private int maxPoolSize = 30;
        
        private IObjectPool<Item.Ingredient.Ingredient> _pool;
    
        public override void OnNetworkSpawn()
        {
            InitPool();
            if (IsServer) NetworkManager.PrefabHandler.AddHandler(prefab.NetworkObject, this);
            
            base.OnNetworkSpawn();
        }
    
        public override void OnNetworkDespawn()
        {
            if (IsServer) NetworkManager.PrefabHandler.RemoveHandler(prefab.NetworkObject);
            
            base.OnNetworkDespawn();
        }
    
        private void InitPool()
        {
            _pool = new ObjectPool<Item.Ingredient.Ingredient>(
                CreateIngredient, 
                OnGetIngredient, 
                OnReleaseIngredient, 
                OnDestroyIngredient, 
                true, 
                defaultCapacity,
                maxPoolSize);
            
            for (int i = 0; i < defaultCapacity; i++)
            {
                var ingredient = CreateIngredient();
                ingredient.InitComponents();
                _pool.Release(ingredient);
            }
        }
        
        private Item.Ingredient.Ingredient CreateIngredient()
        {
            var ingredient = Instantiate(prefab,this.transform);
            ingredient.name = $"Ingredient_{ingredient.GetHashCode()}";
            return ingredient;
        }
        
        private void OnGetIngredient(Item.Ingredient.Ingredient ingredient)
        {
            ingredient.gameObject.SetActive(true);
        }
    
        private void OnReleaseIngredient(Item.Ingredient.Ingredient ingredient)
        {
            if (IsServer) ingredient.Reset();
            
            ingredient.gameObject.SetActive(false);
            ingredient.transform.localPosition = Vector3.zero;
            ingredient.transform.localRotation = Quaternion.identity;
        }
    
        private void OnDestroyIngredient(Item.Ingredient.Ingredient ingredient)
        {
        }
    
        public Item.Ingredient.Ingredient GetIngredient(Vector3 pos)
        {
            var ingredient = _pool.Get();
            ingredient.transform.position = pos;
            return ingredient;
        }
    
        public void ReleaseIngredient(Item.Ingredient.Ingredient ingredient)
        {
            if (IsServer) ingredient.NetworkObject.TrySetParent(this.NetworkObject);
            _pool.Release(ingredient);
        }
    
        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return GetIngredient(position).NetworkObject;
        }
    
        public void Destroy(NetworkObject networkObject)
        {
            var ingredient = networkObject.GetComponent<Item.Ingredient.Ingredient>();
            ReleaseIngredient(ingredient);
        }
    }
}