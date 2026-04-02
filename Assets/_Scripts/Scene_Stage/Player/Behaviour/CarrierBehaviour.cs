using _Scripts.Messages.Stage;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Item;
using _Scripts.Scene_Stage.Item.Ingredient;
using _Scripts.Scene_Stage.Player.Status;
using _Scripts.Scene_Stage.Table;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Player.Behaviour
{
    public class CarrierBehaviour : AttachableNode, IBehaviourWithInput
    {
         // Data
         private readonly int _carryParamHash = Animator.StringToHash("Carry");
         private StageSfxListData _sfxList;
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
            StageSfxListData sfxList,
            PlacementBroker placementBroker,
            ContactBroker contactBroker,
            DetectStatus detectStatus,
            CarryStatus carryStatus,
            PlayerInput inputMap,
            InteractionBehaviour interactionBehaviour,
            Animator playerAnimator,
            ISubscriber<StartStageMessage> startSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
         {
            Debug.Log("<color=red>혹시 설마 주입 두 번 되나요?</color>");
            _sfxList = sfxList;
            
            _placementBroker = placementBroker;
            _contactBroker = contactBroker;
            
            _detectStatus = detectStatus;
            _carryStatus = carryStatus;
      
            _carryAction = inputMap.asset.FindAction("Button0"); // 방법 1
            _throwAction = inputMap.Stage.Button2; // 방법 2
      
            _interactionBehaviour = interactionBehaviour;
            
            _animator = playerAnimator;
            
            startSub
               .Subscribe(SubscribeInputEvents)
               .AddTo(disposableBagBuilder);
            
            endSub
               .Subscribe(UnsubscribeInputEvents)
               .AddTo(disposableBagBuilder);
         }

         #region NGO 관련 메서드
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

            if (!_detectStatus.DetectTable(out var table))
            {
               return;
            }
            
            AssignToBroker(table);
         }

         [Rpc(SendTo.Server)]
         private void BehaveOnCarryingHandRpc()
         {
            if (CarriedItem is IIngredientHolder holder 
                && _detectStatus.DetectItem(out var item)
                && item is Ingredient ingredient)
            {
               if (holder.CanHold(ingredient, out string rejectMsg))
               {
                  holder.Hold(ingredient);
                  PlaySfxRpc(StageSfxType.ActionAllowed);
                  return;
               }
               
               if (!string.IsNullOrEmpty(rejectMsg))
               {
                  Debug.LogWarning($"{rejectMsg} [Player{this.OwnerClientId}]");
               }
            }

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
            
            PlaySfxRpc(StageSfxType.Throw);
         }
         
         public void Pick(Carriable item)
         {
            if (item.IsCarrying) item.Detach();
            item.Attach(this);
         }

         public void Drop()
         {
            this.CarriedItem?.Detach();
         }

         private void AssignToBroker(NetworkObject table)
         {
            var result = default(BrokerResult);
            
            if (table.TryGetComponent(out IPlacable placable))
            {
               result = _placementBroker.AcceptCase(this, placable);
               if (result.IsSuccess)
               {
                  PlaySfxRpc(StageSfxType.ActionAllowed);
                  return;
               }
            }
            
            if (table.TryGetComponent(out IContactable contactable))
            {
               result = _contactBroker.AcceptCase(this, contactable);
               if (result.IsSuccess)
               {
                  PlaySfxRpc(StageSfxType.ActionAllowed);
                  return;
               }
            }

            if (!string.IsNullOrEmpty(result.Reason))
            {
               Debug.LogWarning($"{result.Reason} [Player{this.OwnerClientId} + {table.name}]");
               PlaySfxRpc(StageSfxType.ActionBlocked);
            }
         }
         #endregion
         
         #region Input 관련 메서드
         private void OnCarryStarted(InputAction.CallbackContext ctx)
         {
            if (!_carryStatus.IsCarryAvailable) return;

            if (CarriedItem == null) BehaveOnEmptyHandRpc();
            else BehaveOnCarryingHandRpc();
         }
      
         private void OnThrowStarted(InputAction.CallbackContext ctx)
         {
            if (CarriedItem == null) return;
            if (ctx.interaction is not PressInteraction) return;
            
            ThrowRpc();
         }
         
         public void SubscribeInputEvents(StartStageMessage msg)
         {
            if (!IsLocalPlayer) return;
            
            _carryAction.Enable();
            _carryAction.started += OnCarryStarted;
            
            _throwAction.Enable();
            _throwAction.started += OnThrowStarted;
         }
      
         public void UnsubscribeInputEvents(EndStageMessage msg)
         {
            if (!IsLocalPlayer) return;
            
            _carryAction.started -= OnCarryStarted;
            _carryAction.Disable();
            
            _throwAction.started -= OnThrowStarted;
            _throwAction.Disable();
         }
         #endregion

         [Rpc(SendTo.Owner)]
         private void PlaySfxRpc(StageSfxType sfxType)
         {
            _sfxList.Play(sfxType);
         }
    }
}