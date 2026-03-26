using _Scripts._Shared.Enums;
using _Scripts.Messages.Room;
using MessagePipe;
using UnityEngine;

namespace _Scripts._Shared.UI.QuickMenu.ButtonActions
{
    public class SettingsAction : IButtonAction<QuickMenuButtonType>
    {
        public QuickMenuButtonType ButtonType => QuickMenuButtonType.Setting;

        private readonly IPublisher<SwitchReadyRequest> _switchReadyPub;
        
        public SettingsAction(IPublisher<SwitchReadyRequest> switchReadyPub)
        {
            _switchReadyPub = switchReadyPub;
        }

        public void OnClick()
        {
            Debug.Log("Pop Up Settings");
            var req = new SwitchReadyRequest(true);
            _switchReadyPub.Publish(req);
        }
    }
}