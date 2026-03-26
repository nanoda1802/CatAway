using System;
using _Scripts._Shared.Enums;
using _Scripts._Shared.UI.Pop;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using MessagePipe;

namespace _Scripts._Shared.UI.QuickMenu.ButtonActions
{
    public class RenameAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Rename;
        
        private readonly IPublisher<DialogMessage> _dialogPub;
        private readonly IPublisher<PopUpMessage> _popUpPub;
        private readonly IPublisher<SwitchReadyRequest> _switchReadyPub;


        public RenameAction(
            IPublisher<DialogMessage> dialogPub,
            IPublisher<PopUpMessage> popupPub,
            IPublisher<SwitchReadyRequest> switchReadyPub)
        {
            _dialogPub = dialogPub;
            _popUpPub = popupPub;
            _switchReadyPub = switchReadyPub;
        }

        public void OnClick()
        {
            var popUpMsg = new PopUpMessage(typeof(DialogPop));
            var dialogMsg = new DialogMessage(
                    "Rename",
                    string.Empty,
                    "Type your nickname!",
                    DialogButtonType.Rename | DialogButtonType.Cancel
                );
            
            _popUpPub.Publish(popUpMsg);
            _dialogPub.Publish(dialogMsg);
            _switchReadyPub.Publish(new SwitchReadyRequest(true));
        }
    }
}