using System;
using System.Threading;
using _Scripts.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Random = UnityEngine.Random;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Room
{
    public class CodeSection : SectionBase
    {
        [Header("[ Components ]")]
        [SF] private TextMeshProUGUI codeTxt;
        [SF] private Button copyBtn;

        private IPublisher<NoticeMessage> _noticePub;
        
        [Inject]
        private void Construct(IPublisher<NoticeMessage> noticePub)
        {
            _noticePub = noticePub;
        }

        public override async UniTask Show(CancellationToken token)
        {
            this.gameObject.SetActive(true);
            
            await UniTask.Yield(token);
            
            copyBtn.onClick.RemoveAllListeners();
            copyBtn.onClick.AddListener(OnClickCopy);
        }

        public override async UniTask Hide(CancellationToken token)
        {
            copyBtn.onClick.RemoveAllListeners();
            
            await UniTask.Yield(token);
            
            this.gameObject.SetActive(false);
            
            await UniTask.Delay(1000,cancellationToken:token);
        }

        public void InitElements(string code)
        {
            codeTxt.text = code;
        }

        private void OnClickCopy()
        {
            var code = codeTxt.text.Replace(" ","");
            GUIUtility.systemCopyBuffer = code;
            
            // 모바일은 다르대 
            // UniCliBoard 라는 패키지로 쉽게 할 수 있다는데?

            var msg = new NoticeMessage("The code copy to clipboard.");
            _noticePub.Publish(msg);
        }
    }
}