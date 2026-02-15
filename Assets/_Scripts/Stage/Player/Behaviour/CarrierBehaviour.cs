using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Status;
using _Scripts.Stage.Table;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Player.Behaviour
{
    public class CarrierBehaviour : AttachableNode
    {
         private DetectStatus _detectStatus;
         private CarryStatus _carryStatus;
      
         private InputAction _carryAction;
         private InputAction _throwAction;
      
         private Animator _animator;
      
         private readonly int _carryParamHash = Animator.StringToHash("Carry");
      
         [SF] private Transform throwPoint;
         
         [Inject]
         private void Construct(
            DetectStatus detectStatus,
            CarryStatus carryStatus,
            PlayerInput inputMap,
            Animator playerAnimator)
         {
            _detectStatus = detectStatus;
            _carryStatus = carryStatus;
      
            _carryAction = inputMap.asset.FindAction("Button0"); // 방법 1
            _throwAction = inputMap.Stage.Button2; // 방법 2
      
            _animator = playerAnimator;
         }
      
         public override void OnNetworkSpawn()
         {
            if (!IsLocalPlayer) return;
            SubscribeInputEvents();
         }
      
         public override void OnNetworkDespawn()
         {
            if (!IsLocalPlayer) return;
            UnsubscribeInputEvents();
         }
      
         protected override void OnAttached(AttachableBehaviour attachableBehaviour)
         {
            _carryStatus.CurCarriable = attachableBehaviour as Carriable;
      
            if (!IsLocalPlayer) return;
      
            _animator.SetBool(_carryParamHash, this.HasAttachments);
            _carryStatus.UpdateLastCarryTime();
         }
      
         protected override void OnDetached(AttachableBehaviour attachableBehaviour)
         {
            _carryStatus.CurCarriable = null;
      
            if (!IsLocalPlayer) return;
      
            _animator.SetBool(_carryParamHash, this.HasAttachments);
            _carryStatus.UpdateLastCarryTime();
         }
         
         [ServerRpc]
         private void PickServerRpc()
         {
            if (_detectStatus.DetectItem(out var item))
            {
               item?.Attach(this);
               return;
            }
            
            if (!_detectStatus.DetectTable(out var table)) return;
            if (!table.TryGetComponent(out IPlacable placable)) return;
            
            if (placable.TryDisplace(this))
            {
               Debug.Log("Displace Success");
            }
            else
            {
               Debug.Log("Displace Failure");
            }
         }
      
         [ServerRpc]
         private void DropServerRpc()
         {
            var item = _carryStatus.CurCarriable;
            item?.Detach();
            
            if (!_detectStatus.DetectTable(out var table)) return;
            if (!table.TryGetComponent(out IPlacable placable)) return;
            
            if (placable.TryPlace(item))
            {
               Debug.Log("Place Success");
            }
            else
            {
               Debug.Log("Place Failure");
            }
         }
      
         [ServerRpc]
         private void ThrowServerRpc()
         {
            var item = _carryStatus.CurCarriable;
            var throwable = item?.NetworkObject.GetComponentInChildren<Throwable>();
            
            if (throwable == null) return;
            
            throwable.Throw(throwPoint.position, throwPoint.rotation,throwPoint.forward).Forget();
            item.Detach();
         }
         
         private void OnCarryStarted(InputAction.CallbackContext ctx)
         {
            // if (_interactStatus.IsInteracting) return;
            if (!_carryStatus.IsCarryAvailable) return;
      
            if (this.HasAttachments) DropServerRpc();
            else PickServerRpc();
         }
      
         private void OnThrowStarted(InputAction.CallbackContext ctx)
         {
            if (!this.HasAttachments) return;
            // if (_interactStatus.IsInteracting || _moveStatus.IsDashing) return;
            if (!_carryStatus.IsCarryAvailable || !_carryStatus.HasCarriable) return;
            if (ctx.interaction is not PressInteraction) return;
            
            ThrowServerRpc();
         }
         
         private void SubscribeInputEvents()
         {
            _carryAction.Enable();
            _carryAction.started += OnCarryStarted;
            
            _throwAction.Enable();
            _throwAction.started += OnThrowStarted;
         }
      
         private void UnsubscribeInputEvents()
         {
            _carryAction.started -= OnCarryStarted;
            _carryAction.Disable();
            
            _throwAction.started -= OnThrowStarted;
            _throwAction.Disable();
         }
    }
}