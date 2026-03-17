using _Scripts.Stage.Player.Status;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Stage.Player.Behaviour
{
    public class MovementBehaviour : NetworkBehaviour, INetworkUpdateSystem
    {
        private MoveStatus _moveStatus;
        private InteractStatus _interactStatus;
    
        private InputAction _moveAction;
        private InputAction _dashAction;

        private Rigidbody _playerRb;
        private Animator _animator;
    
        private readonly int _moveParamHash = Animator.StringToHash("Move");
        private readonly int _moveSpeedParamHash = Animator.StringToHash("MoveSpeed");
        private readonly int _dashParamHash = Animator.StringToHash("Dash");
    
        [Inject]
        private void Construct(
            MoveStatus moveStatus, 
            InteractStatus interactStatus,
            PlayerInput inputMap, // 이거 어디선가 enable disable 해줘야해 leak 경고 뜨던데
            Rigidbody playerRb,
            Animator playerAnimator)
        {
            _moveStatus = moveStatus;
            _interactStatus = interactStatus;
            
            _moveAction = inputMap.asset.FindAction("Movement"); // 방법 1
            _dashAction = inputMap.Stage.Button1; // 방법 2

            _playerRb = playerRb;
            _animator = playerAnimator;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsLocalPlayer) return;
            
            this.RegisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
            SubscribeInputEvents();
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsLocalPlayer) return;
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
            UnsubscribeInputEvents();
        }
        
        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!_moveStatus.IsMovable) return;
            
            Move();
            Rotate();
        }
        
        private void Move()
        {
            _playerRb.MovePosition(_playerRb.position + _moveStatus.MoveOffset);
        }
    
        private void Rotate()
        {
            Quaternion smoothRot = Quaternion.Slerp(_playerRb.rotation, _moveStatus.LookRot, _moveStatus.RotRatio);
            _playerRb.MoveRotation(smoothRot);
        }
    
        private async UniTaskVoid Dash()
        {
            SetDashState(true);
            
            _playerRb.MoveRotation(_moveStatus.LookRot);
            _playerRb.AddForce(_moveStatus.DashForce, ForceMode.VelocityChange);

            await _moveStatus.WaitForDash; // UniTask.WaitForSeconds();
            SetDashState(false);
        }
    
        private void SetDashState(bool isDashBegin)
        {
            _animator.SetBool(_dashParamHash, isDashBegin);
            _moveStatus.IsDashing = isDashBegin;
            _playerRb.linearVelocity = _playerRb.angularVelocity = Vector3.zero;
            if (!isDashBegin) _moveStatus.UpdateLastDashTime();
        }
    
        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            if (_interactStatus.IsInteracting) return;
            
            _moveStatus.SetMoveDirection(ctx.ReadValue<Vector2>());
            _animator.SetBool(_moveParamHash, true);
        }
    
        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            _moveStatus.SetMoveDirection(Vector2.zero);
            _animator.SetBool(_moveParamHash, false);
        }
    
        private void OnDashStarted(InputAction.CallbackContext ctx)
        {
            if (_interactStatus.IsInteracting) return;
            if (!_moveStatus.IsMovable || !_moveStatus.IsDashAvailable) return;
            
            Dash().Forget();
            
            _moveStatus.UpdateSpeedMultiplier(true);
            
            // float speed = _moveStatus.UpdateSpeedMultiplier(true);
            // _animator.SetFloat(_moveSpeedParamHash, speed);
        }
        
        private void OnDashCanceled(InputAction.CallbackContext ctx)
        {
            _moveStatus.UpdateSpeedMultiplier(false);
            
            // float speed = _moveStatus.UpdateSpeedMultiplier(false);
            // _animator.SetFloat(_moveSpeedParamHash, speed);
        }
        
        private void SubscribeInputEvents()
        {
            _moveAction.Enable();
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMoveCanceled;
            
            _dashAction.Enable();
            _dashAction.started += OnDashStarted;
            _dashAction.canceled += OnDashCanceled;
        }
    
        private void UnsubscribeInputEvents()
        {
            _moveAction.performed -= OnMovePerformed;
            _moveAction.canceled -= OnMoveCanceled;
            _moveAction.Disable();
    
            _dashAction.started -= OnDashStarted;
            _dashAction.canceled -= OnDashCanceled;
            _dashAction.Disable();
        }
    }
}
