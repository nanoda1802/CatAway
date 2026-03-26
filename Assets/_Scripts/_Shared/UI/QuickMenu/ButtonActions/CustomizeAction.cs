using _Scripts._Shared.Enums;
using _Scripts._Shared.UI.Pop;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using MessagePipe;

namespace _Scripts._Shared.UI.QuickMenu.ButtonActions
{
    public class CustomizeAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Customize;

        private readonly IPublisher<PopUpMessage> _popUpPub;
        private readonly IPublisher<SwitchReadyRequest> _switchReadyPub;
        
        public CustomizeAction(
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<SwitchReadyRequest> switchReadyPub)
        {
            _popUpPub = popUpPub;
            _switchReadyPub = switchReadyPub;
        }

        public void OnClick()
        {
            var popUpMsg = new PopUpMessage(typeof(CustomizePop));
            var switchReadyReq = new SwitchReadyRequest(true);
            
            _popUpPub.Publish(popUpMsg);
            _switchReadyPub.Publish(switchReadyReq);
        }
    }
}