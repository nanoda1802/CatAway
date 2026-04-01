using System.Threading;
using _Scripts.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Room.UI
{
    public class ToastSection : SectionBase
    {
        [SF] private TextMeshProUGUI toastTxt;
        
        [Inject]
        private void Construct(ISubscriber<RoomToastMessage> noticeSub)
        {
            noticeSub
                .Subscribe(DisplayToast)
                .AddTo(DisposableBagBuilder);
        }
        
        private void DisplayToast(RoomToastMessage msg)
        {
            var token = RefreshToken();
            
            SetText(msg.Notice);
            
            Show(token).Forget();
        }

        public override async UniTask Show(CancellationToken token)
        {
            this.gameObject.SetActive(true);
            
            await UniTask.Delay(3000,cancellationToken:token);
            
            this.Hide(token).Forget();
        }

        public override async UniTask Hide(CancellationToken token)
        {
            await UniTask.Yield(token);
            
            this.gameObject.SetActive(false);
        }
        
        private void SetText(string toast)
        {
            toastTxt.SetText(toast);
        }
    }
}