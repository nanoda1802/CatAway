using _Scripts.Scene_Result;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Scene_Room
{
    public class ResultMemberPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly ResultMemberSyncer _syncer;

        public ResultMemberPrefabHandler(ResultMemberSyncer syncer)
        {
            _syncer = syncer;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var member = _syncer.CreateMemberObject(ownerClientId, position, rotation);
            return member.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject)
        {
            Object.Destroy(networkObject.gameObject);
        }
    }
}