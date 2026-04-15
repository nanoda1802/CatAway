using System.Threading;
using _Scripts.Result._Messages;
using _Scripts.Stage._Enums;
using _Scripts.Stage.UI.Board;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Result.UI
{
    public class ResultBoard : MonoBehaviour
    {
        [SF] private Team team;
        [SF] private RectTransform rectTr;
        [SF] private string defaultTestFormat = "{0}";
        
        [Header("[ Card Elements ]")]
        [SF] private Image victoryImg;
        [SF] private TextMeshProUGUI scoreTxt;
        [SF] private TextMeshProUGUI bestComboTxt;
        [SF] private TextMeshProUGUI orderDeliveredTxt;
        [SF] private TextMeshProUGUI totalTxt;

        private TextMeshProUGUI[] _texts;
        private CancellationTokenSource _cts;

        [Inject]
        private void Construct(
            ISubscriber<ResultBoardMessage> resultSub,
            ISubscriber<SkipRequest> skipSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            Init();
            
            resultSub
                .Subscribe(msg =>
                {
                    SetValues(msg);
                    ShowCard(msg.IsWin).Forget();
                }, new TeamMessageFilter<ResultBoardMessage>(team))
                .AddTo(disposableBagBuilder);
            
            skipSub
                .Subscribe(SkipTween)
                .AddTo(disposableBagBuilder);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void Init()
        {
            _texts = new[] { scoreTxt, bestComboTxt, orderDeliveredTxt, totalTxt };
            
            victoryImg?.gameObject.SetActive(false);
            scoreTxt?.gameObject.SetActive(false);
            bestComboTxt?.gameObject.SetActive(false);
            orderDeliveredTxt?.gameObject.SetActive(false);
            totalTxt?.gameObject.SetActive(false);
        }
        
        private void SetValues(ResultBoardMessage msg)
        {
            // [임시] 텍스트 포맷 데이터로...
            scoreTxt.SetText(defaultTestFormat, msg.Score);
            bestComboTxt.SetText(defaultTestFormat, msg.BestCombo);
            orderDeliveredTxt.SetText("{0}%",(int)(msg.DeliveredRatio * 100));
            totalTxt.SetText(defaultTestFormat, msg.Income);
        }

        private async UniTaskVoid ShowCard(bool isWin)
        {
            var token = RefreshToken();
            this.gameObject.SetActive(true);
            
            await UniTask.Delay(1000, cancellationToken:token).SuppressCancellationThrow();
            
            foreach (var text in _texts)
            {
                await TweenText(text, 1000, token);
            }

            if (isWin) await TweenVictory(1000, token);
        }

        private async UniTask HideCard()
        {
            var token = RefreshToken();
            
            await UniTask.Delay(1000, cancellationToken:token);
            
            this.gameObject.SetActive(false);
        }

        private void SkipTween(SkipRequest req)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTask TweenText(TextMeshProUGUI text, float duration, CancellationToken token)
        {
            text.gameObject.SetActive(true);
            await UniTask.Delay((int)duration,cancellationToken:token).SuppressCancellationThrow();
            // 카운터 트윈
        }

        private async UniTask TweenVictory(float duration, CancellationToken token)
        {
            victoryImg?.gameObject.SetActive(true);
            await UniTask.Delay((int)duration,cancellationToken:token).SuppressCancellationThrow();
            // 이미지는 크기 트윈, 도장 찍는 느낌 (크게 시작해서 작아지는데, punch 느낌)
            // totalTxt도 크기 트윈, 살짝 커졌다 돌아오는
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