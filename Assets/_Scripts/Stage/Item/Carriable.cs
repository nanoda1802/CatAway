using _Scripts.Stage.Player.Behaviour;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item
{
    public class Carriable : AttachableBehaviour, INetworkUpdateSystem
    {
        [SF] private CarriableType carriableType;
        private Rigidbody _rb;
        
        public CarriableType Type => carriableType;
        public bool IsAttach => m_AttachState is AttachState.Attaching or AttachState.Attached;
        
        public void Construct(Rigidbody rb)
        {
            _rb = rb;
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
                    Debug.LogError($"[{this.OwnerClientId} Carriable.OnAttachStateChanged] \"{attachState}\"은 존재하지 않는 AttachState 입니다.");
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

        private void OnAttaching()
        {
            _rb.linearVelocity = _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        private void OnAttached()
        {
            Debug.Log($"[OnAttached] {this.NetworkObject.gameObject.name} attach to {m_AttachableNode.NetworkObject.gameObject.name} (carrier ? {this.m_AttachableNode is CarrierBehaviour})");
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            SyncWithNetObjPosition();
            
            if (this.m_AttachableNode is CarrierBehaviour) this.RegisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
        }

        private void OnDetaching()
        {
            NetworkObject.transform.rotation = this.transform.rotation;
            this.UnregisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
        }

        private void OnDetached()
        {
            _rb.isKinematic = false;
        }

        private void SyncWithNetObjPosition()
        {
            NetworkObject.transform.position = this.transform.position;
        }
    }
}