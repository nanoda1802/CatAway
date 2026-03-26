using _Scripts.Scene_Stage.Table;
using UnityEngine;

namespace _Scripts.Scene_Stage.Player.Status
{
    public class InteractStatus
    {
        private readonly PlayerData _data;
    
        private float _lastInteractTime;
        private int _curAnimParamHash = -1;
    
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

        public void StartInteractionAnim(Animator animator, int paramHash)
        {
            _curAnimParamHash = paramHash;
            animator.SetBool(_curAnimParamHash, true);
        }

        public void StopInteractionAnim(Animator animator)
        {
            animator.SetBool(_curAnimParamHash, false);
            _curAnimParamHash = -1;
        }
    }
}