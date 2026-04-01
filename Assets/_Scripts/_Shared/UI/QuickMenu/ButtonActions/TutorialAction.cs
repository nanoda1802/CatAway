using _Scripts._Shared.Enums;
using _Scripts._Shared.UI.Pop;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using MessagePipe;
using UnityEngine;

namespace _Scripts._Shared.UI.QuickMenu.ButtonActions
{
    public class TutorialAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Tutorial;

        private readonly IPublisher<PopUpMessage> _popUpPub;
        private readonly IPublisher<SwitchReadyRequest> _switchReadyPub;
        
        private float _lastClickTime;
        private bool IsValidClick => Time.time - _lastClickTime >= 1f;
        
        public TutorialAction(
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<SwitchReadyRequest> switchReadyPub)
        {
            _popUpPub = popUpPub;
            _switchReadyPub = switchReadyPub;
        }

        public void OnClick()
        {
            if (!IsValidClick) return;
            _lastClickTime = Time.time;
            
            var popUpMsg = new PopUpMessage(typeof(TutorialPop));
            var switchReadyReq = new SwitchReadyRequest(true);
            
            _popUpPub.Publish(popUpMsg);
            _switchReadyPub.Publish(switchReadyReq);
        }
    }
}