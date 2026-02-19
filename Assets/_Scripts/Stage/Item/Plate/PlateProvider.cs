

using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Plate
{
    public class PlateProvider : NetworkBehaviour
    {
        [SF] private Plate prefab;
        [SF] private PlateData plateData;
        [SF] private int defaultCapacity = 6;
        [SF] private int maxPoolSize = 6;
        
        private IObjectPool<Plate> _pool;

        public bool HasInactivePlate => _pool.CountInactive > 0;
        
        public override void OnNetworkSpawn()
        {
            var plateNetObj = prefab.GetComponent<NetworkObject>();
            var prefabHandler = new PlatePrefabHandler(this);
            NetworkManager.PrefabHandler.AddHandler(plateNetObj, prefabHandler);
            
            InitPool();
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            var plateNetObj = prefab.GetComponent<NetworkObject>();
            NetworkManager.PrefabHandler.RemoveHandler(plateNetObj);
            
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
            var plate = Instantiate(prefab,this.transform);
            plate.name = $"Plate_{plate.GetHashCode()}";
            plate.InitComponents(IsServer, plateData);
            return plate;
        }
        
        private void OnGetPlate(Plate plate)
        {
            plate.gameObject.SetActive(true);
        }
    
        private void OnReleasePlate(Plate plate)
        {
            plate.gameObject.SetActive(false);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localRotation = Quaternion.identity;
        }
    
        private void OnDestroyIngredient(Plate plate)
        {
        }
    
        public Plate GetPlate(Vector3 pos)
        {
            var plate = _pool.Get();
            plate.transform.position = pos;
            return plate;
        }
    
        public void ReleasePlate(Plate plate)
        {
            if (IsServer) plate.NetworkObject.TrySetParent(this.NetworkObject);
            _pool.Release(plate);
        }
    }
}