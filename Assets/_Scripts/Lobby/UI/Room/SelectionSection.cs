using System;
using System.Threading;
using _Scripts.Lobby.UI.Messages;
using _Scripts.Lobby.UI.Messages.Room;
using _Scripts.Stage;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Room
{
    public class SelectionSection : SectionBase
    {
        [Header("[ Mode Switch ]")]
        [SF] private Button modeBtn;
        [SF] private Image modeBtnImg;
        [SF] private TextMeshProUGUI modeBtnTxt;
        [SF] private Image switchIconImg;
        
        [Header("[ Stage Switch ]")]
        [SF] private Image selectionBgImg;
        [SF] private Image stageImg;
        [SF] private Button prevBtn;
        [SF] private Button nextBtn;

        private RoomViewUiData _data;
        private IPublisher<SwitchModeRequest> _switchModPub;
        
        [Inject]
        private void Construct(
            RoomViewUiData data,
            IPublisher<SwitchModeRequest> switchModPub,
            ISubscriber<SwitchModeRespond> switchModSub)
        {
            _data = data;
            _switchModPub = switchModPub;
            
            switchModSub
                .Subscribe(msg => UpdateModeTheme(msg).Forget())
                .AddTo(DisposableBagBuilder);
        }

        public override async UniTask Show(CancellationToken token)
        {
            this.gameObject.SetActive(true);
            
            await UniTask.Yield(token);
            
            modeBtn.onClick.RemoveAllListeners();
            modeBtn.onClick.AddListener(OnClickMode);
        }

        public override async UniTask Hide(CancellationToken token)
        {
            modeBtn.onClick.RemoveAllListeners();
            
            await UniTask.Yield(token);

            this.gameObject.SetActive(false);
        }

        public void InitElements(StageMode mode, bool isHost)
        {
            ApplyTheme(mode);
            
            modeBtn.enabled = isHost;
            switchIconImg.enabled = isHost;
            nextBtn.gameObject.SetActive(isHost);
            prevBtn.gameObject.SetActive(isHost);
        }

        private void OnClickMode()
        {
            _switchModPub.Publish(new SwitchModeRequest());
        }

        private void ApplyTheme(StageMode mode)
        {
            selectionBgImg.color = modeBtnImg.color = _data.GetThemeColor(mode);
            modeBtnTxt.text = mode.ToString().ToUpper();
        }
        
        private async UniTaskVoid UpdateModeTheme(SwitchModeRespond msg)
        {
            var token = RefreshToken();
            
            await this.Hide(token);
            
            ApplyTheme(msg.Mode);
            
            await this.Show(token);
        }
    }
}