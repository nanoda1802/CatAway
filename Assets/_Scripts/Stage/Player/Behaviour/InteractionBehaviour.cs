using _Scripts.Stage.Player.Status;
using _Scripts.Stage.Table;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using VContainer;

namespace _Scripts.Stage.Player.Behaviour
{
    public class InteractionBehaviour : NetworkBehaviour
    {
        // Status
        private DetectStatus _detectStatus;
        private InteractStatus _interactStatus;
        private CarryStatus _carryStatus;
        private MoveStatus _moveStatus;
        // Input
        private InputAction _interactAction;
        // Component
        private Rigidbody _playerRb;
        private Animator _animator;
        
        [Inject]
        private void Construct(
            DetectStatus detectStatus, 
            InteractStatus interactStatus,
            MoveStatus moveStatus,
            CarryStatus carrierStatus, 
            PlayerInput inputMap, 
            Rigidbody playerRb,
            Animator playerAnimator)
        {
            _detectStatus = detectStatus;
            _interactStatus = interactStatus;
            _carryStatus = carrierStatus;
            _moveStatus = moveStatus;
            
            _playerRb = playerRb;
            _animator = playerAnimator;
            
            _interactAction = inputMap.FindAction("Button2");
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
        #endregion
        
    
        [Rpc(SendTo.Server)]
        private void TryInteractRpc()
        {
            if (_carryStatus.HasCarriable) return;
            if (!_detectStatus.DetectTable(out var table)) return;
            if (!table.TryGetComponent(out IInteractable interactable)) return;
            
            if (!interactable.TryInteraction(this, out int animParamHash)) return;
            
            _interactStatus.CurInteractable = interactable;
            StartInteractionRpc(animParamHash, RpcTarget.Single(this.OwnerClientId,RpcTargetUse.Temp));
        }
    
        [Rpc(SendTo.Server)]
        public void CancelRpc()
        {
            if (!_interactStatus.IsInteracting) return;
            
            _interactStatus.CurInteractable.CancelInteraction(this); 
            
            _interactStatus.CurInteractable = null;
            StopInteractionRpc(RpcTarget.Single(this.OwnerClientId,RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server)]
        public void FinishRpc()
        {
            if (!_interactStatus.IsInteracting) return;
            
            _interactStatus.CurInteractable = null;
            StopInteractionRpc(RpcTarget.Single(this.OwnerClientId,RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void StartInteractionRpc(int animParamHash, RpcParams rpcParams = default)
        {
            _moveStatus.MoveConstraint = true;
            _moveStatus.SetMoveDirection(Vector2.zero);
            
            _playerRb.linearVelocity = _playerRb.angularVelocity = Vector3.zero;
            _playerRb.constraints = RigidbodyConstraints.FreezeAll;
            
            _interactStatus.StartInteractionAnim(_animator, animParamHash);
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void StopInteractionRpc(RpcParams rpcParams = default)
        {
            _moveStatus.MoveConstraint = false;
            
            _playerRb.constraints = RigidbodyConstraints.FreezeRotation;
            
            _interactStatus.StopInteractionAnim(_animator);
        }

        #region Input 관련 메서드
        private void OnInteractStarted(InputAction.CallbackContext ctx)
        {
            if (!_interactStatus.IsInteractAvailable) return;
            if (ctx.interaction is not HoldInteraction) return;
            
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            TryInteractionRpc();
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            TryInteractRpc();
            _interactStatus.UpdateLastInteractTime();
>>>>>>> Stashed changes
        }
    
        private void OnInteractCanceled(InputAction.CallbackContext ctx)
        {
            if (ctx.interaction is not HoldInteraction) return;
            
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            CancelInteractionRpc();
            _interactStatus.UpdateLastInteractTime();
=======
            CancelRpc();
>>>>>>> Stashed changes
=======
            CancelRpc();
>>>>>>> Stashed changes
=======
            CancelRpc();
>>>>>>> Stashed changes
=======
            CancelRpc();
>>>>>>> Stashed changes
        }
    
        private void SubscribeInputEvents()
        {
            _interactAction.Enable();
            _interactAction.started += OnInteractStarted;
            _interactAction.canceled += OnInteractCanceled;
        }
    
        private void UnsubscribeInputEvents()
        {
            _interactAction.started -= OnInteractStarted;
            _interactAction.canceled -= OnInteractCanceled;
            _interactAction.Disable();
        }
        #endregion
    }
}