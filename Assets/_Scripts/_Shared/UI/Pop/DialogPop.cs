using System.Threading;
using _Scripts._Helper;
using _Scripts._Messages.Shared;
using _Scripts._Shared.Enums;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using AYellowpaper.SerializedCollections;
using MessagePipe;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.UI.Pop
{
    public class DialogPop : PopBase
    {
        [Header("[ Components ]")]
        [SF] private RectTransform rectTr;
        [SF] private TextMeshProUGUI headerTxt;
        [SF] private TextMeshProUGUI msgTxt;
        [SF] private TMP_InputField inputField;
        [SF] private TextMeshProUGUI inputPlaceholderTxt;
        
        [Header("[ UI Elements ]")]
        [SF] private SerializedDictionary<DialogButtonType, Button> buttons;
        [SF] private Image waitingImg;
        
        [Header("[ Tween Settings ]")]
        [SF] private TweenSettings<float> popUpSettings;
        [SF] private TweenSettings<float> popDownSettings;
        
        private TweenHandler _tweenHandler;
        private IPublisher<RenameMessage> _renamePub;
        private IPublisher<LoadSceneMessage> _loadScenePub;
        private IPublisher<JoinRoomRequest> _joinRoomPub;
        private IPublisher<LeaveRoomMessage> _leaveRoomPub;

        private CancellationTokenSource _cts;
        
        [Inject]
        private void Construct(
            TweenHandler tweenHandler,
            IPublisher<RenameMessage> renamePub,
            IPublisher<LoadSceneMessage> loadScenePub,
            IPublisher<JoinRoomRequest> roomReqPub,
            IPublisher<LeaveRoomMessage> leaveReqPub,
            ISubscriber<DialogMessage> dialogSub,
            DisposableBagBuilder directorBagBuilder)
        {
            _tweenHandler = tweenHandler;
            _renamePub = renamePub;
            _loadScenePub = loadScenePub;
            _joinRoomPub = roomReqPub;
            _leaveRoomPub = leaveReqPub;
            
            dialogSub
                .Subscribe(SetContents)
                .AddTo(directorBagBuilder);
        }

        protected override void PopUp()
        {
            base.PopUp();
            CurSequence = _tweenHandler.ScaleY(ViewGroup,rectTr,popUpSettings,popUpSettings);
        }

        protected override void PopDown()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            if (CurSequence.isAlive) CurSequence.Complete();
            CurSequence = _tweenHandler.ScaleY(ViewGroup,rectTr,popDownSettings,popDownSettings,OnPopDownCompleted);
            
            foreach (var btn in buttons.Values)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners();
            }
        }

        private void OnPopDownCompleted()
        {
            base.PopDown();
        }

        #region OnClick 메서드
        private void OnClickReturn()
        {
            var msg = new LoadSceneMessage("Home", LoadSceneMode.Single);
            _loadScenePub.Publish(msg);
        }

        private void OnClickCancel()
        {
            PopDown();
        }
        
        private void OnClickSubmit()
        {
            var roomCode = inputField.text.Replace(" ","");
            // if (string.IsNullOrEmpty(roomCode)) return;

            var req = new JoinRoomRequest(roomCode, _cts.Token);
            _joinRoomPub.Publish(req);
            
            buttons[DialogButtonType.Submit].gameObject.SetActive(false);
            inputField.text = string.Empty;
            inputField.gameObject.SetActive(false);
            waitingImg.gameObject.SetActive(true);
        }

        private void OnClickLeave()
        {
            PopDown();
            
            var req = new LeaveRoomMessage();
            _leaveRoomPub.Publish(req);
        }

        private void OnClickRetry()
        {
            inputField.gameObject.SetActive(true);
            msgTxt.gameObject.SetActive(false);
            waitingImg.gameObject.SetActive(false);
            buttons[DialogButtonType.Retry].gameObject.SetActive(false);
            buttons[DialogButtonType.Submit].gameObject.SetActive(true);
        }

        private void OnClickRename()
        {
            var nickname = inputField.text.Replace(" ","");
            if (string.IsNullOrEmpty(nickname)) return;

            if (nickname.Length > 7)
            {
                inputField.text = string.Empty;
                inputPlaceholderTxt.text = "Keep it under 7 chars!";
                return;
            }

            var msg = new RenameMessage(nickname);
            _renamePub.Publish(msg);
            
            PopDown();
        }

        #endregion
        
        private void SetContents(DialogMessage msg)
        {
            headerTxt.SetText(msg.Header);
            msgTxt.gameObject.SetActive(msg.HasText);
            inputField.gameObject.SetActive(msg.ShowInputField);
            waitingImg.gameObject.SetActive(msg is { HasText: false, ShowInputField: false });

            if (msg.HasText)
            {
                msgTxt.SetText(msg.Text);
            }
            
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
                            btn.onClick.AddListener(OnClickReturn); break;
                        case DialogButtonType.Submit:
                            btn.onClick.AddListener(OnClickSubmit); break;
                        case DialogButtonType.Cancel:
                            btn.onClick.AddListener(OnClickCancel); break;
                        case DialogButtonType.Leave:
                            btn.onClick.AddListener(OnClickLeave); break;
                        case DialogButtonType.Retry:
                            btn.onClick.AddListener(OnClickRetry); break;
                        case DialogButtonType.Rename:
                            btn.onClick.AddListener(OnClickRename); break;
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