using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.Item
{
    public class Carriable : AttachableBehaviour, INetworkUpdateSystem, IDespawnable
    {
        protected StageHub StageHub;
        
        protected Rigidbody ItemRb;
        protected MeshCollider ItemCollider;
        
        private int _defaultLayerMask;
        private int _ignoreRayCastLayerMask;
        
        public NetworkObject NetObj { get; private set; }
        public bool IsCarrying => m_AttachState is AttachState.Attaching or AttachState.Attached;
        
        [Inject]
        public void ConstructBase(StageHub stageHub)
        {
            NetObj = this.transform.parent.GetComponentInChildren<NetworkObject>();
            ItemRb = this.transform.parent.GetComponentInChildren<Rigidbody>();
            
            ItemCollider = this.transform.parent.GetComponentInChildren<MeshCollider>();
            ItemCollider.convex = true;
            
            _defaultLayerMask = LayerMask.NameToLayer("Item");
            _ignoreRayCastLayerMask = LayerMask.NameToLayer("Ignore Raycast");
            
            StageHub =  stageHub;
        }

        public virtual void Despawn()
        {
            this?.NetObj?.Despawn(false);
        }
        
        public override void OnNetworkSpawn()
        {
            InitPhysics();
            
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
        
        private void InitPhysics()
        {
            if (ItemCollider != null) ItemCollider.enabled = HasAuthority;
            if (ItemRb != null) ItemRb.detectCollisions = HasAuthority;
        }

        private void SyncWithNetObjPosition()
        {
            NetworkObject.transform.position = this.transform.position;
        }
    }
}