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
      
         private InteractionBehaviour _interactionBehaviour;
         
         private Animator _animator;
      
         private readonly int _carryParamHash = Animator.StringToHash("Carry");
      
         [SF] private Transform throwPoint;
         
         [Inject]
         private void Construct(
            DetectStatus detectStatus,
            CarryStatus carryStatus,
            PlayerInput inputMap,
            InteractionBehaviour interactionBehaviour,
            Animator playerAnimator)
         {
            _detectStatus = detectStatus;
            _carryStatus = carryStatus;
      
            _carryAction = inputMap.asset.FindAction("Button0"); // 방법 1
            _throwAction = inputMap.Stage.Button2; // 방법 2
      
            _interactionBehaviour = interactionBehaviour;
            
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
      
            _interactionBehaviour.CancelInteractionRpc();
            
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
         
         [Rpc(SendTo.Server)]
         private void PickRpc()
         {
            if (_detectStatus.DetectItem(out var item))
            {
               item?.Attach(this);
               return;
            }
            
            if (!_detectStatus.DetectTable(out var tableObj)) return;
            if (!tableObj.TryGetComponent(out IPlacable table)) return;
            
            if (table.TryDisplace(this, out Carriable placedItem))
            {
               Debug.Log("Displace Success");
               placedItem.Attach(this);
            }
            else
            {
               Debug.Log("Displace Failure");
            }
         }
      
         [Rpc(SendTo.Server)]
         private void DropRpc()
         {
            var item = _carryStatus.CurCarriable;
            if (item == null) return;

            if (!_detectStatus.DetectTable(out var tableObj) || !tableObj.TryGetComponent(out IPlacable table))
            {
               item.Detach();
               return;
            }
            
            // [수정 예정] CurCarriable이 cookware고, 그 cookware가 hasIngredient면...
            // cookware에서 TakeOutCarriable해서 꺼낸 내용물을 TryPlace...
            
            if (table.TryPlace(item)) return;
            
            if (!item.NetworkObject.TryGetComponent(out IIngredientHolder holder)) return;
            if (!table.TryDisplace(this,out var placedItem)) return;
            if (holder.TryAdd(placedItem)) return;
            
            table.TryPlace(placedItem);
         }

         [Rpc(SendTo.Server)]
         private void ThrowRpc()
         {
            var carriable = _carryStatus.CurCarriable;
            
            if (carriable is null) return;
            if (!carriable.TryGetComponent(out Throwable throwable)) return;
            
            throwable.Throw(throwPoint.position, throwPoint.rotation,throwPoint.forward).Forget();
            carriable.Detach();
         }

         private void OnCarryStarted(InputAction.CallbackContext ctx)
         {
            if (!_carryStatus.IsCarryAvailable) return;
      
            if (this.HasAttachments) DropRpc();
            else PickRpc();
         }
      
         private void OnThrowStarted(InputAction.CallbackContext ctx)
         {
            if (!this.HasAttachments || !_carryStatus.HasCarriable) return;
            if (ctx.interaction is not PressInteraction) return;
            
            ThrowRpc();
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