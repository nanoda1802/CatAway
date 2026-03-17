using System.Threading;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using AYellowpaper.SerializedCollections;
using MessagePipe;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Pop
{
    public class DialogPop : PopBase
    {
        [Header("[ Components ]")]
        [SF] private TextMeshProUGUI headerTxt;
        [SF] private TextMeshProUGUI msgTxt;
        [SF] private TMP_InputField inputField;
        [SF] private TextMeshProUGUI inputPlaceholderTxt;
        
        [Header("[ UI Elements ]")]
        [SF] private SerializedDictionary<DialogButtonType, Button> buttons;
        
        private IPublisher<JoinRoomRequest> _joinRoomPub;
        private IPublisher<LeaveRoomRequest> _leaveRoomPub;
        private IPublisher<ChangeViewRequest> _changeViewPub;

        private CancellationTokenSource _cts;
        
        [Inject]
        private void Construct(
            IPublisher<JoinRoomRequest> roomReqPub,
            IPublisher<LeaveRoomRequest> leaveReqPub,
            IPublisher<ChangeViewRequest> changeViewPub,
            ISubscriber<DialogMessage> dialogSub)
        {
            _joinRoomPub = roomReqPub;
            _leaveRoomPub = leaveReqPub;
            _changeViewPub = changeViewPub;
            
            dialogSub
                .Subscribe(SetContents)
                .AddTo(DisposableBag);
        }

        protected override void PopUp()
        {
            base.PopUp();
        }

        protected override void PopDown()
        {
            foreach (var btn in buttons.Values)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners();
            }
            
            base.PopDown();
        }

        #region OnClick 메서드
        private void OnClickReturnBtn()
        {
            var req = new ChangeViewRequest(typeof(TitleView));
            _changeViewPub.Publish(req);
        }

        private void OnClickCancelBtn()
        {
            PopDown();
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
        
        private void OnClickSubmitBtn()
        {
            var roomCode = inputField.text.Replace(" ","");
            var req = new JoinRoomRequest(roomCode, _cts.Token);
            _joinRoomPub.Publish(req);
        }

        private void OnClickConfirmBtn()
        {
            PopDown();
            
            var clientId = NetworkManager.Singleton.LocalClientId;
            var req = new LeaveRoomRequest(clientId);
            _leaveRoomPub.Publish(req);
        }
        #endregion
        
        private void SetContents(DialogMessage msg)
        {
            headerTxt.text = msg.Header;
            msgTxt.text = msg.Text;
            inputField.gameObject.SetActive(msg.ShowInputField);

            if (msg.ShowInputField)
            {
                inputField.text = string.Empty;
                inputPlaceholderTxt.text = msg.InputPlaceholder;
            }

            if (msg.NeedCancellation)
            {
                RefreshTokenSource(msg.Cts);
            }

            foreach (var btnType in msg.ActiveBtnTypes)
            {
                if (buttons.TryGetValue(btnType, out var btn))
                {
                    btn.gameObject.SetActive(true);
                    
                    btn.onClick.RemoveAllListeners();
                    
                    switch (btnType)
                    {
                        case DialogButtonType.Return:
                            btn.onClick.AddListener(OnClickReturnBtn); break;
                        case DialogButtonType.Submit:
                            btn.onClick.AddListener(OnClickSubmitBtn); break;
                        case DialogButtonType.Cancel:
                            btn.onClick.AddListener(OnClickCancelBtn); break;
                        case DialogButtonType.Confirm:
                            btn.onClick.AddListener(OnClickConfirmBtn); break;
                        default:
                            break;
                    }
                }
            }
        }

        private void RefreshTokenSource(CancellationTokenSource cts)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = cts;
        }
    }
}