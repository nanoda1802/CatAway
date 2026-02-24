using System;
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
         // Data
         private readonly int _carryParamHash = Animator.StringToHash("Carry");
         // Broker
         private PlacementBroker _placementBroker;
         private ContactBroker _contactBroker;
         // Status
         private DetectStatus _detectStatus;
         private CarryStatus _carryStatus;
         // Input
         private InputAction _carryAction;
         private InputAction _throwAction;
         // Component
         private Animator _animator;
         private InteractionBehaviour _interactionBehaviour;
         [SF] private Transform throwPoint;
         // Property
         public Carriable CarriedItem => this._carryStatus.CurCarriable;
         
         [Inject]
         private void Construct(
            PlacementBroker placementBroker,
            ContactBroker contactBroker,
            DetectStatus detectStatus,
            CarryStatus carryStatus,
            PlayerInput inputMap,
            InteractionBehaviour interactionBehaviour,
            Animator playerAnimator)
         {
            _placementBroker = placementBroker;
            _contactBroker = contactBroker;
            
            _detectStatus = detectStatus;
            _carryStatus = carryStatus;
      
            _carryAction = inputMap.asset.FindAction("Button0"); // 방법 1
            _throwAction = inputMap.Stage.Button2; // 방법 2
      
            _interactionBehaviour = interactionBehaviour;
            
            _animator = playerAnimator;
         }

         #region NGO 관련 메서드
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
            _carryStatus.CurCarriable?.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            if (!IsLocalPlayer) return;
      
            _interactionBehaviour.CancelRpc();
            
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
         
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
=======
         #endregion

         #region Carrier 관련 메서드
         [Rpc(SendTo.Server)]
         private void BehaveOnEmptyHandRpc()
         {
            if (_detectStatus.DetectItem(out var item))
            {
               this.Pick(item);
               return;
            }
            
            if (!_detectStatus.DetectTable(out var table)) return;
            
            AssignToBroker(table);
         }

         [Rpc(SendTo.Server)]
         private void BehaveOnCarryingHandRpc()
         {
            if (!_detectStatus.DetectTable(out var table))
            {
               this.Drop();
               return;
            }

            AssignToBroker(table);
>>>>>>> Stashed changes
         }
      
         [Rpc(SendTo.Server)]
<<<<<<< Updated upstream
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
=======
         private void ThrowRpc()
=======
         #endregion

         #region Carrier 관련 메서드
         [Rpc(SendTo.Server)]
         private void BehaveOnEmptyHandRpc()
         {
            if (_detectStatus.DetectItem(out var item))
            {
               this.Pick(item);
               return;
            }
            
            if (!_detectStatus.DetectTable(out var table)) return;
            
            AssignToBroker(table);
         }

         [Rpc(SendTo.Server)]
         private void BehaveOnCarryingHandRpc()
         {
            if (!_detectStatus.DetectTable(out var table))
            {
               this.Drop();
               return;
            }

            AssignToBroker(table);
         }
         
         [Rpc(SendTo.Server)]
         private void ThrowRpc()
         {
            if (CarriedItem is not Ingredient ingredient) return;
            
            ingredient.Throw(throwPoint.position, throwPoint.rotation,throwPoint.forward).Forget();
            ingredient.Detach();
         }
         
         public void Pick(Carriable item)
         {
            if (item.IsCarrying) item.Detach();
            item.Attach(this);
         }

         private void Drop()
         {
            this.CarriedItem?.Detach();
         }

         private void AssignToBroker(NetworkObject table)
         {
            var result = default(BrokerResult);
            
            if (table.TryGetComponent(out IPlacable placable))
            {
               result = _placementBroker.AcceptCase(this, placable);
               if (result.IsSuccess) return;
            }
            
            if (table.TryGetComponent(out IContactable contactable))
            {
               result = _contactBroker.AcceptCase(this, contactable);
               if (result.IsSuccess) return;
            }
            
            if (result.Reason is not null)
               Debug.LogWarning($"{result.Reason} [Player{this.OwnerClientId} + {table.name}]");
         }
         #endregion

         #region Input 관련 메서드
         private void OnCarryStarted(InputAction.CallbackContext ctx)
>>>>>>> Stashed changes
         {
            if (CarriedItem is not Ingredient ingredient) return;
>>>>>>> Stashed changes
=======
         #endregion

<<<<<<< Updated upstream
         #region Carrier 관련 메서드
         [Rpc(SendTo.Server)]
         private void BehaveOnEmptyHandRpc()
         {
            if (_detectStatus.DetectItem(out var item))
            {
               this.Pick(item);
               return;
            }
            
            if (!_detectStatus.DetectTable(out var table)) return;
            
            AssignToBroker(table);
         }

         [Rpc(SendTo.Server)]
         private void BehaveOnCarryingHandRpc()
         {
            if (!_detectStatus.DetectTable(out var table))
            {
               this.Drop();
               return;
            }

            AssignToBroker(table);
         }
         
         [Rpc(SendTo.Server)]
         private void ThrowRpc()
         {
            if (CarriedItem is not Ingredient ingredient) return;
>>>>>>> Stashed changes
            
            ingredient.Throw(throwPoint.position, throwPoint.rotation,throwPoint.forward).Forget();
            ingredient.Detach();
         }
         
         public void Pick(Carriable item)
         {
            if (item.IsCarrying) item.Detach();
            item.Attach(this);
         }

         private void Drop()
         {
            this.CarriedItem?.Detach();
         }

         private void AssignToBroker(NetworkObject table)
         {
            var result = default(BrokerResult);
            
            if (table.TryGetComponent(out IPlacable placable))
            {
               result = _placementBroker.AcceptCase(this, placable);
               if (result.IsSuccess) return;
            }
            
            if (table.TryGetComponent(out IContactable contactable))
            {
               result = _contactBroker.AcceptCase(this, contactable);
               if (result.IsSuccess) return;
            }
            
            if (result.Reason is not null)
               Debug.LogWarning($"{result.Reason} [Player{this.OwnerClientId} + {table.name}]");
         }
         #endregion

         #region Input 관련 메서드
         private void OnCarryStarted(InputAction.CallbackContext ctx)
         {
            if (!_carryStatus.IsCarryAvailable) return;
<<<<<<< Updated upstream
      
            if (this.HasAttachments) DropRpc();
            else PickRpc();
=======

            if (CarriedItem == null) BehaveOnEmptyHandRpc();
            else BehaveOnCarryingHandRpc();
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
            if (CarriedItem == null) BehaveOnEmptyHandRpc();
            else BehaveOnCarryingHandRpc();
>>>>>>> Stashed changes
         }
      
         private void OnThrowStarted(InputAction.CallbackContext ctx)
         {
            if (CarriedItem == null) return;
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
         #endregion
    }
}