using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts._Helper;
using _Scripts._Shared.Sound;
using _Scripts._Wrapper;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Cookware;
using _Scripts.Scene_Stage.Item.Ingredient;
using _Scripts.Scene_Stage.UI.Widget.ProgressBar;
using _Scripts.Scene_Stage.UI.Widget.TableAlert;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Table.Placable
{
    public class StoveTable : NetworkBehaviour, IPlacable, INetworkUpdateSystem
    {
        [SF] private ParticleSystem fireVfx;
        // Data
        [SF] private float dirtinessThreshold = 0.005f;
        [SF] private float preWarnDelay = 5f;
        [SF] private float warnDuration = 10f;
        private StageSfxListData _sfxList;
        // Component
        private AttachableSlot _tableSlot;
        // Dependency
        private PlacementBroker _placementBroker;
        private StageHub _stageHub;
        private VfxHandler _vfxHandler;
        // Caching
        private Cookware _placedCookware;
        private ProgressBarWidget _activeBarWidget;
        private TableAlertWidget _activeAlertWidget;
        private SfxBuilder _activeSfx;
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
            StageSfxListData sfxListData,
            PlacementBroker placementBroker,
            StageHub stageHub,
            VfxHandler vfxHandler)
        {
            _sfxList = sfxListData;
            _placementBroker = placementBroker;
            _stageHub = stageHub;
            _vfxHandler = vfxHandler;
            
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
            if (IsServer) NetworkManager.SceneManager.OnLoadEventCompleted += OnLevelLoaded;
            _sharedProgress.CheckExceedsDirtinessThreshold = CheckDirtiness;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            if (IsServer) CancelWarn();
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _sharedProgress.OnValueChanged = null;
            _sharedProgress.CheckExceedsDirtinessThreshold = null;
            _tableSlot.OnAttach -= OnTableSlotAttached;
            _tableSlot.OnDetach -= OnTableSlotDetached;
            
            _vfxHandler.StopImmediately(fireVfx);
            _activeSfx?.Stop();
            
            base.OnNetworkPreDespawn();
        }
        
        // 씬 배치 네트워크오브젝트라서, 모든 클라이언트에서 스폰 완료된 안전한 시점이 이때 뿐...!
        private void OnLevelLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!NetworkManager.IsServer) return;
            if (!sceneName.StartsWith("Level")) return;
            
            AttachWithSpawn().Forget();
            
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLevelLoaded;
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
        }

        private void OnTableSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            
            if (IsHeating) CancelHeat();
            if (IsWarning) CancelWarn();
            
            _placedCookware.HolderSlot.OnAttach -= OnCookwareSlotAttached;
            _placedCookware.HolderSlot.OnDetach -= OnCookwareSlotDetached;
            _placedCookware = null;
        }

        private void OnCookwareSlotAttached(Carriable item)
        {
            if (!IsServer || item is not Ingredient ingredient || ingredient.IsMaxPrepped) return;
            
            if (ingredient.IsRaw) StartHeat();
            if (ingredient.IsWellPrepped) WarnOverHeat().Forget();
        }

        private void OnCookwareSlotDetached(Carriable item)
        {
            if (!IsServer) return;
            
            if (IsHeating) CancelHeat();
            if (IsWarning) CancelWarn();
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
            cookware.NetObj.Spawn(true);
            
            await UniTask.Yield();
            
            if (!this.IsSpawned) return;
            
            this.Place(cookware);
        }
        #endregion
        
        #region Heat 관련 메서드
        private void StartHeat()
        {
            OnFinished += _placedCookware.HeldIngredient.OnPrepCompleted;
            
            ActivateProgressBarRpc();
            ActivateFxRpc();
            
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void CancelHeat()
        {
            OnFinished = null;

            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            DeactivateProgressBarRpc();
            DeactivateFxRpc();
            
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
            
            if (!fireVfx.isPlaying) ActivateFxRpc();
            
            var canceled = await UniTask.Delay(PreWarnDelay, false, cancellationToken:_warningCts.Token).SuppressCancellationThrow();
            if (canceled) return;
            
            ActivateTableAlertRpc();
            
            canceled = await UniTask.Delay(WarnDuration, false, cancellationToken:_warningCts.Token).SuppressCancellationThrow();

            DeactivateTableAlertRpc(); // [수정] 스폰 상태일 때만 호출하ㅣ게 수정
            DeactivateFxRpc();
            
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
            
            DeactivateFxRpc();
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
        
        #region FX 관련 메서드
        [Rpc(SendTo.Everyone)]
        private void ActivateFxRpc()
        {
            _vfxHandler.PlayVfx(fireVfx);
            
            _activeSfx?.Stop();
            _activeSfx = _sfxList.Play(StageSfxType.Grill);
        }

        [Rpc(SendTo.Everyone)]
        private void DeactivateFxRpc()
        {
            _vfxHandler.StopSmoothly(fireVfx);
            _activeSfx?.Stop();
            _activeSfx = null;
        }
        #endregion
    }
}