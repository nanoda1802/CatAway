using _Scripts._Shared.Enums;
using _Scripts.Messages.StageResult;
using MessagePipe;
using UnityEngine;

namespace _Scripts._Shared.UI.QuickMenu.ButtonActions
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