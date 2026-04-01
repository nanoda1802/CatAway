using System;
using System.Collections.Generic;
using _Scripts._Helper;
using _Scripts._Wrapper;
using _Scripts.Scene_Stage.Enums;
using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Ingredient;
using _Scripts.Scene_Stage.Player.Behaviour;
using _Scripts.Scene_Stage.UI.Widget.ProgressBar;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Table.Placable
{
    public class ChoppingTable : NetworkBehaviour, IPlacable, IInteractable, INetworkUpdateSystem
    {
        [SF] private ParticleSystem chopVfx;
        // Data
        [SF] private float dirtinessThreshold = 0.005f;
        [SF] private IngredientType ingredientMask = IngredientType.Lettuce | IngredientType.Cheese | IngredientType.Tomato;
        private readonly int _chopAnimParamHash = Animator.StringToHash("Chop");
        // Components
        private AttachableSlot _tableSlot;
        // Dependency
        private PlacementBroker _placementBroker;
        private StageHub _stageHub;
        private VfxHandler _vfxHandler;
        // Caching
        [SF] private GameObject knifeModel;
        private Ingredient _targetIngredient;
        private ProgressBarWidget _activeBarWidget;
        private readonly List<ulong> _interactorList = new();
        private TagHandle _itemTag;
        // Network Variable
        private readonly NetworkVariable<float> _sharedProgress = new();
        // Event
        private event Action OnFinished;
        // Property
        public bool IsInteracting => _interactorList.Count > 0 && _targetIngredient != null;
        public Carriable PlacedItem => _targetIngredient;

        [Inject]
        private void Construct(
            PlacementBroker placementBroker,
            StageHub stageHub,
            VfxHandler vfxHandler)
        {
            _placementBroker = placementBroker;
            _stageHub = stageHub;
            _vfxHandler = vfxHandler;
            
            _itemTag = TagHandle.GetExistingTag("Item");
            
            _tableSlot = this.GetComponentInChildren<AttachableSlot>();
            _tableSlot.OnAttach += OnSlotAttached;
            _tableSlot.OnDetach += OnSlotDetached;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Ingredient ingredient) || !ingredient.IsThrowing) return;

            var result = _placementBroker.AcceptCase(ingredient, this);
            if (result.Reason is not null) Debug.LogWarning($"{result.Reason} [ChoppingTable{this.NetworkObjectId}_OnTrigger]");
        }
        
        #region NGO 관련 메서드
        public override void OnNetworkSpawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = CheckDirtiness;
            base.OnNetworkSpawn();
        }
        
        public override void OnNetworkPreDespawn()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _sharedProgress.OnValueChanged = null;
            _sharedProgress.CheckExceedsDirtinessThreshold = null;
            _tableSlot.OnAttach -= OnSlotAttached;
            _tableSlot.OnDetach -= OnSlotDetached;
            
            _vfxHandler.StopImmediately(chopVfx);
            
            base.OnNetworkPreDespawn();
        }

        private void OnSlotAttached(Carriable item)
        {
            if (item is not Ingredient ingredient) return;
            
            ingredient.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (!IsServer) return;
            
            PlaceRpc();
            _targetIngredient = ingredient;
        }

        private void OnSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            
            DisplaceRpc();
            DeactivateProgressBarRpc();
            _targetIngredient = null;
        }
        
        private bool CheckDirtiness(in float prev, in float next)
        {
            return Mathf.Abs(next - prev) >= dirtinessThreshold;
        }
        
        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!IsInteracting) return;
            
            float progress = _targetIngredient.Prepare(1);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f) FinishInteraction();
        }
        #endregion
        
        #region Placable 관련 메서드
        public void Place(Carriable item)
        {
            if (item.IsCarrying) item.Detach();
            item.Attach(_tableSlot);
        }
        
        public bool CanPlace(Carriable item, out string rejectMessage)
        {
            rejectMessage = null;
            
            if (item == null || !item.IsSpawned)
            {
                rejectMessage = "Item이 null이거나 Spawn되지 않은 상태입니다.";
                return false;
            }

            if (item is not Ingredient ingredient)
            {
                rejectMessage = "Ingredient만 Place할 수 있습니다.";
                return false;
            }

            if (!IsAvailableType(ingredient.Type))
            {
                rejectMessage = $"{ingredient.Type}은(는) Chop할 수 없는 Ingredient Type 입니다.";
                return false;
            }

            return true;
        }

        public bool CanDisPlace(out string rejectMessage)
        {
            rejectMessage = null;
            
            if (PlacedItem == null || !PlacedItem.IsSpawned)
            {
                rejectMessage = "PlacedItem이 null이거나 Spawn되지 않은 상태입니다.";
                return false;
            }

            if (IsInteracting)
            {
                rejectMessage = "누군가 PlacedItem을 Chop하는 중입니다.";
                return false;
            }

            return true;
        }
        
        private bool IsAvailableType(IngredientType type)
        {
            return ingredientMask.HasFlag(type);
        }
        
        [Rpc(SendTo.Everyone)]
        private void PlaceRpc()
        {
            knifeModel.SetActive(false);
        }
        
        [Rpc(SendTo.Everyone)]
        private void DisplaceRpc()
        {
            knifeModel.SetActive(true);
        }
        #endregion
        
        #region Interactable 관련 메서드
        public bool TryInteraction(InteractionBehaviour interactor, out int animParamHash)
        {
            animParamHash = -1;
            
            if (IsInteracting) return false;
            if (_targetIngredient == null || _targetIngredient.IsWellPrepped) return false;
            
            _interactorList.Add(interactor.OwnerClientId);
            animParamHash = _chopAnimParamHash;
            
            OnFinished += interactor.FinishRpc;
            OnFinished += _targetIngredient.OnPrepCompleted;
            
            ActivateProgressBarRpc();
            ActivateVfxRpc();
            
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            
            return true;
        }

        public void CancelInteraction(InteractionBehaviour interactor)
        {
            _interactorList.Remove(interactor.OwnerClientId);   
            
            OnFinished -= interactor.FinishRpc;
            OnFinished -= _targetIngredient.OnPrepCompleted;
            
            DeactivateVfxRpc();
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        public void FinishInteraction()
        {
            OnFinished?.Invoke();
            
            _interactorList.Clear();
            
            OnFinished = null;
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            DeactivateProgressBarRpc();
            DeactivateVfxRpc();
            
            _sharedProgress.Value = 0;
        }
        #endregion
        
        #region UI 관련 메서드
        [Rpc(SendTo.Everyone)]
        private void ActivateProgressBarRpc()
        {
            if (_activeBarWidget != null) return;
            
            var provider = _stageHub.FetchProvider<ProgressBarProvider>();
            _activeBarWidget = provider.GetWidget(this.transform.position);
            
            _sharedProgress.OnValueChanged = _activeBarWidget.UpdateProgress;
        }

        [Rpc(SendTo.Everyone)]
        private void DeactivateProgressBarRpc()
        {
            _sharedProgress.OnValueChanged = null;
            
            if (_activeBarWidget == null) return;
            
            var provider = _stageHub.FetchProvider<ProgressBarProvider>();
            provider.ReleaseWidget(_activeBarWidget);
            _activeBarWidget = null;
        }
        #endregion

        #region VFX 관련 메서드
        [Rpc(SendTo.Everyone)]
        private void ActivateVfxRpc()
        {
            _vfxHandler.PlayVfx(chopVfx);
        }

        [Rpc(SendTo.Everyone)]
        private void DeactivateVfxRpc()
        {
            _vfxHandler.StopSmoothly(chopVfx);
        }
        #endregion
    }
}