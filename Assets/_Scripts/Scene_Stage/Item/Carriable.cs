using _Scripts.Scene_Stage.Player.Behaviour;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

namespace _Scripts.Scene_Stage.Item
{
    public class Carriable : AttachableBehaviour, INetworkUpdateSystem
    {
        protected Rigidbody ItemRb;
        protected MeshCollider ItemCollider;
        
        private int _defaultLayerMask;
        private int _ignoreRayCastLayerMask;
        
        public bool IsCarrying => m_AttachState is AttachState.Attaching or AttachState.Attached;
        
        [Inject]
        public virtual void Construct()
        {
            ItemRb = this.transform.parent.GetComponentInChildren<Rigidbody>();
            
            ItemCollider = this.transform.parent.GetComponentInChildren<MeshCollider>();
            ItemCollider.convex = true;
            
            _defaultLayerMask = LayerMask.NameToLayer("Item");
            _ignoreRayCastLayerMask = LayerMask.NameToLayer("Ignore Raycast");
        }

        public override void OnNetworkSpawn()
        {
            ItemCollider.enabled = HasAuthority;
            ItemRb.detectCollisions = HasAuthority;
            
            base.OnNetworkSpawn();
        }

        protected override void OnAttachStateChanged(AttachState attachState, AttachableNode node)
        {
            if (!IsSpawned || !HasAuthority || !node) return;

            switch (attachState)
            {
                case AttachState.Attaching:
                    OnAttaching(); break;
                case AttachState.Attached:
                    OnAttached(); break;
                case AttachState.Detaching:
                    OnDetaching(); break;
                case AttachState.Detached:
                    OnDetached(); break;
                default:
                    break;
            }

            base.OnAttachStateChanged(attachState, node);
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!IsSpawned || !HasAuthority) return;
            if (m_AttachState != AttachState.Attached) return;

            SyncWithNetObjPosition();
        }

        protected virtual void OnAttaching()
        {
            ItemRb.linearVelocity = ItemRb.angularVelocity = Vector3.zero;
            ItemRb.isKinematic = true;
            this.gameObject.layer = _ignoreRayCastLayerMask;
            
            if (HasAuthority) ItemCollider.enabled = false;
        }

        private void OnAttached()
        {
            SyncWithNetObjPosition();
            
            if (this.m_AttachableNode is CarrierBehaviour) 
                this.RegisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
        }

        private void OnDetaching()
        {
            NetworkObject.transform.rotation = this.transform.rotation;
            this.UnregisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
        }

        private void OnDetached()
        {
            ItemRb.isKinematic = false;
            this.gameObject.layer = _defaultLayerMask;
            
            if (HasAuthority) ItemCollider.enabled = true;
        }

        private void SyncWithNetObjPosition()
        {
            NetworkObject.transform.position = this.transform.position;
        }
    }
}