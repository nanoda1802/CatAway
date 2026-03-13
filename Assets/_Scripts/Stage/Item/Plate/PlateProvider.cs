

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
}