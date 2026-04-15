using UnityEngine;

namespace _Scripts.Stage.Item.Cookware
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
    }
}