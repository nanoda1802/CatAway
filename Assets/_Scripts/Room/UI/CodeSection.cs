using System.Threading;
using _Scripts.Room._Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Room.UI
{
    public class CodeSection : SectionBase
    {
        [Header("[ Components ]")]
        [SF] private TextMeshProUGUI codeTxt;
        [SF] private Button copyBtn;

        private IPublisher<RoomToastMessage> _noticePub;
        
        [Inject]
        private void Construct(IPublisher<RoomToastMessage> noticePub)
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

        public override void InitElements(InitRoomMessage msg)
        {
            codeTxt.SetText(msg.Code);
        }
        
        // public void InitElements(string code)
        // {
        //     codeTxt.SetText(code);
        // }

        private void OnClickCopy()
        {
            var code = codeTxt.text.Replace(" ","");
            GUIUtility.systemCopyBuffer = code;
            
            var msg = new RoomToastMessage("The code copy to clipboard.");
            _noticePub.Publish(msg);
        }
    }
}