using _Scripts.Room._Messages;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI.Pop;
using MessagePipe;
using UnityEngine;

namespace _Scripts.Shared.UI.QuickMenu.ButtonActions
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