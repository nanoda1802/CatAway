using System.Collections;
using System.Collections.Generic;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.Table;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Cookware
{
    public class Cookware : NetworkBehaviour, IIngredientHolder
    {
        [SF] private AttachableNode pivot;
        
        private IngredientProvider _ingredientProvider;
        
        [SF] private IngredientType availableType = IngredientType.Patty;
        [SF] private int maxHoldingCount = 1;
        
        private readonly Queue<Carriable> _holdingQueue = new();
        
        public bool IsFull => _holdingQueue.Count >= maxHoldingCount;
        public bool HasIngredient => _holdingQueue.Count > 0;
        public Ingredient.Ingredient FirstIngredient => _holdingQueue.TryPeek(out var peek) ? peek.NetworkObject.GetComponent<Ingredient.Ingredient>() : null;
        
        // [추후 수정] 주입받도록
        private void Construct(IngredientProvider provider)
        {
            this._ingredientProvider = provider;
        }
        
        private void Awake() // [임시]
        {
            InitComponents();
            _ingredientProvider = FindFirstObjectByType<IngredientProvider>();
        }
        
        public void InitComponents()
        {
            // 클라이언트면 콜라이더 isTrigger 키기
            
            var cookwareCarriable = this.GetComponentInChildren<Carriable>();
            var cookwareRb = this.GetComponentInChildren<Rigidbody>();

            cookwareCarriable?.Construct(cookwareRb);
        }
        
        public bool TryAdd(Carriable carriable)
        {
            if (IsFull) return false;
            if (!carriable.IsSpawned) return false;
            if (!carriable.NetworkObject.TryGetComponent(out Ingredient.Ingredient ingredient)) return false;
            if (!availableType.HasFlag(ingredient.Type)) return false;
            
            _holdingQueue.Enqueue(carriable);
            carriable.AttachTo(pivot);
            return true;
        }

        public void ClearHolder()
        {
            while (_holdingQueue.Count > 0)
            {
                var carriable = _holdingQueue.Dequeue();
                carriable.Detach();
                
                var ingredient = carriable.NetworkObject.GetComponent<Ingredient.Ingredient>();
                _ingredientProvider.ReleaseIngredient(ingredient);
                ingredient.NetworkObject.Despawn(false);
            }
            
            ClearCookwareRpc();
        }

        public Carriable TakeOutIngredient()
        {
            return _holdingQueue.TryDequeue(out var carriable) ? carriable : null;
        }

        [Rpc(SendTo.Everyone)]
        private void ClearCookwareRpc()
        {
            // 뭐 모델 동기화라든지... 해줄 거 있으면 해주기
        }
    }
}