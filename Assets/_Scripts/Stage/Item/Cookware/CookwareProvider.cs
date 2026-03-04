using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Stage.Item.Cookware
{
    public class CookwareProvider : ItemProvider<Cookware>
    {
        public override void OnNetworkSpawn()
        {
            InitPool();
            
            NetworkManager.PrefabHandler.AddHandler(Info.Prefab, new CookwarePrefabHandler(this));
            
            base.OnNetworkSpawn();
        }
    
        public Cookware GetCookware(Vector3 pos)
        {
            var cookware = Pool.Get();
            cookware.transform.parent.transform.position = pos;
            return cookware;
        }
    
        public void ReleaseCookware(Cookware cookware)
        {
            if (IsServer) cookware.NetworkObject.TrySetParent(this.NetworkObject);
            Pool.Release(cookware);
        }
    }
}