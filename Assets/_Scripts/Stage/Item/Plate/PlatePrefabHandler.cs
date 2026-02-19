using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Stage.Item.Plate
{
    public class PlatePrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly PlateProvider _provider;

        public PlatePrefabHandler(PlateProvider provider)
        {
            _provider = provider;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return _provider.GetPlate(position).NetworkObject;
        }

        public void Destroy(NetworkObject networkObject)
        {
            if (networkObject.TryGetComponent(out Plate plate))
            {
                _provider.ReleasePlate(plate);
            }
            else
            {
                Object.Destroy(networkObject.gameObject);
            }
        }
    }
}