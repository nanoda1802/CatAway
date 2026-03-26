using _Scripts.Scene_Stage.Item;
using UnityEngine;

namespace _Scripts.Scene_Stage.Player.Status
{
    public class CarryStatus
    {
        private readonly PlayerData _data;
        private float _lastCarryTime;

        public CarryStatus(PlayerData data)
        {
            _data = data;
        }
   
        public bool IsCarryAvailable => _lastCarryTime + _data.CarryInterval <= Time.unscaledTime;
        public Carriable CurCarriable { get; set; }
        public bool HasCarriable => CurCarriable != null;
   
        public void UpdateLastCarryTime()
        {
            _lastCarryTime = Time.unscaledTime;
        }
    }
}