using System.Collections.Generic;
using _Scripts.Stage._Data.Item;
using _Scripts.Stage._Enums;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.UI.Widget.PlatingIcon;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Plate
{
    public class Plate : Carriable, IPrepable, IIngredientHolder
    {
        // Data
        private PlateData _data;
        // Component
        [SF] private MeshFilter platingModel;
        private StageHub _stageHub;
        // Status
        private IngredientType _platingMask = 0;
        private float _curProgress;
        private PrepState _curState;
        // Caching
        private readonly List<IngredientType> _platingList = new();
        private PlatingIconWidget _activeIconWidget;
        // Property
        private bool IsFull => _platingList.Count >= _data.MaxPlatingCount;
        public bool IsWellPrepped => _curState == PrepState.WellDone;
        public bool HasIngredient => _platingList.Count > 0;
        public IngredientType Plating => _platingMask;
        
        [Inject]
        public void Construct(
            PlateData data,
            StageHub stageHub)
        {
            this._data = data;
            this._stageHub = stageHub;
        }
        
        public override void Despawn()
        {
            if (!IsServer) return;

            var provider = StageHub.FetchProvider<PlateProvider>();
            provider.Release(this);
            
            base.Despawn();
        }
        
        #region NGO 관련 메서드
        public override void OnNetworkSpawn()
        {
            if (HasAuthority)
            {
                UpdatePlatingRpc(_platingMask);
            }
            
            base.OnNetworkSpawn();
        }
        #endregion

        #region Prep 관련 메서드
        public float Prepare(int multiplier)
        {
            if (_curProgress >= _data.MaxProgress) return 1;
            
            _curProgress += Time.deltaTime * multiplier;
            return _curProgress / _data.MaxProgress;
        }

        public void OnPrepCompleted()
        {
            if (!HasAuthority) return;
            
            _curProgress = 1;
            _curState = PrepState.WellDone;
            
            MakeCleanRpc();
        }
        
        [Rpc(SendTo.Everyone)]
        private void MakeCleanRpc()
        {
            platingModel.gameObject.SetActive(false);
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
            
            if (!this.IsWellPrepped || IsFull)
            {
                rejectMessage = "Plate가 더럽거나 이미 꽉 찬 상태입니다.";    
                return false;
            }

            if (!ingredient.IsWellPrepped)
            {
                rejectMessage = "조리되지 않은 Ingredient는 Plating할 수 없습니다.";
                return false;
            }

            if (IsAlreadyHeld(ingredient.Type))
            {
                rejectMessage = "이미 Plating된 Type의 Ingredient입니다.";
                return false;
            }

            if (!CheckRequirement(ingredient.Type))
            {
                rejectMessage = "필수 Ingredient가 Plating되지 않았습니다.";
                return false;
            }
            
            return true;
        }

        public void Hold(Ingredient.Ingredient ingredient)
        {
            if (ingredient.IsCarrying) ingredient.Detach();
            
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            provider.Release(ingredient);
            ingredient.NetworkObject.Despawn(false);
            
            if (!HasIngredient) ActivateIconRpc();
            
            _platingMask |= ingredient.Type;
            _platingList.Add(ingredient.Type);
            
            UpdatePlatingRpc(_platingMask);
            UpdateIconRpc(_platingList.Count-1, ingredient.Type);
        }
        
        public void ClearHolder(bool beClean)
        {
            if (!HasAuthority) return;
            
            _curProgress = beClean ? 1 : 0;
            _curState = beClean? PrepState.WellDone : PrepState.Raw;
            
            _platingMask = 0;
            _platingList.Clear();
            
            if (beClean) MakeCleanRpc();
            else MakeDirtyRpc();
            
            DeactivateIconRpc();
        }
        
        private bool IsAlreadyHeld(IngredientType type)
        {
            return _platingMask.HasFlag(type);
        }

        private bool CheckRequirement(IngredientType type)
        {
            var provider = _stageHub.FetchProvider<IngredientProvider>();
            var requiredType = provider.RequiredType;
            
            if (IsAlreadyHeld(requiredType)) return true;
            if (_platingList.Count < _data.MaxPlatingCount - 1) return true;
            
            return type == requiredType;
        }
        
        [Rpc(SendTo.Everyone)]
        private void UpdatePlatingRpc(IngredientType key)
        {
            platingModel.sharedMesh = _data.GetMesh(key);
            platingModel.transform.localScale = key <= 0 ? _data.FoodWasteLocalScale : _data.PlatingLocalScale;
            platingModel.transform.localPosition = _data.PlatingLocalPos;
            if (!platingModel.gameObject.activeSelf) platingModel.gameObject.SetActive(true);
        }
        
        [Rpc(SendTo.Everyone)]
        private void MakeDirtyRpc()
        {
            platingModel.sharedMesh = _data.FoodWasteMesh;
            platingModel.transform.localScale = _data.FoodWasteLocalScale;
            if (!platingModel.gameObject.activeSelf) platingModel.gameObject.SetActive(true);
        }
        #endregion

        #region UI 관련 메서드
        [Rpc(SendTo.Everyone)]
        private void ActivateIconRpc()
        {
            if (_activeIconWidget != null) return;

            var provider = _stageHub.FetchProvider<PlatingIconProvider>();
            _activeIconWidget = provider.GetWidget(this.transform.position);
            _activeIconWidget.AssignTo(this.transform);
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateIconRpc(int idx, IngredientType type)
        {
            if (_activeIconWidget == null) return;
            
            _activeIconWidget.AddIcon(idx, type);
        }

        [Rpc(SendTo.Everyone)]
        private void DeactivateIconRpc()
        {
            if (_activeIconWidget == null) return;
            
            var provider = _stageHub.FetchProvider<PlatingIconProvider>();
            provider.ReleaseWidget(_activeIconWidget);
            _activeIconWidget = null;
        }
        #endregion
    }
}