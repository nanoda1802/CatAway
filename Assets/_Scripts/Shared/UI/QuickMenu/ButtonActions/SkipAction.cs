using _Scripts.Result._Messages;
using _Scripts.Shared._Enums;
using MessagePipe;
using UnityEngine;

namespace _Scripts.Shared.UI.QuickMenu.ButtonActions
{
    public class SkipAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Skip;

        private readonly IPublisher<SkipRequest> _skipPub;
        
        private float _lastClickTime;
        private bool IsValidClick => Time.time - _lastClickTime >= 1f;
        
        public SkipAction(IPublisher<SkipRequest> skipPub)
        {
            _skipPub = skipPub;
        }

        public void OnClick()
        {
            if (!IsValidClick) return;
            _lastClickTime = Time.time;
            
            _skipPub.Publish(new SkipRequest());
        }
    }
}