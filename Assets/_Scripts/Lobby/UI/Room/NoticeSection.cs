using System;
using System.Threading;
using _Scripts.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Room
{
    public class NoticeSection : SectionBase
    {
        [SF] private TextMeshProUGUI noticeTxt;
        
        [Inject]
        private void Construct(ISubscriber<NoticeMessage> noticeSub)
        {
            noticeSub
                .Subscribe(NotifyMessage)
                .AddTo(DisposableBagBuilder);
        }

        private void NotifyMessage(NoticeMessage msg)
        {
            var token = RefreshToken();
            
            UpdateNotice(msg.Notice);
            
            Show(token).Forget();
        }

        public override async UniTask Show(CancellationToken token)
        {
            this.gameObject.SetActive(true);
            
            await UniTask.Yield(token);
            await UniTask.Delay(3000,cancellationToken:token);
            
            this.Hide(token).Forget();
        }

        public override async UniTask Hide(CancellationToken token)
        {
            await UniTask.Yield(token);
            
            this.gameObject.SetActive(false);
        }
        
        private void UpdateNotice(string notice)
        {
            noticeTxt.text = notice;
        }
    }
}