using System;
using System.Threading;
using _Scripts.Messages.StageResult;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Result.UI
{
    public class SkipVoteBoard : MonoBehaviour
    {
        [SF] private SkipIcon[] icons;

        private CancellationTokenSource _cts;
        
        [Inject]
        private void Construct(
            ISubscriber<SkipRespond> skipSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            skipSub
                .Subscribe(res => UpdateBoard(res).Forget())
                .AddTo(disposableBagBuilder);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTask Show(CancellationToken token)
        {
            this.gameObject.SetActive(true);
            await UniTask.Yield(token);
        }

        private void Hide()
        {
            this.gameObject.SetActive(false);
        }

        private async UniTaskVoid UpdateBoard(SkipRespond res)
        {
            var token = RefreshToken();
            
            int agreements = Mathf.Clamp(res.Agreements, 0, icons.Length);
            
            for (int i = 0; i < icons.Length; i++)
            {
                if (i >= res.VoterCount)
                {
                    icons[i].Hide();
                    continue;
                }
                
                bool isAgree = i < agreements;
                icons[i].SetIcon(isAgree).Show(token).Forget();    
            }
            
            if (!this.isActiveAndEnabled) await Show(token);
            // for (int i = 0; i < agreements; i++)
            // {
            //     icons[i].Show().Forget();
            // }
        }

        private CancellationToken RefreshToken()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }
}