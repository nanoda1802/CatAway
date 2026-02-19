using _Scripts.Stage.Item.Ingredient;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace _Scripts.Stage.Item
{
    public class Throwable : NetworkBehaviour
    {
        private IngredientData _data;
        
        private Rigidbody _rb;
        private NetworkTransform _netTr;

        public bool IsThrowing { get; private set; }

        public void Construct(Rigidbody rb, NetworkTransform netTr)
        {
            _rb = rb;
            _netTr = netTr;
        }

        public void InitStatus(IngredientData data)
        {
            _data = data;
        }

        public async UniTaskVoid Throw(Vector3 origin, Quaternion rot, Vector3 dir)
        {
            if (IsThrowing) return;
        
            await UniTask.WaitUntil(() => !_rb.isKinematic);

            IsThrowing = true;
            
            _netTr.Teleport(origin, rot, Vector3.one);
            _rb.linearVelocity = _data.ThrowForce * dir;
            
            await UniTask.WaitWhile(() => Throwing(dir, origin));
            
            await UniTask.WaitWhile(() => Falling());
            
            IsThrowing = false;
        }
        
        private bool Throwing(Vector3 dir, Vector3 origin)
        {
            if (_rb.isKinematic) return false;
            if (_rb.linearVelocity.sqrMagnitude < _data.ValidVelocityCutOff) return false;
            
            _rb.linearVelocity = _data.ThrowForce * dir;
            
            return (origin - _rb.position).sqrMagnitude < _data.DampingThreshold;
        }

        private bool Falling()
        {
            if (_rb.isKinematic) return false;
        
            _rb.linearVelocity *= _data.DampingRatio;
            
            return _rb.linearVelocity.sqrMagnitude > 0.5f;
        }

        public bool HasEnoughVelocity(Vector3 relativeVelocity)
        {
            return relativeVelocity.sqrMagnitude > _data.ValidVelocityCutOff;
        }
    }
}