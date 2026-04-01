using _Scripts.Scene_Stage.Data.Level;
using _Scripts.Stage;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Scene_Stage.Item
{
    public class ItemProvider<T> : NetworkBehaviour, IProvider where T : Carriable
    {
        private IObjectResolver _resolver;
        
        protected IObjectPool<T> Pool;

        protected ProviderInfo<NetworkObject> Info;
        
        [Inject]
        private void ConstructBase(
            IObjectResolver container,
            ProviderData data,
            IPublisher<IProvider> pub,
            IBufferedSubscriber<HubCallMessage> sub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _resolver = container;
            Info = data.GetItemProviderInfo<T>();
            
            pub.Publish(this);
            
            sub.Subscribe(msg =>
                {
                    if (!msg.IsRequest(this)) return;
                    pub.Publish(this);
                })
                .AddTo(disposableBagBuilder);
            
            // InitPool();
        }

        public override void OnNetworkSpawn()
        {
            InitPool();
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.PrefabHandler.RemoveHandler(Info.Prefab);
            
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
            var item = _resolver.Instantiate(Info.Prefab,this.transform);
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

        public virtual void Release(T item)
        {
            if (IsServer) item.NetObj.TrySetParent(this.NetworkObject);
            Pool.Release(item);
        }
    }
}