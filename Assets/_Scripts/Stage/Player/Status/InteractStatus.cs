using _Scripts.Stage.Table;
using UnityEngine;

namespace _Scripts.Stage.Player.Status
{
    public class InteractStatus
    {
        private readonly PlayerData _data;
    
        private float _lastInteractTime;
    
        public InteractStatus(PlayerData data)
        {
            _data = data;
        }
    
        public bool IsInteractAvailable => _lastInteractTime + _data.InteractInterval <= Time.unscaledTime;
        public IInteractable CurInteractable { get; set; }
        public bool IsInteracting => CurInteractable != null;

        public void UpdateLastInteractTime()
        {
            _lastInteractTime = Time.unscaledTime;
        }
    }
}