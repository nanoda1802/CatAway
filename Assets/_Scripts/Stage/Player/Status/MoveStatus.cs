using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Stage.Player.Status
{
    public class MoveStatus
    {
        private readonly PlayerData _data;

        private Vector3 _moveDir;
        private float _speedMultiplier;
        private float _lastDashTime;
    
        public MoveStatus(PlayerData data)
        {
            _data = data;
        
            _moveDir = Vector3.zero;
            _speedMultiplier = _data.MinSpeedMultiplier;
        }

        public bool IsMovable => _moveDir.sqrMagnitude > 0 && !IsDashing;
        public Vector3 MoveOffset => (_data.MoveSpeed * _speedMultiplier) * _moveDir;
        public Quaternion LookRot => Quaternion.LookRotation(_moveDir);
        public float RotRatio => _data.RotSpeed * Time.deltaTime;
        public bool IsDashing { get; set; }
        public Vector3 DashForce => _data.DashSpeed * _moveDir;
        public UniTask WaitForDash => UniTask.WaitForSeconds(_data.DashDuration,false,PlayerLoopTiming.EarlyUpdate);
        public bool IsDashAvailable => _lastDashTime + _data.DashInterval <= Time.unscaledTime;
        public float SpeedMultiplier => _speedMultiplier;

        public void SetMoveDirection(Vector2 inputValue)
        {
            _moveDir = new Vector3(inputValue.x, 0, inputValue.y);
        }

        public float UpdateSpeedMultiplier(bool hasDash)
        {
            _speedMultiplier = hasDash ? _data.MaxSpeedMultiplier : _data.MinSpeedMultiplier;
            return _speedMultiplier;
        }
    
        public void UpdateLastDashTime()
        {
            _lastDashTime = Time.unscaledTime;
        }
    }
}