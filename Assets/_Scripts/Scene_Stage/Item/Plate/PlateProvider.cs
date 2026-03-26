using UnityEngine;

namespace _Scripts.Scene_Stage.Item.Plate
{
    public class PlateProvider : ItemProvider<Plate>
    {
        public bool HasInactivePlate => Pool.CountInactive > 0;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            NetworkManager.PrefabHandler.AddHandler(Info.Prefab, new PlatePrefabHandler(this));
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