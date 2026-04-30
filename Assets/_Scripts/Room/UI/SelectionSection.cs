using System.Threading;
using _Scripts.Room._Data;
using _Scripts.Room._Messages;
using _Scripts.Stage._Enums;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Room.UI
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
        [SF] private StageThumbnailBoard stageThumbnailBoard;
        
        private RoomViewData _viewData;
        private IPublisher<SwitchModeRequest> _switchModPub;
        
        [Inject]
        private void Construct(
            RoomViewData viewData,
            IPublisher<SwitchModeRequest> switchModPub,
            ISubscriber<SwitchModeRespond> switchModSub)
        {
            _viewData = viewData;
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
            
            prevBtn.onClick.RemoveAllListeners();
            prevBtn.onClick.AddListener(stageThumbnailBoard.PublishLeftSign);
            
            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(stageThumbnailBoard.PublishRightSign);
        }

        public override async UniTask Hide(CancellationToken token)
        {
            modeBtn.onClick.RemoveAllListeners();
            prevBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.RemoveAllListeners();
            
            await UniTask.Yield(token);

            this.gameObject.SetActive(false);
        }

        public override void InitElements(InitRoomMessage msg)
        {
            ApplyTheme(msg.Mode);
            
            modeBtn.enabled = msg.IsHostPlayer;
            switchIconImg.enabled = msg.IsHostPlayer;
            nextBtn.gameObject.SetActive(msg.IsHostPlayer);
            prevBtn.gameObject.SetActive(msg.IsHostPlayer);
            
            stageThumbnailBoard.EnableSwipeBy(msg.IsHostPlayer);
            stageThumbnailBoard.InitThumbnails(msg.Mode, msg.StageIndex);
        }
        
        // public void InitElements(StageMode mode, int stageIndex, bool isHost)
        // {
        //     ApplyTheme(mode);
        //     
        //     modeBtn.enabled = isHost;
        //     switchIconImg.enabled = isHost;
        //     nextBtn.gameObject.SetActive(isHost);
        //     prevBtn.gameObject.SetActive(isHost);
        //     
        //     stageThumbnailBoard.EnableSwipeBy(isHost);
        //     stageThumbnailBoard.InitThumbnails(mode, stageIndex);
        // }

        private void OnClickMode()
        {
            var req = new SwitchModeRequest();
            _switchModPub.Publish(req);
        }

        private void ApplyTheme(StageMode mode)
        {
            selectionBgImg.color = modeBtnImg.color = _viewData.GetThemeColor(mode);
            modeBtnTxt.text = mode.ToString().ToUpper();
        }
        
        private async UniTaskVoid UpdateModeTheme(SwitchModeRespond msg)
        {
            var token = RefreshToken();
            
            await this.Hide(token);
            
            ApplyTheme(msg.Mode);
            stageThumbnailBoard.InitThumbnails(msg.Mode);
            
            await this.Show(token);
        }
    }
}