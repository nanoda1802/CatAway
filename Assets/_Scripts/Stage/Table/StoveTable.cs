using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.UI.Movable;
using _Scripts.Stage.UI.Widget;
using _Scripts.Stage.UI.Widget.ProgressBar;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class StoveTable : NetworkBehaviour, IPlacable, INetworkUpdateSystem
    {
        /* 컴포넌트 */
        private AttachableNode _pivot;
        private WidgetProvider<ProgressBarWidget> _widgetProvider;
        /* 캐싱 */
        private Carriable _placedItem;
        private Cookware _targetCookware;
        private IPrepable _targetIngredient;
        private ProgressBarWidget _activeBarWidget;
        /* 네트워크 */
        private readonly NetworkVariable<float> _sharedProgress = new();
        /* 기타 */
        private TagHandle _itemTag;
        private event Action OnFinished;
        /* 프로퍼티 */
        public bool IsHeating => _targetIngredient != null;
        public Carriable PlacedItem => _placedItem;
        
        [Inject]
        private void Construct(IBufferedSubscriber<ProgressBarProvider> subscriber)
        {
           subscriber.Subscribe(msg =>
            {
                _widgetProvider = msg;
                Debug.Log($"[{_widgetProvider is not null}] StoveTable에 widgetProvider 주입");
            });
            
            _pivot = GetComponentInChildren<AttachableNode>();
            _itemTag = TagHandle.GetExistingTag("Item");
        }

        #region 유니티 이벤트 메서드
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Throwable throwable) || !throwable.IsThrowing) return;
            if (!other.TryGetComponent(out Carriable carriable) || carriable.IsAttach) return;

            TryPlace(carriable);
        }
        #endregion

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
            if (!IsHeating) return;
            
            float progress = _targetIngredient.Prepare(1);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f) FinishHeat();
        }
        #endregion

        #region RPC 메서드
        [Rpc(SendTo.Everyone)]
        private void StartHeatRpc()
        {
            var widget = _activeBarWidget ?? ActivateWidget();
            _sharedProgress.OnValueChanged = widget.UpdateProgress;
        }
        
        [Rpc(SendTo.Everyone)]
        private void CancelHeatRpc()
        {
            DeactivateWidget();
            _sharedProgress.OnValueChanged = null;
        }

        [Rpc(SendTo.Everyone)]
        private void FinishHeatRpc()
        {
            DeactivateWidget();
            _sharedProgress.OnValueChanged = null;
        }
        #endregion
        
        #region Placable 관련 메서드
         public bool TryPlace(Carriable item)
        {
            if (item == null || !item.IsSpawned) return false;
            if (!CanPlaceItem(item)) return false;

            if (_targetCookware.IsFull) StartHeat();
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            displacedItem = null;
            if (!_pivot.HasAttachments || _targetCookware == null || _placedItem == null) return false;

            CancelHeat();

            if (carrier.HasAttachments)
            {
                if (CanDisplaceAdditionalItem(ref displacedItem)) return true;
            }
            
            _placedItem.Detach();
            displacedItem = _placedItem;
            
            _placedItem = null;
            _targetCookware = null;
            
            return true;
        }

        private bool CanPlaceItem(Carriable item)
        {
            switch (item.Type)
            {
                case CarriableType.Cookware:
                    if (_pivot.HasAttachments || _placedItem != null) return false;
                    if (!item.NetworkObject.TryGetComponent(out _targetCookware)) return false;
                    item.AttachTo(_pivot);
                    _placedItem = item;
                    return true;
                
                case CarriableType.Ingredient:
                    if (_targetCookware == null || _targetCookware.IsFull) return false;
                    return _targetCookware.TryAdd(item);
                
                case CarriableType.Plate:
                    return false;
                
                default:
                    Debug.LogError($"[{this.OwnerClientId} StoveTable.TryPlace] \"{item.Type}\"은 존재하지 않는 CarriableType 입니다.");
                    return false;
            }
        }

        private bool CanDisplaceAdditionalItem(ref Carriable item)
        {
            if (!_targetCookware.HasIngredient) return false;
            
            item = _targetCookware.TakeOutIngredient();
            item?.Detach();
            
            return item is not null;
        }
        #endregion
        
        #region Heat 관련 메서드
        private void StartHeat()
        {
            if (_targetCookware == null || !_targetCookware.HasIngredient) return;
            
            _targetIngredient = _targetCookware.FirstIngredient;
            if (_targetIngredient is null) return;
                
            OnFinished += _targetIngredient.OnPrepFinished;
                
            StartHeatRpc();
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void CancelHeat()
        {
            if (!IsHeating) return;
            
            CancelHeatRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _targetIngredient = null;
            OnFinished = null;
        }

        private void FinishHeat()
        {
            FinishHeatRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            OnFinished?.Invoke();
            _sharedProgress.Value = 0;
            
            _targetIngredient = null;
            OnFinished = null;
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