using _Scripts.Stage._Data;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Stage.Player
{
    public class PlayerPrefabHandler : NetworkPrefabInstanceHandlerWithData<PlayerSpawnPacket>
    {
        private readonly LevelInitiator _levelInitiator;

        public PlayerPrefabHandler(LevelInitiator levelInitiator)
        {
            _levelInitiator = levelInitiator;
        }

        public override NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation,
            PlayerSpawnPacket instantiationData)
        {
            var player = _levelInitiator.CreatePlayer(position).ApplySpawnInfo(instantiationData.IsRespawn);
            return player.NetObj;
        }

        public override void Destroy(NetworkObject networkObject)
        {
            Object.Destroy(networkObject.gameObject);
        }
    }
}