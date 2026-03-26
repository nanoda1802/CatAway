using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Scene_Stage.Player.Status
{
    public class MoveStatus : IDisposable
    {
        private readonly PlayerData _data;

        private Vector3 _moveDir;
        private float _speedMultiplier;
        private float _lastDashTime;

        private CancellationTokenSource _cts;

        private CancellationToken Token
        {
            get
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }

        public MoveStatus(PlayerData data)
        {
            _data = data;
        
            _moveDir = Vector3.zero;
            _speedMultiplier = _data.MinSpeedMultiplier;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public bool MoveConstraint { get; set; }
        public bool IsMovable => _moveDir != Vector3.zero && !MoveConstraint;
        public Vector3 MoveOffset => (_data.MoveSpeed * _speedMultiplier * Time.fixedDeltaTime) * _moveDir;
        public Quaternion LookRot => Quaternion.LookRotation(_moveDir);
        public float RotRatio => _data.RotSpeed * Time.fixedDeltaTime;
        public bool IsDashing { get; set; }
        public Vector3 DashForce => _data.DashSpeed * _moveDir;
        public UniTask WaitForDash => UniTask.WaitForSeconds(_data.DashDuration,false, PlayerLoopTiming.EarlyUpdate, Token);
        public bool IsDashAvailable => _lastDashTime + _data.DashInterval <= Time.unscaledTime;
        public float KnockBackImpact => _data.KnockBackImpact;
        public UniTask WaitForKnockBack =>
            UniTask.WaitForSeconds(_data.KnockBackDuration, false, PlayerLoopTiming.EarlyUpdate, Token);
        
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