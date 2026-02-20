using System.Collections.Generic;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.Table;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Plate
{
    public class Plate : NetworkBehaviour, IPrepable, IIngredientHolder
    {
        private PlateData _data;
        private IngredientProvider _ingredientProvider;
        
        private readonly List<IngredientType> _platingList = new();
        private IngredientType _platingMask = 0;
        [SF] private MeshFilter platingModel;
        
        private float _curProgress;
        private PrepState _prepState; // 이걸로 동기화 다시 해야해
        
        public bool IsReady => _prepState == PrepState.WellDone;
        public bool IsFull => _platingList.Count >= _data.MaxPlatingCount;
        public bool HasIngredient => _platingList.Count > 0;
        
        // [추후 수정] 주입받도록
        private void Construct(IngredientProvider provider, PlateData data) // 등등
        {
            this._ingredientProvider = provider;
        }

        public void InitComponents(bool isServer, PlateData data)
        {
            _data = data;
            _ingredientProvider = FindFirstObjectByType<IngredientProvider>(); // [임시]
            
            var plateCarriable = this.GetComponentInChildren<Carriable>();
            var plateRb = this.GetComponentInChildren<Rigidbody>();

            plateRb.detectCollisions = isServer;
            
            plateCarriable?.Construct(plateRb);

            var plateCollider = this.GetComponentInChildren<Collider>();
            plateCollider.isTrigger = !isServer;

            if (platingModel == null)
            {
                platingModel = transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>();
            }

            platingModel.gameObject.SetActive(false);
            platingModel.transform.localPosition = data.PlatingLocalPos;
            platingModel.transform.localScale = data.PlatingLocalScale;
        }

        public float Prepare(int multiplier)
        {
            if (_curProgress >= _data.MaxProgress) return 1;
            
            _curProgress += Time.deltaTime * multiplier;
            return _curProgress / _data.MaxProgress;
        }

        public void OnPrepFinished()
        {
            InitStatus();
        }

        public bool TryAdd(Carriable carriable)
        {
            if (!IsReady || IsFull) return false;
            if (!carriable.IsSpawned) return false;
            if (!carriable.NetworkObject.TryGetComponent(out Ingredient.Ingredient ingredient)) return false;
            
            if (_platingMask.HasFlag(ingredient.Type))  return false;
            if (!ingredient.IsReady) return false;
            if (!HasRequiredIngredient(ingredient.Type)) return false;
            
            if (carriable.IsAttach) carriable.Detach();
            _ingredientProvider.ReleaseIngredient(ingredient);
            ingredient.NetworkObject.Despawn(false);
            
            _platingMask |= ingredient.Type;
            _platingList.Add(ingredient.Type);
            
            UpdatePlatingRpc(_platingMask);
            
            return true;
        }

        private bool HasRequiredIngredient(IngredientType type)
        {
            var requiredIngredient = _ingredientProvider.RequiredType;
            
            if (_platingMask.HasFlag(requiredIngredient)) return true;
            if (_platingList.Count < _data.MaxPlatingCount - 1) return true;
            
            return type == requiredIngredient;
        }

        public void ClearHolder()
        {
            if (!IsReady) return;
            _curProgress = 0;
            _prepState = PrepState.Raw;
            ClearPlatingRpc();
        }

        public Carriable TakeOutIngredient()
        {
            return null;
        }

        public void InitStatus()
        {
            _curProgress = 1;
            _prepState = PrepState.WellDone;
            
            _platingMask = 0;
            _platingList.Clear();
            
            ResetStatusRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void UpdatePlatingRpc(IngredientType key)
        {
            if (!platingModel.gameObject.activeSelf) platingModel.gameObject.SetActive(true);
            platingModel.sharedMesh = _data.GetMesh(key);
            platingModel.transform.localScale = _data.PlatingLocalScale;
        }

        [Rpc(SendTo.Everyone)]
        private void ClearPlatingRpc()
        {
            if (!platingModel.gameObject.activeSelf) platingModel.gameObject.SetActive(true);
            platingModel.sharedMesh = _data.FoodWasteMesh;
            platingModel.transform.localScale = _data.FoodWasteLocalScale;
        }
        
        [Rpc(SendTo.Everyone)]
        private void ResetStatusRpc()
        {
            platingModel.gameObject.SetActive(false);
        }
    }
}