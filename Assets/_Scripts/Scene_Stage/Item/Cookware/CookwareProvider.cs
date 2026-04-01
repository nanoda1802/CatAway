using UnityEngine;

namespace _Scripts.Scene_Stage.Item.Cookware
{
    public class CookwareProvider : ItemProvider<Cookware>
    {
        public override void OnNetworkSpawn()
        { 
            base.OnNetworkSpawn();
            NetworkManager.PrefabHandler.AddHandler(Info.Prefab, new CookwarePrefabHandler(this));
        }
    
        public Cookware GetCookware(Vector3 pos)
        {
            var cookware = Pool.Get();
            cookware.transform.parent.transform.position = pos;
            return cookware;
        }
    
        // public override void Release(Ingredient item)
        // {
        //     base.Release(item);
        // }
        
        // public void ReleaseCookware(Cookware cookware)
        // {
        //     if (IsServer) cookware.NetworkObject.TrySetParent(this.NetworkObject);
        //     Pool.Release(cookware);
        // }
    }
}