using _Scripts.Shared._Enums;
using UnityEngine;

namespace _Scripts.Shared.UI.QuickMenu.ButtonActions
{
    public class ExitAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Exit;

        private float _lastClickTime;
        private bool IsValidClick => Time.time - _lastClickTime >= 1f;
        
        public ExitAction()
        {
            
        }

        public void OnClick()
        {
            if (!IsValidClick) return;
            _lastClickTime = Time.time;

            Debug.Log("Exit Game");
        }
    }
}