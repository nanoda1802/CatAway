using System;
using System.Threading;
using _Scripts._Helper;
using _Scripts._Shared.Enums;
using _Scripts._Shared.UI.Pop;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using _Scripts.Scene_Home.Data;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Random = UnityEngine.Random;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Home.UI
{
    public class HomeView : MonoBehaviour
    {
        [SF] private Button createRoomBtn;
        [SF] private Button joinRoomBtn;
        [SF] private RectTransform createBtnRectTr;
        [SF] private RectTransform joinBtnRectTr;
        
        private TweenHandler _tweenHandler;
        private HomeViewData _data;
        
        private IPublisher<CreateRoomRequest> _createRoomPub;
        private IPublisher<PopUpMessage> _popUpPub;
        private IPublisher<DialogMessage> _dialogPub;

        private bool FiftyToFifty => Random.Range(0, 10) > 5;
        
        [Inject]
        private void Construct(
            TweenHandler tweenHandler,
            HomeViewData data,
            IPublisher<CreateRoomRequest> createRoomPub,
            IPublisher<PopUpMessage> popUpPub,
            IPublisher<DialogMessage> dialogPub)
        {
            _tweenHandler = tweenHandler;
            _data = data;
            
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
            
            ShakeButton().Forget();
        }

        private void OnDisable()
        {
            createRoomBtn.onClick.RemoveAllListeners();
            joinRoomBtn.onClick.RemoveAllListeners();
        }

        private async UniTaskVoid ShakeButton()
        {
            while (this.gameObject.activeSelf)
            {
                RectTransform curTarget = FiftyToFifty ? joinBtnRectTr : createBtnRectTr;
                
                await UniTask.Delay(_data.ShakeInterval, cancellationToken:this.destroyCancellationToken);
                await _tweenHandler.Shake(curTarget, _data.ScaleSetting, _data.RotSetting).WithCancellation(this.destroyCancellationToken);
            }
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