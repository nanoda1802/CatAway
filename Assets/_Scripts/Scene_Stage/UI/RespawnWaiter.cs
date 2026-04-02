using System.Timers;
using UnityEngine;

namespace _Scripts.Scene_Stage
{
    public class RespawnWaiter
    {
        public readonly ulong ClientId;
        public readonly Vector3 RespawnPoint;
        private float _waitTimer;
        
        public RespawnWaiter(ulong clientId, float waitTimer, Vector3 respawnPoint)
        {
            ClientId = clientId;
            _waitTimer = waitTimer;
            RespawnPoint = respawnPoint;
        }

        public bool UpdateTimer()
        {
            _waitTimer -= Time.deltaTime;
            return (_waitTimer <= 0);
        }
    }
}