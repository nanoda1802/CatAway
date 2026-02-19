using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Cookware;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.UI.Movable;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class StoveTable : NetworkBehaviour, IPlacable, INetworkUpdateSystem
    {
        [SF] private AttachableNode pivot;
        
        [SF] private ProgressIndicator indicatorPrefab; // [임시]
        [SF] private Canvas movableCanvas; // [임시]
        [SF] private float indicatorOffsetY = 1.5f;
        private ProgressIndicator _activeIndicator;
        
        private Carriable _placedItem;
        private Cookware _placedCookware;

        private readonly NetworkVariable<float> _sharedProgress = new();
        
        private event Action OnFinished;
        private TagHandle _itemTag;
        
        public bool IsWorking => _curTargetIngredient != null;
        
        public Carriable PlacedItem => _placedItem;

        private IPrepable _curTargetIngredient; // [임시]
        
        public override void OnNetworkSpawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = CheckDirtiness;
        
            _itemTag = TagHandle.GetExistingTag("Item");
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _sharedProgress.CheckExceedsDirtinessThreshold = null;
            
            base.OnNetworkDespawn();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.CompareTag(_itemTag)) return;
            if (!other.TryGetComponent(out Throwable throwable) || !throwable.IsThrowing) return;
            if (!other.TryGetComponent(out Carriable carriable) || carriable.IsAttach) return;

            if (TryPlace(carriable))
            {
                
            }
        }
        
        private bool CheckDirtiness(in float prev, in float next)
        {
            return Mathf.Abs(next - prev) >= 0.005f;
        }
        
        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!IsWorking) return;
            
            float progress = _curTargetIngredient.Prepare(1);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f)
            {
                FinishWork();
            }
        }
        
        public bool TryPlace(Carriable carriable)
        {
            if (carriable == null || !carriable.IsSpawned) return false;
            if (!CanPlaceItem(carriable)) return false;

            if (_placedCookware.IsFull)
            {
                StartWork();
            }
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            carriable = null;
            if (!pivot.HasAttachments || _placedItem == null || _placedCookware == null) return false;

            if (IsWorking) CancelWork();

            if (carrier.HasAttachments && CanDisplaceAdditionalItem(ref carriable)) return true;
            
            carriable = _placedItem;
            _placedItem.Detach();
            _placedItem = null;
            _placedCookware = null;
            
            return true;
        }

        private bool CanPlaceItem(Carriable item)
        {
            switch (item.Type)
            {
                case CarriableType.Cookware:
                    if (pivot.HasAttachments || _placedItem != null) return false;
                    if (!item.NetworkObject.TryGetComponent(out _placedCookware)) return false;
                    if (item.IsAttach) item.Detach();
                    item.Attach(pivot);
                    _placedItem = item;
                    return true;
                
                case CarriableType.Ingredient:
                    if (!pivot.HasAttachments || _placedItem == null) return false;
                    if (_placedCookware == null || _placedCookware.IsFull) return false;
                    return _placedCookware.TryAdd(item);
                
                case CarriableType.Plate:
                    return false;
                
                default:
                    Debug.LogError($"[{this.OwnerClientId} StoveTable.TryPlace] \"{item.Type}\"은 존재하지 않는 CarriableType 입니다.");
                    return false;
            }
        }

        private bool CanDisplaceAdditionalItem(ref Carriable item)
        {
            if (!_placedCookware.HasIngredient) return false;
            
            item = _placedCookware.TakeOutCarriable();
            item?.Detach();
            
            return item is not null;
        }

        private void StartWork()
        {
            if (!pivot.HasAttachments || _placedItem == null) return;
            if (_placedCookware == null || !_placedCookware.HasIngredient) return;
            
            _curTargetIngredient = _placedCookware.FirstIngredient;
            if (_curTargetIngredient is null) return;
                
            OnFinished += _curTargetIngredient.OnPrepFinished;
                
            StartWorkRpc();
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
        }

        private void CancelWork()
        {
            CancelWorkRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _curTargetIngredient = null;
            OnFinished = null;
        }

        private void FinishWork()
        {
            FinishWorkRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            OnFinished?.Invoke();
            _sharedProgress.Value = 0;
            
            _curTargetIngredient = null;
            OnFinished = null;
        }

        private ProgressIndicator ActivateIndicator()
        {
            // pool에서 하나 Get (추후 수정)
            var indicator = Instantiate(indicatorPrefab, movableCanvas.transform);
            
            // table의 월드 위치에 오프셋 더한 좌표 전달해 indicator 위치 설정
            var worldPos = transform.position + indicatorOffsetY * Vector3.up;
            indicator.SetPos(worldPos);
            
            // 현재 활성화된 Indicator 캐싱
            _activeIndicator = indicator;
            
            return indicator;
        }

        private void DeactivateIndicator()
        {
            if (_activeIndicator == null) return;
            
            // pool에 Release (추후 수정)
            Destroy(_activeIndicator.gameObject);
            
            // 캐싱해둔 Indicator 비우기
            _activeIndicator = null;
        }
        
        [Rpc(SendTo.Everyone)]
        private void StartWorkRpc()
        {
            var indicator = _activeIndicator ?? ActivateIndicator();
            _sharedProgress.OnValueChanged = indicator.UpdateProgress;
        }
        
        [Rpc(SendTo.Everyone)]
        private void CancelWorkRpc()
        {
            DeactivateIndicator();
            _sharedProgress.OnValueChanged = null;
        }

        [Rpc(SendTo.Everyone)]
        private void FinishWorkRpc()
        {
            DeactivateIndicator();
            _sharedProgress.OnValueChanged = null;
        }
    }
}