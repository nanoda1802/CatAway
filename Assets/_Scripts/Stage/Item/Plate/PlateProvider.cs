

using _Scripts.Stage.Item.Ingredient;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Plate
{
    public class PlateProvider : NetworkBehaviour
    {
        // [수정] 이 두 데이터 나중에 StageData 등에서 받아오기
        [SF] private int defaultCapacity = 6;
        [SF] private int maxPoolSize = 6;
        
        private IObjectResolver _container;
        private NetworkObject _prefab;
        
        private IObjectPool<Plate> _pool;
        private PlateData _data;

        public bool HasInactivePlate => _pool.CountInactive > 0;

        [Inject]
        private void Construct(
            IObjectResolver container,
            PlateData data)
        {
            _container = container;
            _data = data;
            _prefab = data.TempPrefab;
        }
        
        public override void OnNetworkSpawn()
        {
            InitPool();
            
            // var plateNetObj = _prefab.GetComponent<NetworkObject>();
            var prefabHandler = new PlatePrefabHandler(this);
            NetworkManager.PrefabHandler.AddHandler(_prefab, prefabHandler);
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            // var plateNetObj = _prefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.RemoveHandler(_prefab);
            
            base.OnNetworkDespawn();
        }
    
        private void InitPool()
        {
            _pool = new ObjectPool<Plate>(
                CreatePlate, 
                OnGetPlate, 
                OnReleasePlate, 
                OnDestroyIngredient, 
                true, 
                defaultCapacity,
                maxPoolSize);
            
            for (int i = 0; i < defaultCapacity; i++)
            {
                var ingredient = CreatePlate();
                _pool.Release(ingredient);
            }
        }
        
        private Plate CreatePlate()
        {
            var netObj = _container.Instantiate(_prefab,this.transform);
            netObj.name = $"Plate_{netObj.GetHashCode()}"; 
            return netObj.GetComponentInChildren<Plate>();
        }
        
        private void OnGetPlate(Plate plate)
        {
            plate.transform.parent.gameObject.SetActive(true);
        }
    
        private void OnReleasePlate(Plate plate)
        {
            plate.transform.parent.gameObject.SetActive(false);
            plate.transform.parent.transform.localPosition = Vector3.zero;
            plate.transform.parent.transform.localRotation = Quaternion.identity;
        }
    
        private void OnDestroyIngredient(Plate plate)
        {
            Destroy(plate.transform.parent.gameObject);
        }
    
        public Plate GetPlate(Vector3 pos)
        {
            var plate = _pool.Get();
            plate.transform.parent.transform.position = pos;
            return plate;
        }
    
        public void ReleasePlate(Plate plate)
        {
            if (IsServer) plate.NetworkObject.TrySetParent(this.NetworkObject);
            _pool.Release(plate);
        }
    }
}