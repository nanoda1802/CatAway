using System;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Stage.Item
{
    public class ItemProvider<T> : NetworkBehaviour, IProvider where T : Carriable
    {
        private IObjectResolver _container;
        
        protected IObjectPool<T> Pool;

        protected ProviderInfo<NetworkObject> Info;
        
        private IDisposable _subscription;
        
        [Inject]
        private void ConstructBase(
            IObjectResolver container,
            ProviderData data,
            IPublisher<IProvider> pub,
            IBufferedSubscriber<PublishRequestMessage> sub)
        {
            _container = container;
            Info = data.GetItemProviderInfo<T>();
            
            pub.Publish(this);
            
            _subscription = sub.Subscribe(msg =>
            {
                if (!msg.IsRequest(this)) return;
                pub.Publish(this);
            });
        }
        
        public override void OnNetworkDespawn()
        {
            NetworkManager.PrefabHandler.RemoveHandler(Info.Prefab);
            
            _subscription?.Dispose();
            
            base.OnNetworkDespawn();
        }
        
        public void InitPool()
        {
            Pool = new ObjectPool<T>(
                CreateItem, 
                OnGetItem, 
                OnReleaseItem, 
                OnDestroyItem, 
                true, 
                Info.DefaultCount,
                Info.MaxCount);
            
            for (int i = 0; i < Info.DefaultCount; i++)
            {
                var item = CreateItem();
                Pool.Release(item);
            }
        }
        
        private T CreateItem()
        {
            var item = _container.Instantiate(Info.Prefab,this.transform);
            item.name = $"{Info.ObjNamePrefix}_{item.GetHashCode()}";
            return item.GetComponentInChildren<T>();
        }
        
        private void OnGetItem(T item)
        {
            item.transform.parent.gameObject.SetActive(true);
        }
    
        private void OnReleaseItem(T item)
        {
            item.transform.parent.gameObject.SetActive(false);
            item.transform.parent.transform.localPosition = Vector3.zero;
            item.transform.parent.transform.localRotation = Quaternion.identity;
        }

        private void OnDestroyItem(T item)
        {
            Destroy(item.transform.parent.gameObject);
        }
    }
}