using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace _Scripts.Stage.Item.Ingredient
{
    public class Throwable : NetworkBehaviour
    {
        private IngredientData _data;
        private ThrowState _curState = ThrowState.None;
        
        private Rigidbody _rb;
        private NetworkTransform _netTr;

        public bool IsThrowing => _curState != ThrowState.None;
        
        public void Construct(IngredientData data, Rigidbody rb, NetworkTransform netTr)
        {
            _data = data;
            _rb = rb;
            _netTr = netTr;
        }

        public async UniTaskVoid Throw(Vector3 origin, Quaternion rot, Vector3 dir)
        {
            if (IsThrowing) return;
        
            await UniTask.WaitUntil(() => !_rb.isKinematic);

            _netTr.Teleport(origin, rot, Vector3.one);
            
            _curState = ThrowState.Throwing;
            await UniTask.WaitWhile(() => Throwing(dir, origin));
        
            _curState = ThrowState.Falling;
            await UniTask.WaitWhile(() => Falling());
        
            StopThrowing();
        }
        
        private bool Throwing(Vector3 dir, Vector3 origin)
        {
            if (_rb.isKinematic) return false;
            if (_curState != ThrowState.Throwing) return false;
        
            _rb.linearVelocity = _data.ThrowForce * dir;
            return (origin - _rb.position).sqrMagnitude < _data.DampingThreshold;
        }

        private bool Falling()
        {
            if (_rb.isKinematic) return false;
            if (_curState != ThrowState.Falling) return false;
        
            _rb.linearVelocity *= _data.DampingRatio;
            return _rb.linearVelocity.sqrMagnitude > 0.001f;
        }

        public void StopThrowing()
        {
            _curState = ThrowState.None;
        }
    }
}