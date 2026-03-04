using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Ingredient
{
    public class IngredientProvider : ItemProvider<Ingredient>
    {
        private readonly Dictionary<IngredientType, IngredientData> _dataDic = new ();
        public IngredientType RequiredType { get; private set; }

        [Inject]
        private void Construct(
            StageData stageData,
            IngredientData[] dataList)
        {
            RequiredType = stageData.OrderInfo.RequiredType;
            
            foreach (var data in dataList)
            {
                _dataDic.TryAdd(data.Type, data);
            }
            
            BakeMeshes();
        }

        public override void OnNetworkSpawn()
        {
            InitPool();
            
            NetworkManager.PrefabHandler.AddHandler(Info.Prefab, new IngredientPrefabHandler(this));
            
            base.OnNetworkSpawn();
        }

        private void BakeMeshes()
        {
            MeshColliderCookingOptions options = MeshColliderCookingOptions.CookForFasterSimulation |
                                                        MeshColliderCookingOptions.EnableMeshCleaning |
                                                        MeshColliderCookingOptions.UseFastMidphase |
                                                        MeshColliderCookingOptions.WeldColocatedVertices;
            
            foreach (var data in _dataDic.Values)
            {
                data.BakeColliderMesh(options);
            }
        }
    
        public Ingredient GetIngredient(IngredientType type, Vector3 pos)
        {
            var ingredient = Pool.Get();
            var data = _dataDic.GetValueOrDefault(type, _dataDic[RequiredType]);
            ingredient.InitData(data, data.Type == RequiredType);
            ingredient.transform.parent.transform.position = pos;
            return ingredient;
        }
    
        public void ReleaseIngredient(Ingredient ingredient)
        {
            if (IsServer) ingredient.NetworkObject.TrySetParent(this.NetworkObject);
            Pool.Release(ingredient);
        }

        public (Mesh, Vector3) GetModelInfo(IngredientType type)
        {
            var data = _dataDic.GetValueOrDefault(type, _dataDic[RequiredType]);
            var modelInfo = data.GetModelInfo();
            return (modelInfo.RenderMesh, modelInfo.Scale);
        }
    }
    
    // public class IngredientProvider : NetworkBehaviour, IProvider
    // {
    //     // [수정] 이 세 데이터 나중에 StageData 등에서 받아오기
    //     [SF] private int defaultCapacity = 20;
    //     [SF] private int maxPoolSize = 40;
    //     [SF] private IngredientType requiredType;
    //     
    //     private IObjectResolver _container;
    //     private NetworkObject _prefab;
    //     
    //     private IObjectPool<Ingredient> _pool;
    //     private readonly Dictionary<IngredientType, IngredientData> _dataDic = new ();
    //
    //     private IDisposable _subscription;
    //     
    //     public IngredientType RequiredType => requiredType;
    //
    //     [Inject]
    //     private void Construct(
    //         IObjectResolver container,
    //         IngredientData[] dataList,
    //         IPublisher<IProvider> pub,
    //         IBufferedSubscriber<PublishRequestMessage> sub)
    //     {
    //         _container = container;
    //
    //         foreach (var data in dataList)
    //         {
    //             _dataDic.TryAdd(data.Type, data);
    //         }
    //         
    //         _prefab = dataList[0].TempPrefab; // 임시
    //         
    //         BakeMeshes();
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
    //         var prefabHandler = new IngredientPrefabHandler(this);
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
    //     private void BakeMeshes()
    //     {
    //         MeshColliderCookingOptions options = MeshColliderCookingOptions.CookForFasterSimulation |
    //                                                     MeshColliderCookingOptions.EnableMeshCleaning |
    //                                                     MeshColliderCookingOptions.UseFastMidphase |
    //                                                     MeshColliderCookingOptions.WeldColocatedVertices;
    //         
    //         foreach (var data in _dataDic.Values)
    //         {
    //             data.BakeColliderMesh(options);
    //         }
    //     }
    //
    //     public void InitPool()
    //     {
    //         _pool = new ObjectPool<Ingredient>(
    //             CreateIngredient, 
    //             OnGetIngredient, 
    //             OnReleaseIngredient, 
    //             OnDestroyIngredient, 
    //             true, 
    //             defaultCapacity,
    //             maxPoolSize);
    //         
    //         for (int i = 0; i < defaultCapacity; i++)
    //         {
    //             var ingredient = CreateIngredient();
    //             _pool.Release(ingredient);
    //         }
    //     }
    //     
    //     private Ingredient CreateIngredient()
    //     {
    //         var o = _prefab.transform.root;
    //         
    //         var netObj = _container.Instantiate(o,this.transform);
    //         // var netObj = _container.Instantiate(_prefab,this.transform);
    //         netObj.name = $"Ingredient_{netObj.GetHashCode()}";
    //         return netObj.GetComponentInChildren<Ingredient>();
    //     }
    //     
    //     private void OnGetIngredient(Ingredient ingredient)
    //     {
    //         ingredient.transform.parent.gameObject.SetActive(true);
    //     }
    //
    //     private void OnReleaseIngredient(Ingredient ingredient)
    //     {
    //         ingredient.transform.parent.gameObject.SetActive(false);
    //         ingredient.transform.parent.transform.localPosition = Vector3.zero;
    //         ingredient.transform.parent.transform.localRotation = Quaternion.identity;
    //     }
    //
    //     private void OnDestroyIngredient(Ingredient ingredient)
    //     {
    //         Destroy(ingredient.transform.parent.gameObject);
    //     }
    //
    //     public Ingredient GetIngredient(IngredientType type, Vector3 pos)
    //     {
    //         var ingredient = _pool.Get();
    //         var data = _dataDic.GetValueOrDefault(type, _dataDic[requiredType]);
    //         ingredient.InitData(data, data.Type == requiredType);
    //         ingredient.transform.parent.transform.position = pos;
    //         return ingredient;
    //     }
    //
    //     public void ReleaseIngredient(Ingredient ingredient)
    //     {
    //         if (IsServer) ingredient.NetworkObject.TrySetParent(this.NetworkObject);
    //         _pool.Release(ingredient);
    //     }
    //
    //     public (Mesh, Vector3) GetModelInfo(IngredientType type)
    //     {
    //         var data = _dataDic.GetValueOrDefault(type, _dataDic[requiredType]);
    //         var modelInfo = data.GetModelInfo();
    //         return (modelInfo.RenderMesh, modelInfo.Scale);
    //     }
    // }
}