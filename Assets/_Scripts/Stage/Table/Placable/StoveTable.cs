using System;
using System.Threading;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.UI.Widget.ProgressBar;
using _Scripts.Stage.UI.Widget.TableAlert;
using _Scripts.Wrapper;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table.Placable
{
    public class StoveTable : NetworkBehaviour, IPlacable, INetworkUpdateSystem
    {
        // Data
        [SF] private float dirtinessThreshold = 0.005f;
        [SF] private float preWarnDelay = 5f;
        [SF] private float warnDuration = 10f;
        // Component
        private AttachableSlot _tableSlot;
        // Dependency
        private PlacementBroker _placementBroker;
        private StageHub _stageHub;
        // Caching
        private Cookware _placedCookware;
        private ProgressBarWidget _activeBarWidget;
        private TableAlertWidget _activeAlertWidget;
        private CancellationTokenSource _warningCts;
        private TagHandle _itemTag;
        // Network Variable
        private readonly NetworkVariable<float> _sharedProgress = new();
        // Event
        private event Action OnFinished;
        // Property
        public Carriable PlacedItem => _placedCookware;
        private bool HasHeatTarget => _placedCookware != null && _placedCookware.HasIngredient;
        private TimeSpan PreWarnDelay => TimeSpan.FromSeconds(preWarnDelay);
        private TimeSpan WarnDuration => TimeSpan.FromSeconds(warnDuration);
        private bool IsHeating => OnFinished != null;
        private bool IsWarning => _warningCts != null;

        [Inject]
        private void Construct(
            PlacementBroker placementBroker,
            StageHub stageHub)
        {
            _placementBroker = placementBroker;
            _stageHub = stageHub;
            
            _itemTag = TagHandle.GetExistingTag("Item");
            
            _tableSlot = this.GetComponentInChildren<AttachableSlot>();
            _tableSlot.OnAttach += OnTableSlotAttached;
            _tableSlot.OnDetach += OnTableSlotDetached;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Ingredient ingredient) || !ingredient.IsThrowing) return;

            var result = _placementBroker.AcceptCase(ingredient, this);
            if (result.Reason is not null) Debug.LogWarning($"{result.Reason} [StoveTable{this.NetworkObjectId}_OnTrigger]");
        }
        
        #region NGO 관련 메서드
        public override void OnNetworkSpawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = CheckDirtiness;
            base.OnNetworkSpawn();
        }

        protected override void OnNetworkPostSpawn()
        {
            if (IsServer) AttachWithSpawn().Forget();
            
            base.OnNetworkPostSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = null;
            _tableSlot.OnAttach -= OnTableSlotAttached;
            _tableSlot.OnDetach -= OnTableSlotDetached;
            
            base.OnNetworkPreDespawn();
        }
        
        private void OnTableSlotAttached(Carriable item)
        {
            if (item is not Cookware cookware) return;
            
            cookware.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            if (!IsServer) return;
            
            _placedCookware = cookware;
            _placedCookware.HolderSlot.OnAttach += OnCookwareSlotAttached;
            _placedCookware.HolderSlot.OnDetach += OnCookwareSlotDetached;
            
            if (!_placedCookware.HasIngredient) return;
            
            if (_placedCookware.HeldIngredient.IsRaw) StartHeat();
            if (_placedCookware.HeldIngredient.IsWellPrepped) WarnOverHeat().Forget();
            
            // 불 vfx 켜기 rpc
        }

        private void OnTableSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            
            if (IsHeating) CancelHeat();
            if (IsWarning) CancelWarn();
            
            _placedCookware.HolderSlot.OnAttach -= OnCookwareSlotAttached;
            _placedCookware.HolderSlot.OnDetach -= OnCookwareSlotDetached;
            _placedCookware = null;
            
            // 불 vfx 끄기 rpc
        }

        private void OnCookwareSlotAttached(Carriable item)
        {
            if (!IsServer || item is not Ingredient ingredient || ingredient.IsMaxPrepped) return;
            
            if (ingredient.IsRaw) StartHeat();
            if (ingredient.IsWellPrepped) WarnOverHeat().Forget();
            
            // 불 vfx 켜기 rpc
        }

        private void OnCookwareSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            
            if (IsHeating) CancelHeat();
            if (IsWarning) CancelWarn();
            
            // 불 vfx 끄기 rpc
        }

        private bool CheckDirtiness(in float prev, in float next)
        {
            return Mathf.Abs(next - prev) >= dirtinessThreshold;
        }
        
        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!HasHeatTarget) return;
            
            float progress = _placedCookware.HeldIngredient.Prepare(1);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f) FinishHeat();
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

            if (item is not Cookware)
            {
                rejectMessage = "Cookware가 아닌 아이템은 Place할 수 없습니다.";
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

            return true;
        }
        
        private async UniTaskVoid AttachWithSpawn()
        {
            var provider = _stageHub.FetchProvider<CookwareProvider>();
            var cookware = provider.GetCookware(this.transform.position);
            cookware.NetworkObject.Spawn();
            
            await UniTask.Yield();
            this.Place(cookware);
        }
        #endregion
        
        #region Heat 관련 메서드
        private void StartHeat()
        {
            OnFinished += _placedCookware.HeldIngredient.OnPrepCompleted;
            
            ActivateProgressBarRpc();
            
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void CancelHeat()
        {
            OnFinished = null;

            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            DeactivateProgressBarRpc();
            
            _sharedProgress.Value = 0;
        }

        private void FinishHeat()
        {
            OnFinished?.Invoke();
            
            OnFinished = null;
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            DeactivateProgressBarRpc();
            
            _sharedProgress.Value = 0;
            
            WarnOverHeat().Forget();
        }

        private async UniTaskVoid WarnOverHeat()
        {
            if (_warningCts is null || _warningCts.IsCancellationRequested)
            {
                _warningCts?.Dispose();
                _warningCts = new CancellationTokenSource();
            }
            
            var canceled = await UniTask.Delay(PreWarnDelay, false, cancellationToken:_warningCts.Token).SuppressCancellationThrow();
            if (canceled) return;
            
            ActivateTableAlertRpc();
            
            canceled = await UniTask.Delay(WarnDuration, false, cancellationToken:_warningCts.Token).SuppressCancellationThrow();

            DeactivateTableAlertRpc();
            
            if (!canceled && HasHeatTarget)
            {
                _placedCookware.HeldIngredient.OnOverCooked();
            }
        }

        private void CancelWarn()
        {
            _warningCts?.Cancel();
            _warningCts?.Dispose();
            _warningCts = null;
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
        
        [Rpc(SendTo.Everyone)]
        private void ActivateTableAlertRpc()
        {
            if (_activeAlertWidget != null) return;
            
            var provider = _stageHub.FetchProvider<TableAlertProvider>();
            _activeAlertWidget = provider.GetWidget(this.transform.position);
        }

        [Rpc(SendTo.Everyone)]
        private void DeactivateTableAlertRpc()
        {
            if (_activeAlertWidget == null) return;
            
            var provider = _stageHub.FetchProvider<TableAlertProvider>();
            provider.ReleaseWidget(_activeAlertWidget);
            _activeAlertWidget = null;
        }
        #endregion
    }
}