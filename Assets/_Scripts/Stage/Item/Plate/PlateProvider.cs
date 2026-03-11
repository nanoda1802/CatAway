

using System;
using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Plate
{
    public class PlateProvider : ItemProvider<Plate>
    {
        public bool HasInactivePlate => Pool.CountInactive > 0;
        
        public override void OnNetworkSpawn()
        {
            InitPool();
            
            NetworkManager.PrefabHandler.AddHandler(Info.Prefab, new PlatePrefabHandler(this));
            
            base.OnNetworkSpawn();
        }
    
        public Plate GetPlate(Vector3 pos)
        {
            var plate = Pool.Get();
            plate.transform.parent.transform.position = pos;
            return plate;
        }
    
        public void ReleasePlate(Plate plate)
        {
            if (IsServer) plate.NetworkObject.TrySetParent(this.NetworkObject);
            Pool.Release(plate);
        }
    }
    
    // public class PlateProvider : NetworkBehaviour, IProvider
    // {
    //     private IObjectResolver _resolver;
    //     private NetworkObject _prefab;
    //     
    //     private IObjectPool<Plate> _pool;
    //     private PlateData _data;
    //
    //     private IDisposable _subscription;
    //     
    //     public bool HasInactivePlate => _pool.CountInactive > 0;
    //
    //     [Inject]
    //     private void Construct(
    //         IObjectResolver container,
    //         PlateData data,
    //         IPublisher<IProvider> pub,
    //         IBufferedSubscriber<PublishRequestMessage> sub)
    //     {
    //         _resolver = container;
    //         _data = data;
    //         _prefab = data.TempPrefab;
    //         
    //         pub.Publish(this);
    //         
    //         _subscription = sub.Subscribe(msg =>
    //         {
    //             if (!msg.IsRequest(this)) return;
    //             pub.Publish(this);
    //         });
    //     }
    //     
    //     public override void OnNetworkSpawn()
    //     {
    //         InitPool();
    //         
    //         var prefabHandler = new PlatePrefabHandler(this);
    //         NetworkManager.PrefabHandler.AddHandler(_prefab, prefabHandler);
    //         
    //         base.OnNetworkSpawn();
    //     }
    //
    //     public override void OnNetworkDespawn()
    //     {
    //         NetworkManager.PrefabHandler.RemoveHandler(_prefab);
    //         
    //         _subscription?.Dispose();
    //         
    //         base.OnNetworkDespawn();
    //     }
    //
    //     public void InitPool()
    //     {
    //         _pool = new ObjectPool<Plate>(
    //             CreatePlate, 
    //             OnGetPlate, 
    //             OnReleasePlate, 
    //             OnDestroyIngredient, 
    //             true, 
    //             _data.DefaultCount,
    //             _data.MaxCount);
    //         
    //         for (int i = 0; i < _data.DefaultCount; i++)
    //         {
    //             var ingredient = CreatePlate();
    //             _pool.Release(ingredient);
    //         }
    //     }
    //     
    //     private Plate CreatePlate()
    //     {
    //         var netObj = _resolver.Instantiate(_prefab,this.transform);
    //         netObj.name = $"Plate_{netObj.GetHashCode()}"; 
    //         return netObj.GetComponentInChildren<Plate>();
    //     }
    //     
    //     private void OnGetPlate(Plate plate)
    //     {
    //         plate.transform.parent.gameObject.SetActive(true);
    //     }
    //
    //     private void OnReleasePlate(Plate plate)
    //     {
    //         plate.transform.parent.gameObject.SetActive(false);
    //         plate.transform.parent.transform.localPosition = Vector3.zero;
    //         plate.transform.parent.transform.localRotation = Quaternion.identity;
    //     }
    //
    //     private void OnDestroyIngredient(Plate plate)
    //     {
    //         Destroy(plate.transform.parent.gameObject);
    //     }
    //
    //     public Plate GetPlate(Vector3 pos)
    //     {
    //         var plate = _pool.Get();
    //         plate.transform.parent.transform.position = pos;
    //         return plate;
    //     }
    //
    //     public void ReleasePlate(Plate plate)
    //     {
    //         if (IsServer) plate.NetworkObject.TrySetParent(this.NetworkObject);
    //         _pool.Release(plate);
    //     }
    // }
}