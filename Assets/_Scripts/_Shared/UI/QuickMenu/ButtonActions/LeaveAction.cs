using _Scripts._Shared.Enums;
using _Scripts._Shared.UI.Pop;
using _Scripts.Messages;
using MessagePipe;

namespace _Scripts._Shared.UI.QuickMenu.ButtonActions
{
    public class LeaveAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Leave;

        private readonly IPublisher<DialogMessage> _dialogPub;
        private readonly IPublisher<PopUpMessage> _popUpPub;
        
        public LeaveAction(
            IPublisher<DialogMessage> dialogPub,
            IPublisher<PopUpMessage> popUpPub)
        {
            _dialogPub = dialogPub;
            _popUpPub = popUpPub;
        }

        public void OnClick()
        {
            var popUpMsg = new PopUpMessage(typeof(DialogPop));
            var dialogMsg = new DialogMessage(
                "Leave Room",
                "Return to Home?",
                string.Empty,
                DialogButtonType.Leave | DialogButtonType.Cancel
            );
            
            _dialogPub.Publish(dialogMsg);
            _popUpPub.Publish(popUpMsg);
        }
    }
}