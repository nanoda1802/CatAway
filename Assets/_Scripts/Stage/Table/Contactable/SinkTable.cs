using System;
using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.Table.Placable;
using _Scripts.Stage.UI.Widget;
using _Scripts.Stage.UI.Widget.ProgressBar;
using _Scripts.Wrapper;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Contactable
{
    public class SinkTable : NetworkBehaviour, IInteractable, INetworkUpdateSystem, IContactable
    {
        // Data
        [SF] private float dirtinessThreshold = 0.005f;
        private readonly int _washAnimParamHash = Animator.StringToHash("WashDish");
        // Dependency
        private AttachableSlot _tableSlot;
        private StageHub _stageHub;
        // Caching
        private Plate _targetPlate;
        private ProgressBarWidget _activeBarWidget;
        private readonly List<ulong> _interactorList = new();
        // Network Variable
        private readonly NetworkVariable<float> _sharedProgress = new();
        // Event?
        private event Action OnFinished;
        // Property
        public bool IsInteracting => _interactorList.Count > 0 && _targetPlate != null;

        [Inject]
        private void Construct(StageHub stageHub)
        {
            _stageHub = stageHub;
            
            _tableSlot = GetComponentInChildren<AttachableSlot>();
            _tableSlot.OnAttach += OnSlotAttached;
            _tableSlot.OnDetach += OnSlotDetached;
        }
        
        #region NGO 관련 메서드
        public override void OnNetworkSpawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = CheckDirtiness;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = null;
            _tableSlot.OnAttach -= OnSlotAttached;
            _tableSlot.OnDetach -= OnSlotDetached;
            
            base.OnNetworkDespawn();
        }
        
        private bool CheckDirtiness(in float prev, in float next)
        {
            return Mathf.Abs(next - prev) >= dirtinessThreshold;
        }
        
        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (_interactorList.Count <= 0) return;
            
            float progress = _targetPlate.Prepare(_interactorList.Count);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f) FinishInteraction();
        }

        private void OnSlotAttached(AttachableBehaviour attachableBehaviour)
        {
            if (!IsServer || attachableBehaviour is not Plate plate) return;
            
            ActivateProgressBarRpc();
            OnFinished += plate.OnPrepCompleted;
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void OnSlotDetached(AttachableBehaviour attachableBehaviour)
        {
            if (!IsServer || attachableBehaviour is not Plate plate) return;
            
            DeactivateProgressBarRpc();
            OnFinished = null;
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _targetPlate =  null;
            _interactorList.Clear();
            _sharedProgress.Value = 0;
        }
        #endregion

        #region Contactable 관련 메서드
        public bool TryContact(Carriable item, out string failMessage)
        {
            failMessage = null;
            
            if (item is Plate { IsWellPrepped : false })
            {
                return true;
            }
            
            failMessage = "Sink엔 더러워진 Plate만 제출할 수 있습니다.";
            return false;
        }

        public void RespondTo(Plate plate)
        {
            if (plate.IsCarrying) plate.Detach();
            var provider = _stageHub.FetchProvider<PlateProvider>();
            provider.ReleasePlate(plate);
            plate.NetworkObject.Despawn(false);
        }
        #endregion
        
        #region Interactable 관련 메서드
        public bool TryInteraction(InteractionBehaviour interactor, out int animParamHash)
        {
            animParamHash = -1;
            
            if (_targetPlate == null)
            {
                var provider = _stageHub.FetchProvider<PlateProvider>();
                if (!provider.HasInactivePlate) return false;
                
                _targetPlate = provider.GetPlate(transform.position);
                _targetPlate.NetworkObject.Spawn();
                _targetPlate.Attach(_tableSlot);
            }
            
            animParamHash = _washAnimParamHash;
            
            _interactorList.Add(interactor.OwnerClientId);
            OnFinished += interactor.FinishRpc;
            
            return true;
        }

        public void CancelInteraction(InteractionBehaviour interactor)
        {
            _interactorList.Remove(interactor.OwnerClientId);
            OnFinished -= interactor.FinishRpc;
        }
        
        public void FinishInteraction()
        {
            OnFinished?.Invoke();

            var plateRack = _stageHub.FetchPlacable<PlateRackTable>();
            plateRack.Place(_targetPlate);
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
    }
}