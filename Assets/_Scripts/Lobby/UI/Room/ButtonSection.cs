using System;
using System.Threading;
using _Scripts.Messages.Room;
using _Scripts.Stage;
using _Scripts.Stage.Data;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using VContainer;
using SF= UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Room
{
    public class ButtonSection : SectionBase
    {
        [Header("[ Ready Button ]")]
        [SF] private Button readyBtn;
        [SF] private Image readyBtnImg;
        [SF] private TextMeshProUGUI readyBtnTxt;
        [SF] private Image stateIconImg;
        [SF] private Image coverImg;
        
        [Header("[ Start Button ]")]
        [SF] private Button startBtn;
        [SF] private Image startBtnImg;
        [SF] private TextMeshProUGUI startBtnTxt;
        [SF] private Image lockIconImg;

        private RoomViewUiData _data;

        private IPublisher<LoadStageMessage> _startStagePub;
        private IPublisher<SwitchReadyRequest> _switchReadyPub;
        
        [Inject]
        private void Construct(
            RoomViewUiData data,
            IPublisher<LoadStageMessage> startStagePub,
            IPublisher<SwitchReadyRequest> switchReadyPub,
            ISubscriber<SwitchModeRespond> switchModeSub,
            ISubscriber<SwitchReadyRespond> switchReadySub,
            ISubscriber<SwitchStartMessage> switchStartSub)
        {
            _data = data;
            _startStagePub = startStagePub;
            _switchReadyPub = switchReadyPub;
            
            switchReadySub
                .Subscribe(UpdateReadyButton)
                .AddTo(DisposableBagBuilder);
            
            switchStartSub
                .Subscribe(UpdateStartButton)
                .AddTo(DisposableBagBuilder);
            
            switchModeSub
                .Subscribe(msg => UpdateModeTheme(msg).Forget())
                .AddTo(DisposableBagBuilder);
        }
        
        public override async UniTask Show(CancellationToken token)
        {
            this.gameObject.SetActive(true);
            
            await UniTask.Yield(token);
            
            readyBtn.onClick.RemoveAllListeners();
            readyBtn.onClick.AddListener(OnClickReadyBtn);
            
            startBtn.onClick.RemoveAllListeners();
            startBtn.onClick.AddListener(OnClickStartBtn);
        }

        public override async UniTask Hide(CancellationToken token)
        {
            readyBtn.onClick.RemoveAllListeners();
            startBtn.onClick.RemoveAllListeners();
            
            await UniTask.Yield(token);
            
            this.gameObject.SetActive(false);
        }

        private void ApplyTheme(StageMode mode)
        {
            readyBtnImg.color = startBtnImg.color = _data.GetThemeColor(mode);
        }
        
        public void InitElements(StageMode mode, bool isHost)
        {
            ApplyTheme(mode);
            
            readyBtn.gameObject.SetActive(!isHost);
            startBtn.gameObject.SetActive(isHost);
        }

        private async UniTaskVoid UpdateModeTheme(SwitchModeRespond msg)
        {
            var token = RefreshToken();
            
            await this.Hide(token);
            
            ApplyTheme(msg.Mode);
            
            await this.Show(token);
        }

        private void UpdateReadyButton(SwitchReadyRespond res)
        {
            if (!res.ToMe) return;
            
            stateIconImg.sprite = res.IsReady
                    ? _data.CheckIcon 
                    : _data.CrossIcon;
            
            stateIconImg.color = res.IsReady 
                    ? _data.CheckColor 
                    : _data.CrossColor;
            
            coverImg.enabled = !res.IsReady;
        }

        private void UpdateStartButton(SwitchStartMessage msg)
        {
            startBtn.interactable = msg.CanStart;
            lockIconImg.enabled = !msg.CanStart;
        }

        private void OnClickReadyBtn()
        {
            var req = new SwitchReadyRequest();
            _switchReadyPub.Publish(req);
        }
        
        private void OnClickStartBtn()
        {
            var msg = new LoadStageMessage();
            _startStagePub.Publish(msg);
        }
    }
}