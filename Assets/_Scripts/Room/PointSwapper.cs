using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using VContainer.Unity;

namespace _Scripts.Room
{
    public class PointSwapper : IInitializable, IDisposable
    {
        private readonly Camera _mainCam;
        private readonly NetworkManager _netManager;
        private readonly PlayerInput _playerInput;
        
        private readonly InputAction _pressAction;
        private readonly InputAction _posAction;

        private readonly int _pointMask;

        private RoomMember _draggingMem;
        
        private float _zDistCamToPlayer;
        private Vector3 _dragOffset;

        private MemberPoint _originPoint;
        
        private bool IsDragging => _draggingMem != null;
        private bool IsBlockedByUi => EventSystem.current.IsPointerOverGameObject();
        private Vector2 ScreenPos => _posAction.ReadValue<Vector2>();
            
        public PointSwapper(
                NetworkManager netManager,
                PlayerInput playerInput)
        {
            _mainCam = Camera.main;
            _pointMask = LayerMask.GetMask("MemberPoint");
            
            _netManager = netManager;
            _playerInput = playerInput;

            _pressAction = playerInput.Room.PointerPress;
            _posAction = playerInput.Room.PointerPosition;
        }

        public void Initialize()
        {
            if (!_netManager.IsHost) return;
            
            SubscribeInputEvents();
            InitCache();
        }

        public void Dispose()
        {
            UnsubscribeInputEvents();
        }

        private bool Detect(Vector2 screenPos, LayerMask targetMask, out RaycastHit hit)
        {
            hit = default;
            if (IsBlockedByUi) return false;
            
            bool detected = Physics.Raycast(
                _mainCam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0)),
                out hit,
                Mathf.Infinity,
                targetMask);
            
            return detected;
        }

        private void InitCache()
        {
            _zDistCamToPlayer = 0;
            _draggingMem = null;
            _dragOffset = Vector3.zero;
            _originPoint = null;
        }

        private void OnPressStarted(InputAction.CallbackContext ctx)
        {
            if (ctx.interaction is not HoldInteraction) return;
            if (IsDragging) return;
            if (!Detect(ScreenPos, _pointMask, out RaycastHit hit)
                || !hit.transform.TryGetComponent(out _originPoint)) return; 
            if (!_originPoint.HasMem) return;

            
            _draggingMem = _originPoint.CurMem.StartDrag();
            _zDistCamToPlayer = Mathf.Abs(_mainCam.transform.position.z - _draggingMem.CurPos.z);

            Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(ScreenPos.x, ScreenPos.y, _zDistCamToPlayer));
            _dragOffset = _originPoint.transform.position - worldPos;
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            if (!IsDragging) return;
            
            if (!Detect(ScreenPos, _pointMask, out RaycastHit hit)
                || !hit.transform.TryGetComponent(out MemberPoint otherPoint))
            {
                _draggingMem.MoveTo(_originPoint.Pos, _originPoint.Rot);
                InitCache();
                return;
            }
            
            _originPoint.SwapMem(otherPoint);
            InitCache();
        }

        private void OnPosPerformed(InputAction.CallbackContext ctx)
        {
            if (!IsDragging) return;
            
            Vector3 inputWorldPos = _mainCam.ScreenToWorldPoint(new Vector3(ScreenPos.x, ScreenPos.y, _zDistCamToPlayer));
            Vector3 targetPos = inputWorldPos + _dragOffset;
            
            _draggingMem.CurPos = new Vector3(targetPos.x, targetPos.y, 0);
        }

        private void SubscribeInputEvents()
        {
            _playerInput.Enable();
            
            _pressAction.started += OnPressStarted;
            _pressAction.canceled += OnPressCanceled;
            
            _posAction.performed += OnPosPerformed;
            
            _pressAction.Enable();
            _posAction.Enable();
        }

        private void UnsubscribeInputEvents()
        {
            _playerInput.Disable();
            
            _pressAction.started -= OnPressStarted;
            _pressAction.canceled -= OnPressCanceled;
            
            _posAction.performed -= OnPosPerformed;
            
            _pressAction.Disable();
            _posAction.Disable();
        }
    }
}