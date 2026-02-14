using _Scripts.Stage.Table;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using VContainer;

namespace _Scripts.Stage.Player
{
    public class InteractionBehaviour : NetworkBehaviour
    {
        private DetectStatus _detectStatus;
        private InteractStatus _interactStatus;
        private CarryStatus _carryStatus;
        private MoveStatus _moveStatus;
        
        private InputAction _interactAction;
    
        private Animator _animator;
    
        private ClientRpcParams _clientRpcParams;
        private readonly ulong[] _clientId = new ulong[1];
        
        [Inject]
        private void Construct(
            DetectStatus detectStatus, 
            InteractStatus interactStatus,
            MoveStatus moveStatus,
            CarryStatus carrierStatus, 
            PlayerInput inputMap, 
            Animator playerAnimator)
        {
            _detectStatus = detectStatus;
            _interactStatus = interactStatus;
            _carryStatus = carrierStatus;
            _moveStatus = moveStatus;
            
            _animator = playerAnimator;
            
            _interactAction = inputMap.FindAction("Button2");
            
            _clientRpcParams = new ClientRpcParams();
            _clientRpcParams.Send.TargetClientIds = _clientId;
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
    
        [ServerRpc]
        private void StartInteractionServerRpc(ServerRpcParams rpcParams = default)
        {
            if (_carryStatus.HasCarriable) return;
            if (!_detectStatus.DetectTable(out var table)) return;
            if (!table.TryGetComponent<IInteractable>(out var interactable)) return;
            
            bool hasStated = interactable.TryInteractStart(this);
            
            _clientId[0] = rpcParams.Receive.SenderClientId;
            ConfirmStartClientRpc(hasStated,interactable.AnimParamHash,_clientRpcParams);
            
            if (hasStated) _interactStatus.CurInteractable = interactable;
        }
    
        [ServerRpc]
        private void StopInteractionServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!_interactStatus.IsInteracting) return;
            bool hasStopped = _interactStatus.CurInteractable.TryInteractStop(this); 
            
            _clientId[0] = rpcParams.Receive.SenderClientId;
            ConfirmStopClientRpc(hasStopped,_interactStatus.CurInteractable.AnimParamHash,_clientRpcParams);
            
            if (hasStopped) _interactStatus.CurInteractable = null;
        }
        
        public void FinishInteraction(ulong targetClientId)
        {
            if (!_interactStatus.IsInteracting) return;
            
            _clientId[0] = targetClientId;
            ConfirmStopClientRpc(true, _interactStatus.CurInteractable.AnimParamHash, _clientRpcParams);
            _interactStatus.CurInteractable = null;
        }
    
        [ClientRpc]
        private void ConfirmStartClientRpc(bool isSuccess, int animParamHash, ClientRpcParams rpcParams = default)
        {
            if (isSuccess)
            {
                Debug.Log("Interact Start Success");
                _animator.SetBool(animParamHash, true);
                // _moveStatus.MoveConstraint = true;
            }
            else
            {
                Debug.Log("Interact Start Failed");
                // 실패했을 때 처리
            }
        }
        
        [ClientRpc]
        private void ConfirmStopClientRpc(bool isSuccess, int animParamHash, ClientRpcParams rpcParams = default)
        {
            if (isSuccess)
            {
                Debug.Log("Interact Stop Success");
                _animator.SetBool(animParamHash, false);
                // _moveStatus.MoveConstraint = false;
            }
            else
            {
                Debug.Log("Interact Stop Failed");
                // 실패했을 때 처리
            }
        }
    
        private void OnInteractStarted(InputAction.CallbackContext ctx)
        {
            if (!_interactStatus.IsInteractAvailable) return;
            if (ctx.interaction is not HoldInteraction) return;
            
            StartInteractionServerRpc();
        }
    
        private void OnInteractCanceled(InputAction.CallbackContext ctx)
        {
            if (ctx.interaction is not HoldInteraction) return;
            
            StopInteractionServerRpc();
            _interactStatus.UpdateLastInteractTime();
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
    }
}