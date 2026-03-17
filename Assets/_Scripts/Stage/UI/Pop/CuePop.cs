using _Scripts.Lobby.UI.Pop;
using _Scripts.Messages.Stage;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.UI.Pop
{
    public class CuePop : PopBase
    {
        [SF] private RectTransform rectTr;
        [SF] private TextMeshProUGUI cueTxt;
        
        [Inject]
        private void Construct(ISubscriber<CueMessage> cueSub)
        {
            cueSub
                .Subscribe(HandleCueMessage)
                .AddTo(DisposableBag);
        }

        protected override void PopDown()
        {
            // 취소토큰으로 트윈 취소
            // 트윈 종료시키고 rect 값들 원복 시키기
            // 특히 shakePos 같은 거나 localScale 같은 거 원래 값으로 잘...
            base.PopDown();
        }

        private void HandleCueMessage(CueMessage msg)
        {
            switch (msg.Type)
            {
                case CueType.Start:
                    PlayStartCue(msg.Duration).Forget();
                    break;
                
                case CueType.End:
                    PlayEndCue(msg.Duration).Forget();
                    break;
                
                default:
                    break;
            }
        }

        private async UniTaskVoid PlayStartCue(float duration)
        {
            // Ready? 텍스트 갱신
            cueTxt.text = "Ready?";
            
            // Delay(1000), 아마 duration * 400f 만큼?
            await UniTask.Delay((int)(duration * 400f));

            // Time to Work! 텍스트 갱신
            cueTxt.text = "WorkTime!";

            // rectTr.localScale 0 -> 1 트윈, 아마 duration * 0.2f 만큼?
            await UniTask.Delay((int)(duration * 200f));
            // ShakePosition 루프 트윈

            // 서버에서 duration초 세고 popDown 메세지 보내줄 거임
        }
        
        private async UniTaskVoid PlayEndCue(float duration)
        {
            // Timeout! 텍스트 갱신
            cueTxt.text = "Timeout!";
            
            // rectTr.localScale 0 -> 1 트윈, 아마 duration * 0.2f 만큼?
            await UniTask.Delay((int)(duration * 200f));
            // ShakePosition 루프 트윈
            
            // 서버에서 duration초 세고 popDown 메세지 보내줄 거임
        }
    }
}