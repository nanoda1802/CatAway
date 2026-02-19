using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.UI.Movable;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class ChoppingTable : NetworkBehaviour, IPlacable, IInteractable, INetworkUpdateSystem
    {
        [SF] private AttachableNode pivot;
        [SF] private GameObject knifeModel;
        [SF] private IngredientType availableType = IngredientType.Lettuce | IngredientType.Cheese | IngredientType.Tomato;

        [SF] private ProgressIndicator indicatorPrefab; // [임시]
        [SF] private Canvas movableCanvas; // [임시]
        [SF] private float indicatorOffsetY = 1.2f;
        private ProgressIndicator _activeIndicator;
        
        private Carriable _placedItem;
        private IPrepable _chopTarget;
        private readonly NetworkVariable<float> _sharedProgress = new();

        private event Action OnFinished;
        private TagHandle _itemTag;
        
        private readonly int _chopAnimParamHash = Animator.StringToHash("Chop");
        
        public Carriable PlacedItem => _placedItem;
        public bool IsInteracting => _chopTarget != null;

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
            if (!IsInteracting) return;
            
            float progress = _chopTarget.Prepare(1);
            _sharedProgress.Value = progress;

            if (progress >= 0.99f)
            {
                FinishInteraction();
            }
        }
        
        public bool TryPlace(Carriable carriable)
        {
            if (carriable == null || !carriable.IsSpawned) return false;
            if (pivot.HasAttachments || _placedItem is not null) return false;
            if (!carriable.NetworkObject.TryGetComponent(out Ingredient ingredient)) return false;
            if (!availableType.HasFlag(ingredient.Type)) return false;
            
            if (carriable.IsAttach) carriable.Detach();
            carriable.Attach(pivot);
            _placedItem = carriable;
            
            PlaceRpc();
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            carriable = null;
            if (carrier == null) return false;
            if (!pivot.HasAttachments || _placedItem is null) return false;
            
            carriable = _placedItem;
            
            _placedItem.Detach();
            _placedItem = null;
            
            DisplaceRpc();
            
            return true;
        }

        public bool TryInteraction(InteractionBehaviour interactor, out int animParamHash)
        {
            animParamHash = -1;
            
            if (IsInteracting) return false;
            if (!pivot.HasAttachments || _placedItem is null) return false;
            if (!_placedItem.NetworkObject.TryGetComponent(out _chopTarget)) return false;
            if (_chopTarget.IsReady) return false;
            
            animParamHash = _chopAnimParamHash;
            
            OnFinished += interactor.FinishInteractionRpc;
            OnFinished += _chopTarget.OnPrepFinished;
            
            StartInteractionRpc();
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            
            return true;
        }

        public void CancelInteraction(ulong clientId)
        {
            CancelInteractionRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _chopTarget = null;
            OnFinished = null;
        }

        public void FinishInteraction()
        {
            FinishInteractionRpc();
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
            
            OnFinished?.Invoke();
            
            _chopTarget = null;
            OnFinished = null;
            _sharedProgress.Value = 0;
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
        private void PlaceRpc()
        {
            knifeModel.SetActive(false);
        }
        
        [Rpc(SendTo.Everyone)]
        private void DisplaceRpc()
        {
            knifeModel.SetActive(true);
            
            DeactivateIndicator();
            _sharedProgress.OnValueChanged = null;
        }

        [Rpc(SendTo.Everyone)]
        private void StartInteractionRpc()
        {
            var indicator = _activeIndicator ?? ActivateIndicator();
            _sharedProgress.OnValueChanged = indicator.UpdateProgress;
        }
        
        [Rpc(SendTo.Everyone)]
        private void CancelInteractionRpc()
        {
            _sharedProgress.OnValueChanged = null;
        }

        [Rpc(SendTo.Everyone)]
        private void FinishInteractionRpc()
        {
            DeactivateIndicator();
            _sharedProgress.OnValueChanged = null;
        }
    }
}