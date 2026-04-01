using System;
using System.Text;
using _Scripts._Helper;
using _Scripts.Messages.Stage;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Data.UI;
using MessagePipe;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.UI.Board.Timer
{
    public class TimerBoard : MonoBehaviour, IBoard<float>
    {
        // Data
        private StageData _stageData;
        private BoardUiData _boardUiData;
        // Dependency
        private TweenHandler _tweenHandler;
        // Component
        [SF] private Image fillBarImg;
        [SF] private TextMeshProUGUI timerTxt;
        // Caching
        private readonly StringBuilder _sb = new StringBuilder();
        private int _prevSecond = -1;
        private Sequence _curSeq;

        private bool IsTimerDirty(int newSecond) => newSecond != _prevSecond;
        private bool IsFillBarDirty(float newFillAmount) => Mathf.Abs(newFillAmount - fillBarImg.fillAmount) > _boardUiData.FillBarDirtyThreshold;
        private bool CheckFever(float remainingTime) => remainingTime < _boardUiData.FeverTime;

        [Inject]
        private void Construct(
            StageData stageData,
            BoardUiData boardUiData,
            TweenHandler tweenHandler,
            ISubscriber<float> timerSub,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _stageData = stageData;
            _boardUiData = boardUiData;
            _tweenHandler = tweenHandler;
            
            timerSub
                .Subscribe(Apply)
                .AddTo(disposableBagBuilder);

            endSub
                .Subscribe(msg => fillBarImg.fillAmount = 0)
                .AddTo(disposableBagBuilder);
        }
        
        public void Apply(float data)
        {
            UpdateFillBar(data / _stageData.Duration);

            (int m, int s) = Convert(data);
            
            UpdateTimer(m,s, CheckFever(data));
        }

        private void UpdateFillBar(float ratio)
        {
            if (!IsFillBarDirty(ratio)) return;
            
            // fillBarImg.fillAmount = Mathf.Lerp(0,1,ratio);
            fillBarImg.fillAmount = ratio;
        }

        private void UpdateTimer(int minute, int second, bool isFever)
        {
            if (!IsTimerDirty(second)) return;
            
            _sb.Clear();
            _sb.AppendFormat(_boardUiData.TimerFormat, minute, second);
            timerTxt.SetText(_sb);

            if (isFever)
            {
                if (_curSeq.isAlive) _curSeq.Complete();
                _curSeq = _tweenHandler.PunchScaleWithColor(timerTxt, _boardUiData.TimerColorSettings, _boardUiData.TimerPunchSettings);
            }

            _prevSecond = second;
        }

        private (int, int) Convert(float time)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            return (timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}