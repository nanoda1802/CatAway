using _Scripts._Helper;
using _Scripts.Messages.Stage;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Player.Status;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Player.Behaviour
{
    public class MovementBehaviour : NetworkBehaviour, INetworkUpdateSystem, IBehaviourWithInput
    {
        [SF] private ParticleSystem dashVfx;
        
        private MoveStatus _moveStatus;
        private InteractStatus _interactStatus;
    
        private VfxHandler _vfxHandler;
        
        private InputAction _moveAction;
        private InputAction _dashAction;

        private Rigidbody _playerRb;
        private Animator _animator;

        private StageSfxListData _sfxList;
    
        private readonly int _moveParamHash = Animator.StringToHash("Move");
        private readonly int _moveSpeedParamHash = Animator.StringToHash("MoveSpeed");
        private readonly int _dashParamHash = Animator.StringToHash("Dash");
    
        [Inject]
        private void Construct(
            MoveStatus moveStatus, 
            InteractStatus interactStatus,
            VfxHandler vfxHandler,
            PlayerInput inputMap,
            Rigidbody playerRb,
            Animator playerAnimator,
            StageSfxListData sfxList,
            ISubscriber<StartStageMessage> startSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _moveStatus = moveStatus;
            _interactStatus = interactStatus;
            
            _vfxHandler = vfxHandler;
            
            _moveAction = inputMap.asset.FindAction("Movement"); // 방법 1
            _dashAction = inputMap.Stage.Button1; // 방법 2

            _playerRb = playerRb;
            _animator = playerAnimator;
            
            _sfxList = sfxList;
            
            startSub
                .Subscribe(SubscribeInputEvents)
                .AddTo(disposableBagBuilder);
            
            endSub
                .Subscribe(UnsubscribeInputEvents)
                .AddTo(disposableBagBuilder);
        }
        
        public override void OnNetworkDespawn()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
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

            _sfxList.Play(StageSfxType.Dash, true, this.transform);
            
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
            if (IsSpawned) ActivateVfxRpc();
            
            // float speed = _moveStatus.UpdateSpeedMultiplier(true);
            // _animator.SetFloat(_moveSpeedParamHash, speed);
        }
        
        private void OnDashCanceled(InputAction.CallbackContext ctx)
        {
            _moveStatus.UpdateSpeedMultiplier(false);
            if (IsSpawned) DeactivateVfxRpc();
            
            // float speed = _moveStatus.UpdateSpeedMultiplier(false);
            // _animator.SetFloat(_moveSpeedParamHash, speed);
        }
        
        public void SubscribeInputEvents(StartStageMessage msg)
        {
            if (!IsLocalPlayer) return;
            
            _moveAction.Enable();
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMoveCanceled;
            
            _dashAction.Enable();
            _dashAction.started += OnDashStarted;
            _dashAction.canceled += OnDashCanceled;
            
            this.RegisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
        }
    
        public void UnsubscribeInputEvents(EndStageMessage msg)
        {
            if (!IsLocalPlayer) return;
            
            _moveAction.performed -= OnMovePerformed;
            _moveAction.canceled -= OnMoveCanceled;
            _moveAction.Disable();
    
            _dashAction.started -= OnDashStarted;
            _dashAction.canceled -= OnDashCanceled;
            _dashAction.Disable();
            
            this.UnregisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
        }

        [Rpc(SendTo.Everyone)]
        private void ActivateVfxRpc()
        {
            _vfxHandler.PlayVfx(dashVfx);
        }
        
        [Rpc(SendTo.Everyone)]
        private void DeactivateVfxRpc()
        {
            _vfxHandler.StopSmoothly(dashVfx);
        }
    }
}
