using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Scene_Stage.Table
{
    public class TablePrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly IObjectResolver _resolver;
        private readonly NetworkObject _prefab;

        public TablePrefabHandler(IObjectResolver resolver, NetworkObject prefab)
        {
            _resolver = resolver;
            _prefab = prefab;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return _resolver.Instantiate(_prefab, position, rotation);
        }

        public void Destroy(NetworkObject networkObject)
        {
            Object.Destroy(networkObject);
        }
    }
}