using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item
{
    public class Carriable : AttachableBehaviour, INetworkUpdateSystem
    {
        [SF] private CarriableType carriableType;
        private Rigidbody _rb;
        
        public CarriableType Type => carriableType;
        
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
                    break;
            }

            base.OnAttachStateChanged(attachState, node);
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!IsSpawned || !HasAuthority) return;
            if (m_AttachState != AttachState.Attached) return;

            NetworkObject.transform.position = this.transform.position;
        }

        private void OnAttaching()
        {
            this.RegisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
            _rb.isKinematic = true;
        }

        private void OnAttached()
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void OnDetaching()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
        }

        private void OnDetached()
        {
            _rb.isKinematic = false;
        }
    }
}