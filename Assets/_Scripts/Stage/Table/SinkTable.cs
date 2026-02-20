using System;
using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.UI.Movable;
using _Scripts.Stage.UI.Widget;
using _Scripts.Stage.UI.Widget.ProgressBar;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class SinkTable : NetworkBehaviour, IPlacable, IInteractable, INetworkUpdateSystem
    {
        /* 컴포넌트 */
        private PlateProvider _plateProvider;
        private WidgetProvider<ProgressBarWidget> _widgetProvider;
        /* 캐싱 */
        private Plate _washTarget;
        private ProgressBarWidget _activeBarWidget;
        private readonly List<ulong> _interactorList = new();
        /* 네트워크 */
        private readonly NetworkVariable<float> _sharedProgress = new();
        /* 기타 */
        private TableHub _tableHub;
        private event Action OnFinished;
        private readonly int _washAnimParamHash = Animator.StringToHash("WashDish");
        /* 프로퍼티 */
        public bool IsInteracting => _interactorList.Count > 0;
        public Carriable PlacedItem => null;

        [Inject]
        private void Construct(
            PlateProvider plateProvider,
            TableHub tableHub,
            IBufferedSubscriber<ProgressBarProvider> sub)
        {
            _plateProvider = plateProvider;
            _tableHub = tableHub;
            
            sub.Subscribe(msg =>
            {
                _widgetProvider = msg;
                Debug.Log($"[{_widgetProvider is not null}] StoveTable에 widgetProvider 주입");
            });
        }
        
        #region 네트워크 이벤트 관련 메서드
        public override void OnNetworkSpawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = CheckDirtiness;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = null;
            base.OnNetworkDespawn();
        }
        
        private bool CheckDirtiness(in float prev, in float next)
        {
            return Mathf.Abs(next - prev) >= 0.005f;
        }
        
        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!IsInteracting) return;
            
            float progress = _washTarget.Prepare(_interactorList.Count);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f) FinishInteraction();
        }
        #endregion

        #region RPC 메서드
        [Rpc(SendTo.Everyone)]
        private void PlaceRpc()
        {
            // [추가] provider.HasInactivePlate가 true 면 싱크대 에 접시 오브젝트 보여주기
            Debug.Log("There are Plates to Wash!!");
        }

        [Rpc(SendTo.Everyone)]
        private void StartInteractionRpc()
        {
            var widget = _activeBarWidget ?? ActivateWidget();
            _sharedProgress.OnValueChanged = widget.UpdateProgress;
        }
        
        [Rpc(SendTo.Everyone)]
        private void CancelInteractionRpc()
        {
            _sharedProgress.OnValueChanged = null;
        }

        [Rpc(SendTo.Everyone)]
        private void FinishInteractionRpc()
        {
            DeactivateWidget();
            _sharedProgress.OnValueChanged = null;
        }
        #endregion
        
        #region Placable 관련 메서드
        public bool TryPlace(Carriable item)
        {
            if (item == null || !item.IsSpawned) return false;
            if (item.Type != CarriableType.Plate) return false;
            if (!item.NetworkObject.TryGetComponent(out Plate plate) || plate.IsReady) return false;
            
            _plateProvider.ReleasePlate(plate);
            plate.NetworkObject.Despawn(false);
            
            if (_plateProvider.HasInactivePlate) PlaceRpc();
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            displacedItem = null;
            return false;
        }
        #endregion
        
        #region Interactable 관련 메서드
        public bool TryInteraction(InteractionBehaviour interactor, out int animParamHash)
        {
            animParamHash = -1;
            
            if (_washTarget is null)
            {
                if (!_plateProvider.HasInactivePlate) return false;
                _washTarget = _plateProvider.GetPlate(transform.position + 100 * Vector3.down);
                _washTarget.NetworkObject.Spawn();
                // [수정] 그리고 얘를 끄던가 어디 숨겨놓던가 해야하는데 아무튼...
                // 일단 엄청 밑에 넣어두자
                // 계속 떨어지고 있겠네...
            }
            
            animParamHash = _washAnimParamHash;
            
            _interactorList.Add(interactor.OwnerClientId);
            OnFinished += interactor.FinishInteractionRpc;
            OnFinished -= _washTarget.OnPrepFinished; // 상호작용은 여러 명이 해도, Plate는 하나라서 방어용
            OnFinished += _washTarget.OnPrepFinished;
            
            StartInteractionRpc();
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            
            return true;
        }

        public void CancelInteraction(ulong clientId)
        {
            CancelInteractionRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            OnFinished = null;
            _interactorList.Remove(clientId);
        }
        
        public void FinishInteraction()
        {
            FinishInteractionRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            OnFinished?.Invoke();

            var carriable = _washTarget.GetComponentInChildren<Carriable>();
            _tableHub.Fetch<PlateRackTable>()?.TryPlace(carriable);
            
            _washTarget = null;
            _sharedProgress.Value = 0;
            OnFinished = null;
            _interactorList.Clear();
        }
        #endregion
        
        #region UI 관련 메서드
        private ProgressBarWidget ActivateWidget()
        {
            var widget = _widgetProvider.GetWidget(this.transform.position);
            _activeBarWidget = widget;
            return widget;
        }

        private void DeactivateWidget()
        {
            if (_activeBarWidget == null) return;
            
            _widgetProvider.ReleaseWidget(_activeBarWidget);
            _activeBarWidget = null;
        }
        #endregion
    }
}