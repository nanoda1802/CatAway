using System;
using System.Threading;
using _Scripts._Shared.Enums;
using _Scripts._Shared.UI.Pop;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Home.UI
{
    public class HomeView : MonoBehaviour
    {
        [SF] private Button createRoomBtn;
        [SF] private Button joinRoomBtn;
        
        private IPublisher<CreateRoomRequest> _createRoomPub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<DialogMessage> _dialogPub;
        
        [Inject]
        private void Construct(
            IPublisher<CreateRoomRequest> createRoomPub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<DialogMessage> dialogPub)
        {
            _createRoomPub = createRoomPub;
            _popUpPub = popUpPub;
            _dialogPub = dialogPub;
        }

        private void OnEnable()
        {
            createRoomBtn.onClick.RemoveAllListeners();
            joinRoomBtn.onClick.RemoveAllListeners();
            
            createRoomBtn.onClick.AddListener(OnCreate);
            joinRoomBtn.onClick.AddListener(OnJoin);
        }

        private void OnDisable()
        {
            createRoomBtn.onClick.RemoveAllListeners();
            joinRoomBtn.onClick.RemoveAllListeners();
        }

        private void OnCreate()
        {
            var cts = new CancellationTokenSource();
            
            var createRoomReq = new CreateRoomRequest(cts.Token);
            _createRoomPub.Publish(createRoomReq);
            
            var dialogMsg = new DialogMessage(
                "Create Room",
                string.Empty,
                string.Empty,
                DialogButtonType.Cancel,
                cts
            );
            
            SendDialogMessage(dialogMsg);
            SendPopUpMessage(typeof(DialogPop));
        }
        
        private void OnJoin()
        {
            var cts = new CancellationTokenSource();
            var dialogMsg = new DialogMessage(
                "Join Room",
                string.Empty,
                "Please type the code...",
                DialogButtonType.Submit | DialogButtonType.Cancel,
                cts
            );
            
            SendDialogMessage(dialogMsg);
            SendPopUpMessage(typeof(DialogPop));
        }

        private void SendDialogMessage(DialogMessage msg)
        {
            _dialogPub.Publish(msg);
        }

        private void SendPopUpMessage(Type popType)
        {
            var popUpMsg = new PopUpMessage(popType);
            _popUpPub.Publish(popUpMsg);
        }
    }
}