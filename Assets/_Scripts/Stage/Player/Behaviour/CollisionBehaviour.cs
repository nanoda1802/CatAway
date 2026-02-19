using System;
using _Scripts.Stage.Item;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Status;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.Player.Behaviour
{
    public class CollisionBehaviour : NetworkBehaviour
    {
        private MoveStatus _moveStatus;
        
        private CarrierBehaviour _carrierBehaviour;
        
        private Rigidbody _playerRb;
     
        private TagHandle _itemTag;
        
        [Inject]
        private void Construct(
            Rigidbody playerRb,
            MoveStatus moveStatus,
            CarrierBehaviour carrierBehaviour)
        {
            _playerRb = playerRb;
            _moveStatus = moveStatus;
            _carrierBehaviour = carrierBehaviour;
            
            _itemTag = TagHandle.GetExistingTag("Item");
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!IsServer || !IsSpawned) return;
            if (!other.collider.CompareTag(_itemTag)) return;
            if (!other.collider.TryGetComponent(out Throwable throwable)) return;
            if (!throwable.IsThrowing || !throwable.HasEnoughVelocity(other.relativeVelocity)) return;

            if (_carrierBehaviour.HasAttachments)
            {
                KnockBackClientRpc(other.relativeVelocity, RpcTarget.Single(this.OwnerClientId, RpcTargetUse.Temp));
            }
            else
            {
                if (!other.collider.TryGetComponent(out Carriable carriable)) return;
                carriable.Attach(_carrierBehaviour);
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void KnockBackClientRpc(Vector3 knockBackDir, RpcParams rpcParams = default)
        {
            knockBackDir.y = 0;
            knockBackDir.Normalize();
            
            KnockBack(knockBackDir).Forget();
        }

        private async UniTaskVoid KnockBack(Vector3 dir)
        {
            _moveStatus.MoveConstraint = true; 
            
            _playerRb.linearVelocity = _playerRb.angularVelocity = Vector3.zero;
            _playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; 
            
            _playerRb.AddForce(_moveStatus.KnockBackImpact * dir, ForceMode.VelocityChange);

            await _moveStatus.WaitForKnockBack;

            _moveStatus.MoveConstraint = false;
            
            _playerRb.linearVelocity *= 0.5f;
            _playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}