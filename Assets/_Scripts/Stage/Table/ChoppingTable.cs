using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.UI.Widget.ProgressBar;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class ChoppingTable : NetworkBehaviour, IPlacable, IInteractable, INetworkUpdateSystem
    {
        /* 데이터 */
        [SF] private IngredientType availableType = IngredientType.Lettuce | IngredientType.Cheese | IngredientType.Tomato;
        /* 컴포넌트 */
        [SF] private GameObject knifeModel;
        private AttachableNode _pivot;
        private ProgressBarProvider _widgetProvider;
        /* 캐싱 */
        private Carriable _placedItem;
        private IPrepable _targetIngredient;
        private ProgressBarWidget _activeBarWidget;
        /* 네트워크 */
        private readonly NetworkVariable<float> _sharedProgress = new();
        /* 기타 */
        private TagHandle _itemTag;
        private event Action OnFinished;
        private readonly int _chopAnimParamHash = Animator.StringToHash("Chop");
        /* 프로퍼티 */
        public bool IsInteracting => _targetIngredient != null;
        public Carriable PlacedItem => _placedItem;

        [Inject]
        private void Construct(IBufferedSubscriber<ProgressBarProvider> sub)
        {
            sub.Subscribe(msg =>
            {
                _widgetProvider = msg;
                Debug.Log($"[{_widgetProvider is not null}] ChoppingTable에 widgetProvider 주입");
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
            if (!IsInteracting) return;
            
            float progress = _targetIngredient.Prepare(1);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f) FinishInteraction();
        }
        #endregion

        #region RPC 메서드
        [Rpc(SendTo.Everyone)]
        private void PlaceRpc()
        {
            knifeModel.SetActive(false);
        }
        
        [Rpc(SendTo.Everyone)]
        private void DisplaceRpc()
        {
            knifeModel.SetActive(true);
            
            DeactivateWidget();
            _sharedProgress.OnValueChanged = null;
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
            if (_pivot.HasAttachments || _placedItem is not null) return false;
            if (!item.NetworkObject.TryGetComponent(out Ingredient ingredient)) return false;
            if (!availableType.HasFlag(ingredient.Type)) return false;
            
            item.AttachTo(_pivot);
            _placedItem = item;
            
            PlaceRpc();
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable displacedItem)
        {
            displacedItem = null;
            if (carrier == null || !carrier.IsSpawned) return false;
            if (!_pivot.HasAttachments || _placedItem is null) return false;
            
            displacedItem = _placedItem;
            
            _placedItem.Detach();
            _placedItem = null;
            
            DisplaceRpc();
            
            return true;
        }
        #endregion
        
        #region Interactable 관련 메서드
        public bool TryInteraction(InteractionBehaviour interactor, out int animParamHash)
        {
            animParamHash = -1;
            
            if (IsInteracting) return false;
            if (!_pivot.HasAttachments || _placedItem is null) return false;
            if (!_placedItem.NetworkObject.TryGetComponent(out _targetIngredient)) return false;
            if (_targetIngredient.IsReady) return false;
            
            animParamHash = _chopAnimParamHash;
            
            OnFinished += interactor.FinishInteractionRpc;
            OnFinished += _targetIngredient.OnPrepFinished;
            
            StartInteractionRpc();
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            
            return true;
        }

        public void CancelInteraction(ulong clientId)
        {
            CancelInteractionRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _targetIngredient = null;
            OnFinished = null;
        }

        public void FinishInteraction()
        {
            FinishInteractionRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            OnFinished?.Invoke();
            
            _targetIngredient = null;
            _sharedProgress.Value = 0;
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