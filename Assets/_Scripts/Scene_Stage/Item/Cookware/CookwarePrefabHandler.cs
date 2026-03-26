using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Scene_Stage.Item.Cookware
{
    public class CookwarePrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly CookwareProvider _provider;

        public CookwarePrefabHandler(CookwareProvider provider)
        {
            _provider = provider;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return _provider.GetCookware(position).NetworkObject;
        }

        public void Destroy(NetworkObject networkObject)
        {
            var cookware = networkObject.GetComponentInChildren<Cookware>();

            if (cookware != null)
            {
                _provider.ReleaseCookware(cookware);
            }
            else
            {
                Object.Destroy(networkObject.gameObject);
            }
        }
    }
}