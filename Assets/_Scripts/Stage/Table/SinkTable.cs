using System;
using System.Collections.Generic;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Plate;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.UI.Movable;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Table
{
    public class SinkTable : NetworkBehaviour, IPlacable, IInteractable, INetworkUpdateSystem
    {
        [SF] private ProgressIndicator indicatorPrefab; // [임시]
        [SF] private Canvas movableCanvas; // [임시]
        [SF] private float indicatorOffsetY = 1.2f;
        private ProgressIndicator _activeIndicator;
        
        [SF] private PlateProvider _plateProvider;

        private IPlacable _plateRackTable;
        
        private readonly List<ulong> _interactorList = new();
        
        private Plate _washTarget;
        private readonly NetworkVariable<float> _sharedProgress = new();
        private event Action OnFinished;
        
        private readonly int _washAnimParamHash = Animator.StringToHash("WashDish");
        
        public Carriable PlacedItem => null;
        public bool IsInteracting => OnFinished != null;

        private void Awake() // [임시]
        {
            _plateRackTable = FindFirstObjectByType<PlateRackTable>();
        }
        
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

            if (progress >= 0.99f)
            {
                FinishInteraction();
            }
        }
        
        public bool TryPlace(Carriable carriable)
        {
            if (carriable == null || !carriable.IsSpawned) return false;
            if (carriable.Type != CarriableType.Plate) return false;
            if (!carriable.NetworkObject.TryGetComponent(out Plate plate) || plate.IsReady) return false;
            
            _plateProvider.ReleasePlate(plate);
            plate.NetworkObject.Despawn(false);
            
            // provider.HasInactivePlate가 true 면 싱크대 에 접시 오브젝트 보여주기
            if (_plateProvider.HasInactivePlate) PlaceRpc();
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier, out Carriable carriable)
        {
            carriable = null;
            return false;
        }

        public bool TryInteraction(InteractionBehaviour interactor, out int animParamHash)
        {
            animParamHash = -1;
            
            if (_washTarget is null)
            {
                if (!_plateProvider.HasInactivePlate) return false;
                _washTarget = _plateProvider.GetPlate(transform.position + 100 * Vector3.down);
                _washTarget.NetworkObject.Spawn();
                // 그리고 얘를 끄던가 어디 숨겨놓던가 해야하는데 아무튼...
                // 일단 엄청 밑에 넣어두자
                // 계속 떨어지고 있겠네...
            }
            
            animParamHash = _washAnimParamHash;
            
            OnFinished += interactor.FinishInteractionRpc;

            // 상호작용은 여러 명이 해도, Plate는 하나라서 방어용
            OnFinished -= _washTarget.OnPrepFinished;
            OnFinished += _washTarget.OnPrepFinished;
            
            StartInteractionRpc();
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            
            _interactorList.Add(interactor.OwnerClientId);
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
            _plateRackTable.TryPlace(carriable);
            
            _washTarget = null;
            OnFinished = null;
            _sharedProgress.Value = 0;
            _interactorList.Clear();
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
            Debug.Log("There are Plates to Wash!!");
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