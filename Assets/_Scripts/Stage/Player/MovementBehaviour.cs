using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace _Scripts.Stage.Player
{
    public class MovementBehaviour : NetworkBehaviour, INetworkUpdateSystem
    {
        [SerializeField] PlayerData playerData; // [임시]
        private PlayerInput _inputMap;  // [임시]
        
        private MoveStatus _moveStatus;
        private InteractStatus _interactStatus;
    
        private InputAction _moveAction;
        private InputAction _dashAction;
    
        private Transform _playerTr;
        private CharacterController _charCtrl;
        private Animator _animator;
    
        private readonly int _moveParamHash = Animator.StringToHash("Move");
        private readonly int _moveSpeedParamHash = Animator.StringToHash("MoveSpeed");
        private readonly int _dashParamHash = Animator.StringToHash("Dash");
    
        [Inject]
        private void Construct(
            MoveStatus moveStatus, 
            InteractStatus interactStatus,
            PlayerInput inputMap, // 이거 어디선가 enable disable 해줘야해 leak 경고 뜨던데
            CharacterController  characterController,
            Animator playerAnimator)
        {
            _inputMap =  inputMap; // [임시]
            
            _moveStatus = moveStatus;
            _interactStatus = interactStatus;
            
            _moveAction = inputMap.asset.FindAction("Movement"); // 방법 1
            _dashAction = inputMap.Stage.Button1; // 방법 2

            _playerTr = this.transform;
            _charCtrl = characterController;
            _animator = playerAnimator;
        }

        private void Awake() // [임시]
        {
            Construct(
                new MoveStatus(playerData),
                new InteractStatus(playerData),
                new PlayerInput(),
                GetComponent<CharacterController>(),
                GetComponentInChildren<Animator>());
        }

        public override void OnNetworkSpawn()
        {
            if (!IsLocalPlayer) return;
            
            _inputMap.Enable(); // [임시]
            this.RegisterNetworkUpdate(NetworkUpdateStage.Update);
            SubscribeInputEvents();
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsLocalPlayer) return;
            
            _inputMap.Disable(); // [임시]
            this.UnregisterNetworkUpdate(NetworkUpdateStage.Update);
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
            _charCtrl.Move(_moveStatus.MoveOffset);
        }
    
        private void Rotate()
        {
            Quaternion smoothRot = Quaternion.Slerp(_playerTr.rotation, _moveStatus.LookRot, _moveStatus.RotRatio);
            transform.rotation = smoothRot;
        }
    
        private async UniTaskVoid Dash()
        {
            SetDashState(true);
            
            // 어떻게 해야하지?
            Debug.Log("Dashing");

            await _moveStatus.WaitForDash; // UniTask.WaitForSeconds();
            SetDashState(false);
        }
    
        private void SetDashState(bool isDashBegin)
        {
            _animator.SetBool(_dashParamHash, isDashBegin);
            _moveStatus.IsDashing = isDashBegin;
            if (!isDashBegin) _moveStatus.UpdateLastDashTime();
        }
    
        private void OnMoveStarted(InputAction.CallbackContext ctx)
        {
            if (_interactStatus.IsInteracting) return;
            _animator.SetBool(_moveParamHash, true);
        }
    
        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            if (_interactStatus.IsInteracting) return;
            _moveStatus.SetMoveDirection(ctx.ReadValue<Vector2>());
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
            
            float speed = _moveStatus.UpdateSpeedMultiplier(true);
            // _animator.SetFloat(_moveSpeedParamHash, speed);
        }
        
        private void OnDashCanceled(InputAction.CallbackContext ctx)
        {
            float speed = _moveStatus.UpdateSpeedMultiplier(false);
            // _animator.SetFloat(_moveSpeedParamHash, speed);
        }
        
        private void SubscribeInputEvents()
        {
            _moveAction.Enable();
            _moveAction.started += OnMoveStarted;
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMoveCanceled;
            
            _dashAction.Enable();
            _dashAction.started += OnDashStarted;
            _dashAction.canceled += OnDashCanceled;
        }
    
        private void UnsubscribeInputEvents()
        {
            _moveAction.started -= OnMoveStarted;
            _moveAction.performed -= OnMovePerformed;
            _moveAction.canceled -= OnMoveCanceled;
            _moveAction.Disable();
    
            _dashAction.started -= OnDashStarted;
            _dashAction.canceled -= OnDashCanceled;
            _dashAction.Disable();
        }
    }
}
