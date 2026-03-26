using System;
using _Scripts.Scene_Stage.Item;
using Unity.Netcode.Components;

namespace _Scripts._Wrapper
{
    public class AttachableSlot : AttachableNode
    {
        public event Action<Carriable> OnAttach;
        public event Action<Carriable> OnDetach;

        protected override void OnAttached(AttachableBehaviour attachableBehaviour)
        {
            if (attachableBehaviour is not Carriable item) return;
            OnAttach?.Invoke(item);
            
            base.OnAttached(attachableBehaviour);
        }

        protected override void OnDetached(AttachableBehaviour attachableBehaviour)
        {
            if (attachableBehaviour is not Carriable item) return;
            OnDetach?.Invoke(item);
            
            base.OnDetached(attachableBehaviour);
        }

        public override void OnNetworkPreDespawn()
        {
            OnAttach = null;
            OnDetach = null;
            
            base.OnNetworkPreDespawn();
        }
    }
}