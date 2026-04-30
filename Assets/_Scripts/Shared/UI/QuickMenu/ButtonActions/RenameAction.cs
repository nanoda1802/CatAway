using _Scripts.Room._Messages;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI.Pop;
using MessagePipe;
using UnityEngine;

namespace _Scripts.Shared.UI.QuickMenu.ButtonActions
{
    public class RenameAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Rename;
        
        private readonly IPublisher<DialogMessage> _dialogPub;
        private readonly IPublisher<PopUpMessage> _popUpPub;
        private readonly IPublisher<SwitchReadyRequest> _switchReadyPub;

        private float _lastClickTime;
        private bool IsValidClick => Time.time - _lastClickTime >= 1f;

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
            if (!IsValidClick) return;
            _lastClickTime = Time.time;
            
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