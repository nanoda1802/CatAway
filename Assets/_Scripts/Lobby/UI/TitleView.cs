using System;
using System.Threading;
using _Scripts.Lobby.UI.Messages;
using _Scripts.Lobby.UI.Messages.Room;
using _Scripts.Lobby.UI.Pop;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI
{
    public class TitleView : MonoBehaviour, IView
    {
        [SF] private CanvasGroup canvasGroup;
        [SF] private Button createRoomBtn;
        [SF] private Button joinRoomBtn;
        [SF] private QuickMenuType quickMenuType  = QuickMenuType.Customize | QuickMenuType.Tutorial | QuickMenuType.Setting | QuickMenuType.Exit;
        
        private IPublisher<CreateRoomRequest> _createRoomPub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<DialogMessage> _dialogPub;
        
        private CancellationTokenSource _cts;
        
        public QuickMenuType RequiredQuickMenu => quickMenuType;
        
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
        
        public async UniTask Activate(CancellationToken ct = default)
        {
            await UniTask.Yield(ct);

            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            
            createRoomBtn.onClick.RemoveAllListeners();
            createRoomBtn.onClick.AddListener(OnClickCreate);
            joinRoomBtn.onClick.RemoveAllListeners();
            joinRoomBtn.onClick.AddListener(OnClickJoin);
        }

        public async UniTask Deactivate(CancellationToken ct = default)
        {
            await UniTask.Yield(ct);

            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            
            createRoomBtn.onClick.RemoveAllListeners();
            joinRoomBtn.onClick.RemoveAllListeners();
        }

        private void OnClickCreate()
        {
            var cts = new CancellationTokenSource();
            
            _dialogPub.Publish(new DialogMessage(
                "Create Room",
                "Waiting For Respond...",
                string.Empty, 
                DialogButtonType.Cancel,
                cts
                ));
            
            _popUpPub.Publish(new PopUpMessage(typeof(DialogPop)));

            _createRoomPub.Publish(new CreateRoomRequest(cts.Token));
        }

        private void OnClickJoin()
        {
            var cts = new CancellationTokenSource();
            
            _dialogPub.Publish(new DialogMessage(
                "Join Room",
                string.Empty, 
                "Please type the code...",
                DialogButtonType.Submit | DialogButtonType.Cancel,
                cts
            ));
            
            _popUpPub.Publish(new PopUpMessage(typeof(DialogPop)));
        }
    }
}