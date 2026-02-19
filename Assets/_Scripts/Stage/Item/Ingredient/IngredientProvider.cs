using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Ingredient
{
    public class IngredientProvider : NetworkBehaviour
    {
        [SF] private Ingredient prefab;
        [SF] private SerializedDictionary<IngredientType, IngredientData> ingredientDataDic;
        [SF] private IngredientType requiredType;
        [SF] private int defaultCapacity = 20;
        [SF] private int maxPoolSize = 40;
        
        private IObjectPool<Ingredient> _pool;

        public IngredientType RequiredType => requiredType;
        
        public override void OnNetworkSpawn()
        {
            var ingredientNetObj = prefab.GetComponent<NetworkObject>();
            var prefabHandler = new IngredientPrefabHandler(this);
            NetworkManager.PrefabHandler.AddHandler(ingredientNetObj, prefabHandler);
            
            Debug.Log($"[provider spawn] server : {IsServer} / host : {IsHost} / auth : {HasAuthority}");
            InitPool();
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            var ingredientNetObj = prefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.RemoveHandler(ingredientNetObj);
            
            base.OnNetworkDespawn();
        }
    
        private void InitPool()
        {
            _pool = new ObjectPool<Ingredient>(
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
                _pool.Release(ingredient);
            }
        }
        
        private Ingredient CreateIngredient()
        {
            var ingredient = Instantiate(prefab,this.transform);
            ingredient.name = $"Ingredient_{ingredient.GetHashCode()}";
            ingredient.InitComponents(IsServer);
            return ingredient;
        }
        
        private void OnGetIngredient(Ingredient ingredient)
        {
            ingredient.gameObject.SetActive(true);
        }
    
        private void OnReleaseIngredient(Ingredient ingredient)
        {
            ingredient.gameObject.SetActive(false);
            ingredient.transform.localPosition = Vector3.zero;
            ingredient.transform.localRotation = Quaternion.identity;
        }
    
        private void OnDestroyIngredient(Ingredient ingredient)
        {
        }
    
        public Ingredient GetIngredient(IngredientType type, Vector3 pos)
        {
            var ingredient = _pool.Get();
            var data = ingredientDataDic.GetValueOrDefault(type, ingredientDataDic[requiredType]);
            ingredient.InitStatus(IsServer, data, data.Type == requiredType);
            ingredient.transform.position = pos;
            return ingredient;
        }
    
        public void ReleaseIngredient(Ingredient ingredient)
        {
            if (IsServer) ingredient.NetworkObject.TrySetParent(this.NetworkObject);
            _pool.Release(ingredient);
        }

        public (Mesh, Vector3) GetModelInfo(IngredientType type)
        {
            var data = ingredientDataDic.GetValueOrDefault(type, ingredientDataDic[requiredType]);
            return (data.DefaultRenderMesh, data.DefaultScale);
        }
    }
}