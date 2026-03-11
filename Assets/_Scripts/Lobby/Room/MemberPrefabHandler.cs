using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Lobby.Room
{
    public class MemberPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly MemberSyncer _syncer;

        public MemberPrefabHandler(MemberSyncer syncer)
        {
            _syncer = syncer;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var member = _syncer.CreateMemberObject(ownerClientId);
            return member.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject)
        {
            Object.Destroy(networkObject.gameObject);
        }
    }
}